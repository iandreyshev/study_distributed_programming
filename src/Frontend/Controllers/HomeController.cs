using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frontend.Controllers
{
	public class HomeController : Controller
	{
		private static readonly HttpClient client = new HttpClient();

		public IActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public IActionResult Upload()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Upload(string data)
		{
			var values = new Dictionary<string, string>
			{
				{ "value", data }
			};

			var content = new FormUrlEncodedContent(values);
			var response = await client.PostAsync("http://127.0.0.1:5000/api/values", content);

			Console.WriteLine(response);

			var responseString = await response.Content.ReadAsStringAsync();

			return Ok(responseString);
		}
	}
}
