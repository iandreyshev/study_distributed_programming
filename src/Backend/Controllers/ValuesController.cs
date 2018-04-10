using System;
using Microsoft.AspNetCore.Mvc;
using Backend.MessageQueue;
using Backend.Repository;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		private IRepository _repository;
		private IMessageQueue _messageQueue;

		public ValuesController(
			IRepository repository,
			IMessageQueue messageQueue)
		{
			_messageQueue = messageQueue;
			_repository = repository;
		}

		// POST api/Values
		[HttpPost]
		public string Post([FromForm]string text)
		{
			string id = Guid.NewGuid().ToString();

			_repository.SetString(id, text);
			_messageQueue.Post("", id);

			return id;
		}
	}
}
