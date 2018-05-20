using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VowelsConsCounter
{
	public class Program
	{
		private const string EXCHANGE = "vowel-cons-api";
		private const string EXCHANGE_TYPE = ExchangeType.Direct;
		private const string QUEUE = "calculate-vowels-queue";
		private const string ROUTE = "calculate-vowels-count";
		private const string ROUTE_VOWELS_RATE = "calculate-vowels-rate";
		private const string SEPARATOR = "|";

		private static HashSet<char> VOVELS
			= new HashSet<char>() { 'a', 'e', 'i', 'u', 'y' };
		private static HashSet<char> CONSONANTS
			= new HashSet<char>() { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };

		private static ConnectionMultiplexer RedisConnection =>
			ConnectionMultiplexer.Connect("localhost");
		private const int DATABASE_COUNT = 16;

		public static void Main(string[] args)
		{
			try
			{
				StartMessageListener();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		private static void StartMessageListener()
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					channel.ExchangeDeclare(EXCHANGE, EXCHANGE_TYPE);

					// To receive messages and calculate vowels
					channel.QueueDeclare(QUEUE, exclusive: false);
					channel.QueueBind(QUEUE, EXCHANGE, ROUTE);

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var textId = Encoding.UTF8.GetString(body);
						Console.WriteLine("Received message: {0}", textId);

						var text = GetDatabase(textId).StringGet(textId);
						var result = textId + SEPARATOR + CalcResult(text);
						var resultMessage = Encoding.UTF8.GetBytes(result);
						channel.BasicPublish(EXCHANGE, ROUTE_VOWELS_RATE, body: resultMessage);
						Console.WriteLine("Publish message: {0}", result);
					};

					channel.BasicConsume(QUEUE, true, consumer);

					Console.WriteLine("Waiting... Press any key to exit");
					Console.ReadLine();
				}
			}
		}

		private static string CalcResult(string message)
		{
			int vovels = 0;
			int consonants = 0;

			if (message != null)
			{
				foreach (char character in message)
				{
					char ch = Char.ToLower(character);

					if (VOVELS.Contains(ch))
					{
						++vovels;
					}
					else if (CONSONANTS.Contains(ch))
					{
						++consonants;
					}
				}
			}

			return String.Format("{0}{1}{2}", vovels, SEPARATOR, consonants);
		}

		private static IDatabase GetDatabase(string key)
		{
			int databaseId = 0;

			for (int i = 0; i < key.Length; ++i)
			{
				databaseId += key[i];
			}

			databaseId %= DATABASE_COUNT;
			Console.WriteLine("Key: {0}, Redis database: {1}", key, databaseId);

			return RedisConnection.GetDatabase(databaseId);
		}
	}
}
