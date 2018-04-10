using RabbitMQ.Client;
using System.Text;

namespace Backend.MessageQueue
{
	public class RabbitMessageQueue : IMessageQueue
	{
		private readonly string QUEUE_NAME = "backend-api";

		public void Post(string tag, string message)
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					channel.QueueDeclare(QUEUE_NAME, false, false, false, null);
					var body = Encoding.UTF8.GetBytes(message);
					channel.BasicPublish("", QUEUE_NAME, null, body);
				}
			}
		}
	}
}
