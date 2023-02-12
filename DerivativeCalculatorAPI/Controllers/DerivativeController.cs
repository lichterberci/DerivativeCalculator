using Microsoft.AspNetCore.Mvc;
using DerivativeCalculator;
using System.Net;

namespace DerivativeCalculatorAPI.Controllers
{
	[ApiController]
	[Route("/")]
	public class DerivativeController : ControllerBase
	{
		private readonly ILogger<DerivativeController> _logger;

		public DerivativeController(ILogger<DerivativeController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// A general query for differentiating custom input
		/// </summary>
		/// <param name="body">Contains the input string and optionally the preferenes</param>
		/// <returns>The derivative along with the steps</returns>
		[HttpPost("differentiate")]
		public ResponseData Post([FromBody]DifferentiateQueryBody body)
		{
			var input = body.input ?? "";
			var preferences = body.preferences ?? Preferences.Default;

			var simplificationParams = SimplificationParams.Default with
			{
				opsNotToEval = preferences.simplificationPreferences.GetOpsNotToEval()
			};

			if (string.IsNullOrEmpty(input))
			{
				Console.WriteLine("Input is null when using body!");
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("PARSING ERROR", "A bemenet null!");
			}

			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription> stepDescriptions;
			char varToDiff;

			_logger.LogInformation($"Differentiating from body! input: {input}");

			try
			{
				outputAsLatex = DerivativeCalculator.DerivativeManager.DifferentiateString(
					input, 
					out inputAsLatex, 
					out simplifiedInputAsLatex, 
					out stepsAsLatex, 
					out stepDescriptions, 
					out varToDiff,
					simplificationParams
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"Parsing error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("PARSING ERROR", e.Message);
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"Differentiation error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("DIFFERENTIATION ERROR", e.Message);
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"Simplification error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("SIMPLIFICATION ERROR", e.Message);
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"NotFinitNumberException: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("EVALUATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unkown error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN ERROR", e.Message);
			}

			_logger.LogInformation($"Differentiation was successfull! (input={input}, outputAsLatex={outputAsLatex})");

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}

		/// <summary>
		/// IMPORTANT: SHOULD NOT BE USED
		/// </summary>
		/// <returns>An error</returns>
		// without data, the request is invalid
		[HttpGet("differentiate")]
		public ResponseData GetWithoutInput ()
		{
			Console.WriteLine("differenatiate endpoint called without input!");

			_logger.LogWarning($"Empty differentiation endpoint called!");

			Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return new ResponseData("PARSING ERROR", "A bemenet üres!");
		}

		/// <summary>
		/// This is a suboptimal query for differentiating
		/// </summary>
		/// <param name="preferences">Query params</param>
		/// <param name="input">URI param</param>
		/// <returns>The derivative along with the steps</returns>
		[HttpGet("differentiate/{input}")]
		public ResponseData Get([FromQuery]Preferences preferences, string input)
		{
			var simplificationParams = SimplificationParams.Default with
			{
				opsNotToEval = preferences.simplificationPreferences.GetOpsNotToEval()
			};

			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription> stepDescriptions;
			char varToDiff;

			_logger.LogInformation($"Differentiating from URI! input: {input}");

			try
			{
				outputAsLatex = DerivativeManager.DifferentiateString(
					input, 
					out inputAsLatex, 
					out simplifiedInputAsLatex, 
					out stepsAsLatex, 
					out stepDescriptions,
					out varToDiff,
					simplificationParams
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ParsingError: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("PARSING ERROR", e.Message);
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"DifferentiationError: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("DIFFERENTIATION ERROR", e.Message);
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"SimplificationError: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("SIMPLIFICATION ERROR", e.Message);
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"NotFiniteNumberException: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("EVALUATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"Unknown error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN ERROR", e.Message);
			}

			_logger.LogInformation($"Differentiation was successfull! (input={input}, outputAsLatex={outputAsLatex})");

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}
	}
}