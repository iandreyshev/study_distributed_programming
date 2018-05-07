using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;

namespace TextListener
{
	public class Program
	{
		private const string EXCHANGE_NAME = "backend-api";
		private const string EXCHANGE_TYPE = ExchangeType.Fanout;

		private const int DATABASE_COUNT = 16;

		private static ConnectionMultiplexer RadisConnection => ConnectionMultiplexer.Connect("localhost");

		public static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Waiting... Press any key to exit");
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
						var message = GetDatabase(messageId).StringGet(messageId);
						Console.WriteLine("Received message: {0}", message);
					};

					channel.BasicConsume(queueName, true, consumer);
					Console.ReadLine();
				}
			}
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

			return RadisConnection.GetDatabase(databaseId);
		}
	}
}
