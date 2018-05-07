using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;
using System.Threading;

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
					channel.ExchangeDeclare(EXCHANGE, EXCHANGE_TYPE);

					channel.QueueDeclare(QUEUE, exclusive: false);
					channel.QueueBind(QUEUE, EXCHANGE, ROUTE);

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var message = Encoding.UTF8.GetString(body);
						Console.WriteLine("Received message: {0}", message);

						var data = message.Split(SEPARATOR);
						var result = CalcResult(data[1], data[2]);

						var textId = data[0];
						Database.StringSet(RESULT_ID_PREFIX + data[0], result);
						Console.WriteLine("Result for '{0}': {1}", textId, result);
					};

					channel.BasicConsume(QUEUE, true, consumer);

					Console.WriteLine("Waiting... Press any key to exit");
					Console.ReadLine();
				}
			}
		}

		private static string CalcResult(string vowelCount, string consCount)
		{
			Thread.Sleep(5000);

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
	}
}
