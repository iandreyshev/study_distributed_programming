using RabbitMQ.Client;
using System.Text;

namespace Backend.MessageQueue
{
	public class RabbitMessageQueue : IMessageQueue
	{
		private readonly string EXCHANGE_NAME = "backend-api";
		private readonly string EXCHANGE_TYPE = "fanout";

		public void Post(string tag, string message)
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					channel.ExchangeDeclare(EXCHANGE_NAME, EXCHANGE_TYPE);
					var body = Encoding.UTF8.GetBytes(message);
					channel.BasicPublish(EXCHANGE_NAME, "", null, body);
				}
			}
		}
	}
}
