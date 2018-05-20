using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;

namespace TextRankCalc
{
	public class Program
	{
		private const string EXCHANGE = "vowel-cons-api";
		private const string EXCHANGE_TYPE = ExchangeType.Direct;
		private const string QUEUE = "calculate-rate-queue";
		private const string ROUTE = "calculate-vowels-rate";
		private const string SEPARATOR = "|";
		private const string RESULT_ID_PREFIX = "TextRank_";

		private const string RESULT_EXCHANGE_TYPE = ExchangeType.Fanout;
		private const string RESULT_EXCHANGE = "result-api";

		private static ConnectionMultiplexer RedisConnection => ConnectionMultiplexer.Connect("localhost");
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
					DeclareQueue(channel);

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var message = Encoding.UTF8.GetString(body);
						Console.WriteLine("Received message: {0}", message);

						var data = message.Split(SEPARATOR);
						var result = CalcResult(data[1], data[2]);

						var textId = data[0];
						GetDatabase(textId, out int dbIndex)
						.StringSet(RESULT_ID_PREFIX + data[0], result);
						Console.WriteLine("Result for '{0}': {1}", textId, result);
						Console.WriteLine("Save result to database {0}", dbIndex);

						PostResult(channel, result);
					};

					channel.BasicConsume(QUEUE, true, consumer);

					Console.WriteLine("Waiting... Press any key to exit");
					Console.ReadLine();
				}
			}
		}

		private static void DeclareQueue(IModel channel)
		{
			channel.ExchangeDeclare(EXCHANGE, EXCHANGE_TYPE);
			channel.QueueDeclare(QUEUE, exclusive: false);
			channel.QueueBind(QUEUE, EXCHANGE, ROUTE);

			channel.ExchangeDeclare(RESULT_EXCHANGE, RESULT_EXCHANGE_TYPE);
			var resultQueue = channel.QueueDeclare().QueueName;
			channel.QueueBind(resultQueue, RESULT_EXCHANGE, routingKey: "");
		}

		private static string CalcResult(string vowelCount, string consCount)
		{
			try
			{
				var vowels = int.Parse(vowelCount);
				var consonans = int.Parse(consCount);

				if (consonans == 0)
				{
					return "Infinite";
				}

				return ((float)vowels / consonans).ToString();
			}
			catch (Exception)
			{
				return "null";
			}
		}

		private static IDatabase GetDatabase(string key, out int dbIndex)
		{
			int databaseId = 0;

			for (int i = 0; i < key.Length; ++i)
			{
				databaseId += key[i];
			}

			dbIndex = databaseId % DATABASE_COUNT;
			Console.WriteLine("Key: {0}, Redis database: {1}", key, databaseId);

			return RedisConnection.GetDatabase(dbIndex);
		}

		private static void PostResult(IModel channel, string result)
		{
			var resultMessage = Encoding.UTF8.GetBytes(result);
			channel.BasicPublish(RESULT_EXCHANGE, "", null, resultMessage);
			Console.WriteLine("Post message to result queue: {0}", result);
		}
	}
}
