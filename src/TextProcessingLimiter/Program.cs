using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace TextProcessingLimiter
{
	public class Program
	{
		private const string APPLY_EXCHANGE_NAME = "processing-limiter";
		private const string APPLY_EXCHANGE_TYPE = ExchangeType.Fanout;

		private static int m_availableRequests = 2;

		public static void Main(string[] args)
		{
			StartMessageListener();
		}

		private static void StartMessageListener()
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					// To publish messages
					channel.ExchangeDeclare(APPLY_EXCHANGE_NAME, APPLY_EXCHANGE_TYPE);

					var backendReceiver = new Receiver("backend-api", ExchangeType.Fanout);
					backendReceiver.AddCallback((message) =>
					{
						Console.WriteLine("Received message from backend: {0}", message);

						if (m_availableRequests > 0)
						{
							message = true.ToString() + "|" + message;
							--m_availableRequests;
						}
						else
						{
							message = false.ToString() + "|" + message;
						}

						var body = Encoding.UTF8.GetBytes(message);
						channel.BasicPublish(APPLY_EXCHANGE_NAME, "", null, body);
						Console.WriteLine("Publish message: {0}", message);
					});

					var resultReceiver = new Receiver("success-marked", ExchangeType.Fanout);
					resultReceiver.AddCallback((message) =>
					{
						Console.WriteLine("Received message from success-marker: {0}", message);

						try
						{
							if (bool.Parse(message.Split("|")[0]))
							{
								++m_availableRequests;
							}
						}
						catch (Exception) { }

						Console.WriteLine("Available requests count: " + m_availableRequests);
					});

					Console.WriteLine("Waiting... Press any key to exit");
					Console.ReadLine();
				}
			}
		}
	}
}
