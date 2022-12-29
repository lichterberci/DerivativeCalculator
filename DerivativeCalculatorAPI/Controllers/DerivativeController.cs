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

		[HttpPost("differentiate")]
		public ResponseData Post([FromBody] string input)
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

		// without data, the request is invalid
		[HttpGet("differentiate")]
		public ResponseData GetWithoutInput ()
		{
			Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return new ResponseData();
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