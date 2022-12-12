using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DerivativeCalculator;

namespace DerivativeCalculatorWebsite.Pages
{
	public class RandomModel : PageModel
	{

		private readonly ILogger<IndexModel> _logger;

		private string prettyInput = "";
		private string prettySimplifiedInput = "";
		private List<string> prettySteps = new List<string>();
		private string derivativeOutput = "";
		private string errorString = "";

		public RandomModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void OnPost()
		{
			string level = Request.Form["level"];

			Console.WriteLine(level	);

			DifficultyMetrics difficulty = level switch
			{
				"easy" => DifficultyMetrics.Easy,
				"medium" => DifficultyMetrics.Medium,
				"hard" => DifficultyMetrics.Hard,
				"hardcore" => DifficultyMetrics.Hardcore,
				_ => DifficultyMetrics.Medium
			};

			TreeNode tree = ExerciseGenerator.GenerateRandomTree(difficulty);

			TreeUtils.PrintTree(tree);
			Console.WriteLine($"(={TreeUtils.CollapseTreeToString(tree)})");

			TreeUtils.PrintTree(tree.Diff('x'));
			Console.WriteLine($"(={TreeUtils.CollapseTreeToString(tree.Diff('x'))}");

			string prettyInput = tree.ToLatexString();

			try
			{
				derivativeOutput = DerivativeManager.DifferentiateTree(tree, 'x', out prettyInput, out prettySimplifiedInput, out prettySteps, out _);
			}
			catch (Exception ex)
			{
				errorString = ex.Message;
			}

			ViewData["prettyInput"] = prettyInput;
			ViewData["prettySteps"] = prettySteps;
			ViewData["prettySimplifiedInput"] = prettySimplifiedInput;
			ViewData["derivativeOutput"] = derivativeOutput;
			ViewData["errorString"] = errorString;
			ViewData["isShowingSolution"] = false;
		}

		public void ShowSolution(object sender, EventArgs e)
		{
			ViewData["prettySteps"] = prettySteps;
		}

		public void HideSolution(object sender, EventArgs e)
		{
			ViewData["prettySteps"] = new List<string>();
		}

		public void OnGet()
		{
			ViewData["prettySteps"] = new List<string>();
		}
	}
}