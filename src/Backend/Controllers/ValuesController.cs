using System;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		static readonly string QUEUE_NAME = "backend-api";
		static readonly string RADIS_HOST = "localhost";

		// GET api/values/<id>
		[HttpGet("{id}")]
		public string Get(string id)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RADIS_HOST);
			IDatabase database = redis.GetDatabase();
			return database.StringGet(id);
		}

		// POST api/values
		[HttpPost]
		public string Post([FromForm]string value)
		{
			Console.WriteLine("Data: " + value);
			string id = Guid.NewGuid().ToString();
			Console.WriteLine("Id: " + id);

			SaveData(id, value);
			PostMessageAboutNewData(id);

			return id;
		}

		private void SaveData(string id, string value)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RADIS_HOST);
			IDatabase database = redis.GetDatabase();
			database.StringSet(id, value);
		}

		private void PostMessageAboutNewData(string id)
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					channel.QueueDeclare(QUEUE_NAME, false, false, false, null);
					var body = Encoding.UTF8.GetBytes(id);
					channel.BasicPublish("", QUEUE_NAME, null, body);
				}
			}
		}
	}
}
