using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;

namespace TextStats
{
	public class Program
	{
        private const string RECEIVE_EXCHANGE_TYPE = ExchangeType.Fanout;
        private const string RECEIVE_EXCHANGE_NAME = "success-marked";

        private static ConnectionMultiplexer RedisConnection =>
			ConnectionMultiplexer.Connect("localhost");
		private static IDatabase Database { get { return RedisConnection.GetDatabase(); } }

		private const string COUNT_KEY = "RESULT_COUNT";
		private const string BEST_COUNT_KEY = "RESULT_BEST_COUNT";
		private const string AVG_COUNT_KEY = "RESULT_AVG";
		private const string AVG_SUM_KEY = "RESULT_AVG_SUM";

		private static int _count = Converter.ToInt(Database.StringGet(COUNT_KEY), 0);
		private static int _bestCount = Converter.ToInt(Database.StringGet(BEST_COUNT_KEY), 0);
		private static float _avgSum = Converter.ToFloat(Database.StringGet(AVG_COUNT_KEY), 0);
		private static float _average = Converter.ToFloat(Database.StringGet(AVG_SUM_KEY), 0);

		public static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Start with values:");
				Console.WriteLine("Count: {0}", _count);
				Console.WriteLine("Best count: {0}", _bestCount);
				Console.WriteLine("Average: {0}", _average);
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
					channel.ExchangeDeclare(RECEIVE_EXCHANGE_NAME, RECEIVE_EXCHANGE_TYPE);
					var queueName = channel.QueueDeclare().QueueName;
					channel.QueueBind(queueName, RECEIVE_EXCHANGE_NAME, routingKey: "");

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, eventArgs) =>
					{
						var body = eventArgs.Body;
						var message = Encoding.UTF8.GetString(body);
						Console.WriteLine("Received message: {0}", message);

						HandleResult(message);
					};

					channel.BasicConsume(queueName, true, consumer);
					Console.ReadLine();
				}
			}
		}

		private static void HandleResult(string resultStr)
		{
			try
			{
                var resultData = resultStr.Split("|");
                var result = float.Parse(resultData[1]);

				++_count;

				if (result >= 0.5f)
				{
					++_bestCount;
				}

				_avgSum += result;
				_average = _avgSum / _count;
			}
			catch (Exception)
			{
				Console.WriteLine("Not a number");
			}

			var database = RedisConnection.GetDatabase();
			database.StringSet(COUNT_KEY, _count);
			database.StringSet(BEST_COUNT_KEY, _bestCount);
			database.StringSet(AVG_COUNT_KEY, _average);
			database.StringSet(AVG_SUM_KEY, _avgSum);

			Console.WriteLine(
				"Count: {0}, Best count: {1}, Average: {2}",
				_count,
				_bestCount,
				_average);
		}
	}
}
