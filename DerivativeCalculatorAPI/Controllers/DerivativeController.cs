using Microsoft.AspNetCore.Mvc;
using DerivativeCalculator;
using System.Net;

namespace DerivativeCalculatorAPI.Controllers
{
	[ApiController]
	[Route("/")]
	public class DerivativeController : ControllerBase
	{
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
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions);
		}

		// without data, the request is invalid
		[HttpGet("differentiate")]
		public ResponseData GetWithoutInput ()
		{
			Console.WriteLine("differenatiate endpoint called without input!");

			Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
			Response.Headers.Add("x-exception-type", "PARSING ERROR");
			Response.Headers.Add("x-exception-message", "Input is empty!");

			Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return new ResponseData();
		}

		[HttpGet("differentiate/{input}")]
		public ResponseData Get(string input)
		{
			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription> stepDescriptions;

			try
			{
				outputAsLatex = DerivativeManager.DifferentiateString(
					input, 
					out inputAsLatex, 
					out simplifiedInputAsLatex, 
					out stepsAsLatex, 
					out stepDescriptions
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Request-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions);
		}
	}
}