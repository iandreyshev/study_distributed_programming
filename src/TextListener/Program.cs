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

		private static ConnectionMultiplexer RadisConnection => ConnectionMultiplexer.Connect("localhost");
		private static IDatabase Database => RadisConnection.GetDatabase();

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
					// Why we always declare new exchange ?
					channel.ExchangeDeclare(EXCHANGE_NAME, EXCHANGE_TYPE);

					var queueName = channel.QueueDeclare().QueueName;
					channel.QueueBind(queueName, EXCHANGE_NAME, routingKey: "");

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var messageId = Encoding.UTF8.GetString(body);
						Console.WriteLine("Received message: {0}", Database.StringGet(messageId));
					};

					channel.BasicConsume(queueName, true, consumer);
					Console.ReadLine();
				}
			}
		}
	}
}
