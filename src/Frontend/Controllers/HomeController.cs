using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
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

		public async Task<IActionResult> Statistic()
		{
			var client = new HttpClient();
			var response = await client.GetAsync("http://127.0.0.1:5000/api/TextDetails/Statistic");
			var result = new StatisticViewModel
			{
				Result = await response.Content.ReadAsStringAsync()
			};

			return View(result);
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
			var result = new TextDetailsViewModel
			{
				IsExist = response.IsSuccessStatusCode,
				Result = await response.Content.ReadAsStringAsync()
			};

			return View(result);
		}
	}
}
