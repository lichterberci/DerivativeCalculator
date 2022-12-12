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
		private List<(string, StepDescription?)> stepsWithDescriptions = new List<(string, StepDescription?)>();
		private string derivativeOutput = "";
		private string errorString = "";

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void OnPost ()
		{
			string derivativeInput = Request.Form["derivativeInput"];

			List<string> steps;
			List<StepDescription> descriptions;

			try
			{
				derivativeOutput = DerivativeCalculator.DerivativeManager.DifferentiateString(derivativeInput, out prettyInput, out prettySimplifiedInput, out steps, out descriptions);

				stepsWithDescriptions = new List<(string, StepDescription?)>(steps.Select((step, i) => (step, descriptions[i])));
			}
			catch (Exception e)
			{
				errorString = e.Message;
			}

			ViewData["prettyInput"] = prettyInput;
			ViewData["prettySimplifiedInput"] = prettySimplifiedInput;
			ViewData["derivativeOutput"] = derivativeOutput;
			ViewData["errorString"] = errorString;
			ViewData["isShowingSolution"] = false;
			ViewData["stepsWithDescriptions"] = stepsWithDescriptions;
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