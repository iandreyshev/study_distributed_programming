using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frontend.Controllers
{
	public class HomeController : Controller
	{
		private HttpClient _client = new HttpClient();

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult TextDetails()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(string text)
		{
			var response = await PostAsync("text", text, "http://127.0.0.1:5000/api/Values");
			var responseString = await response.ReadAsStringAsync();

			return Ok(responseString);
		}

		[HttpPost]
		public async Task<IActionResult> TextDetails(string id)
		{
			var response = await PostAsync("id", id, "http://127.0.0.1:5000/api/TextDetails");
			var responseString = await response.ReadAsStringAsync();

			return Ok(responseString);
		}

		private async Task<HttpContent> PostAsync(string key, string value, string url)
		{
			var values = new Dictionary<string, string>
			{
				{ key, value }
			};

			var content = new FormUrlEncodedContent(values);
			var response = await _client.PostAsync(url, content);

			return response.Content;
		}
	}
}
