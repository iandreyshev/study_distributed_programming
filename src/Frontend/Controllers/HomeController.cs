using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frontend.Controllers
{
	public class HomeController : Controller
	{
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
			var values = new Dictionary<string, string>
			{
				{ "text", text }
			};

			var client = new HttpClient();
			var content = new FormUrlEncodedContent(values);
			var response = await client.PostAsync("http://127.0.0.1:5000/api/Values", content);
			var responseString = await response.Content.ReadAsStringAsync();

			return RedirectToAction(nameof(ShowTextDetails), new { id = responseString });
		}

		[HttpGet]
		public async Task<IActionResult> ShowTextDetails(string id)
		{
			var client = new HttpClient();
			var response = await client.GetAsync("http://127.0.0.1:5000/api/TextDetails/" + id);
			var resultString = await response.Content.ReadAsStringAsync();

			return View(resultString as object);
		}
	}
}
