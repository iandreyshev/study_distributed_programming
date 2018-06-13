using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace TextProcessingLimiter
{
	class Receiver
	{
		private ConnectionFactory m_factory = null;
		private IConnection m_connection = null;
		private IModel m_channel = null;
		private EventingBasicConsumer m_consumer = null;
		private string m_exchange = "";

		public Receiver(string exchangeName, string exchangeType)
		{
			m_factory = new ConnectionFactory();
			m_connection = m_factory.CreateConnection();
			m_channel = m_connection.CreateModel();
			m_consumer = new EventingBasicConsumer(m_channel);
			m_exchange = exchangeName;

			m_channel.ExchangeDeclare(m_exchange, exchangeType);
		}

		public void AddCallback(Action<string> onRecieveCallback)
		{
			var queue = m_channel.QueueDeclare().QueueName;
			m_channel.QueueBind(queue, m_exchange, routingKey: "");

			m_consumer.Received += (model, ea) =>
			{
				onRecieveCallback(Encoding.UTF8.GetString(ea.Body));
			};

			m_channel.BasicConsume(
				queue: queue,
				autoAck: true,
				consumer: m_consumer);
		}
	}
}
