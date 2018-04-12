using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace TextRankCalc
{
	public class Program
	{
		private const string EXCHANGE_NAME = "backend-api";
		private const string EXCHANGE_TYPE = ExchangeType.Fanout;
		private const string RESULT_ID_PREFIX = "TextRank_";

		private static HashSet<char> VOVELS
			= new HashSet<char>() { 'a', 'e', 'i', 'u', 'y' };
		private static HashSet<char> CONSONANTS
			= new HashSet<char>() { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };

		private static ConnectionMultiplexer RadisConnection => ConnectionMultiplexer.Connect("localhost");
		private static IDatabase Database => RadisConnection.GetDatabase();

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
					channel.ExchangeDeclare(EXCHANGE_NAME, EXCHANGE_TYPE);

					var queueName = channel.QueueDeclare().QueueName;
					channel.QueueBind(queueName, EXCHANGE_NAME, routingKey: "");

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var messageId = Encoding.UTF8.GetString(body);
						var message = Database.StringGet(messageId);
						Console.WriteLine("Received message: {0}", message);

						var result = CalcResult(message);
						Database.StringSet(RESULT_ID_PREFIX + messageId, result);
						Console.WriteLine("Result for message '{0}': {1}", message, result);
					};

					channel.BasicConsume(queueName, true, consumer);

					Console.WriteLine("Waiting... Press any key to exit");
					Console.ReadLine();
				}
			}
		}

		private static string CalcResult(string message)
		{
			if (message == null)
			{
				return "Message is null";
			}

			int vovels = 0;
			int consonants = 0;

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

			var result = "Vovels: {0}. Consonants: {1}. Ratio (V/C): {2}.";
			var ratio = "Infinite";

			if (consonants != 0)
			{
				ratio = ((float)vovels / consonants).ToString();
			}

			return String.Format(result, vovels, consonants, ratio);
		}
	}
}
