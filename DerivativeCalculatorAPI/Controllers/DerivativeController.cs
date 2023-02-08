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
				return new ResponseData();
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

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"Parsing error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"Differentiation error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"Simplification error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EVALUATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"NotFinitNumberException: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogError($"Unkown error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
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

			Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
			Response.Headers.Add("x-exception-type", "PARSING ERROR");
			Response.Headers.Add("x-exception-message", "Input is empty!");

			_logger.LogWarning($"Empty differentiation endpoint called!");

			Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return new ResponseData();
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

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"ParsingError: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"DifferentiationError: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"SimplificationError: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EVALUATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"NotFiniteNumberException: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				_logger.LogWarning($"Unknown error: {e.Message}\nStacktrace: {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}

			_logger.LogInformation($"Differentiation was successfull! (input={input}, outputAsLatex={outputAsLatex})");

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}
	}
}