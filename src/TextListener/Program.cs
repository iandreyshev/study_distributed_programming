using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;

namespace TextListener
{
	public class Program
	{
		private static readonly string QUEUE_NAME = "backend-api";

		private static ConnectionMultiplexer RadisConnection => ConnectionMultiplexer.Connect("localhost");
		private static IDatabase Database => RadisConnection.GetDatabase();

		public static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Waiting message...");
				Console.WriteLine("Press any key to exit.");

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
					channel.QueueDeclare(QUEUE_NAME, false, false, false, null);

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var messageId = Encoding.UTF8.GetString(body);
						Console.WriteLine("Received message: {0}", Database.StringGet(messageId));
					};

					channel.BasicConsume(QUEUE_NAME, true, consumer);
					Console.ReadLine();
				}
			}
		}
	}
}
