using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace TextSuccessMarker
{
	public class Program
	{
        private const string REEIVE_EXCHANGE_TYPE = ExchangeType.Fanout;
        private const string RECEIVE_EXCHANGE_NAME = "result-api";

        private const string NOTIFY_EXCHANGE_NAME = "success-marked";
        private const string NOTIFY_EXCHANGE_TYPE = ExchangeType.Fanout;

        private const float MIN_GOOD_RESULT = 0.5f;

        public static void Main(string[] args)
		{
            StartListenResultQueue();
        }

        private static void StartListenResultQueue()
        {
            ConnectionFactory factory = new ConnectionFactory();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(RECEIVE_EXCHANGE_NAME, REEIVE_EXCHANGE_TYPE);
                    channel.ExchangeDeclare(NOTIFY_EXCHANGE_NAME, NOTIFY_EXCHANGE_TYPE);

                    var receiveQueue = channel.QueueDeclare().QueueName;
                    channel.QueueBind(receiveQueue, RECEIVE_EXCHANGE_NAME, routingKey: "");

                    var notifyQueue = channel.QueueDeclare().QueueName;
                    channel.QueueBind(notifyQueue, NOTIFY_EXCHANGE_NAME, routingKey: "");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, eventArgs) =>
                    {
                        var body = eventArgs.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Received message: {0}", message);

                        message = IsGoodResult(message).ToString() + "|" + message;

                        var resultBody = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(NOTIFY_EXCHANGE_NAME, "", null, resultBody);
                        Console.WriteLine("Publish: " + message);
                    };

                    channel.BasicConsume(receiveQueue, true, consumer);
                    Console.ReadLine();
                }
            }
        }

        private static bool IsGoodResult(string resultStr)
        {
            try
            {
                float result = float.Parse(resultStr);

                if (result <= MIN_GOOD_RESULT)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
	}
}
