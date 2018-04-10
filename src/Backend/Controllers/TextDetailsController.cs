using Backend.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class TextDetailsController : Controller
	{
		private static readonly int REPEATS_COUNT = 5;
		private static readonly int REPEAT_PAUSE_MS = 100;

		private IRepository _repository;

		public TextDetailsController(IRepository repository)
		{
			_repository = repository;
		}

		// POST api/TextDetails
		[HttpPost]
		public string Post([FromForm]string id)
		{
			string result = null;

			Utils.LambdaUtils.Repeat(REPEATS_COUNT, REPEAT_PAUSE_MS, (_) => {
				result = _repository.GetString(id);
				return result != null;
			});

			if (result == null)
			{
				return "NOT FOUND";
			}

			return "FOUND!";
		}
	}
}
