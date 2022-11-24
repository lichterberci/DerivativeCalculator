using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DerivativeCalculator;

namespace DerivativeCalculatorWebsite.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void Click()
		{

		}

		public void OnGet()
		{
			string dateTime = DateTime.Now.ToString("d", new CultureInfo("hu"));
			ViewData["TimeStamp"] = dateTime;

			string input = "2x+1";

			ViewData["DerivativeInput"] = input;

			string output = Manager.DifferentiateString(input);

			ViewData["DerivativeOutput"] = output;
		}
	}
}