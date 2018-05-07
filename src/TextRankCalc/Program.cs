using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;

namespace TextRankCalc
{
	public class Program
	{
		private const string BASE_EXCHANGE_NAME = "backend-api";
		private const string BASE_EXCHANGE_TYPE = ExchangeType.Fanout;

		private const string VOWEL_CONS_EXCHANGE = "vowel-cons-api";
		private const string VOWEL_CONS_EXCHANGE_TYPE = ExchangeType.Direct;
		private const string ROUTE_VOWELS_CALC = "calculate-vowels-count";

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
					// To publish messages
					channel.ExchangeDeclare(VOWEL_CONS_EXCHANGE, VOWEL_CONS_EXCHANGE_TYPE);

					// To receive messages
					channel.ExchangeDeclare(BASE_EXCHANGE_NAME, BASE_EXCHANGE_TYPE);
					var queueName = channel.QueueDeclare().QueueName;
					channel.QueueBind(queueName, BASE_EXCHANGE_NAME, routingKey: "");

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var message = Encoding.UTF8.GetString(eventArgs.Body);
						Console.WriteLine("Received message: {0}", message);

						channel.BasicPublish(VOWEL_CONS_EXCHANGE, ROUTE_VOWELS_CALC, body: eventArgs.Body);
					};

					channel.BasicConsume(queueName, true, consumer);

					Console.WriteLine("Waiting... Press any key to exit");
					Console.ReadLine();
				}
			}
		}
	}
}
