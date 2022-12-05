using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DerivativeCalculator;

namespace DerivativeCalculatorWebsite.Pages
{
	public class IndexModel : PageModel
	{

		private readonly ILogger<IndexModel> _logger;

		private string prettyInput = "";
		private string prettySimplifiedInput = "";
		private List<string> prettySteps = new List<string>();
		private string derivativeOutput = "";
		private string errorString = "";

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void OnPost ()
		{
			string derivativeInput = Request.Form["derivativeInput"];

			try
			{
				derivativeOutput = DerivativeCalculator.DerivativeManager.DifferentiateString(derivativeInput, out prettyInput, out prettySimplifiedInput, out prettySteps);
			}
			catch (Exception e)
			{
				errorString = e.Message;
			}

			ViewData["prettySteps"] = prettySteps;
			ViewData["prettyInput"] = prettyInput;
			ViewData["prettySimplifiedInput"] = prettySimplifiedInput;
			ViewData["derivativeOutput"] = derivativeOutput;
			ViewData["errorString"] = errorString;
			ViewData["isShowingSolution"] = false;
		}

		//public void ShowSolution(object sender, EventArgs e)
		//{
		//	ViewData["prettySteps"] = prettySteps;
		//}

		//public void HideSolution(object sender, EventArgs e)
		//{
		//	ViewData["prettySteps"] = new List<string>();
		//}

		public void OnGet()
		{
			//ViewData["prettySteps"] = new List<string>();
		}
	}
}