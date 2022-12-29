using Microsoft.AspNetCore.Mvc;
using DerivativeCalculator;
using System.Net;

namespace DerivativeCalculatorAPI.Controllers
{
	[ApiController]
	[Route("/")]
	public class DerivativeController : ControllerBase
	{
		[HttpGet("test")]
		public IEnumerable<string> Test()
		{
			return new string[] { "asd", "dsa" };
		}

		[HttpGet("differentiate")]
		public ResponseData GetFromBody([FromBody] string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				Console.WriteLine("Input is null when using body!");
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription> stepDescriptions;

			try
			{
				outputAsLatex = DerivativeCalculator.DerivativeManager.DifferentiateString(input, out inputAsLatex, out simplifiedInputAsLatex, out stepsAsLatex, out stepDescriptions);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions);
		}

		[HttpGet("differentiate/{input}")]
		public ResponseData Get(string input)
		{
			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex = new();
			List<StepDescription> stepDescriptions = new();

			try
			{
				outputAsLatex = DerivativeCalculator.DerivativeManager.DifferentiateString(input, out inputAsLatex, out simplifiedInputAsLatex, out stepsAsLatex, out stepDescriptions);
			} 
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions);
		}
	}
}