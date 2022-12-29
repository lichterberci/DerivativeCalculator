using System.Net;
using System.Text.Json;

using DerivativeCalculator;

using Microsoft.AspNetCore.Mvc;

namespace DerivativeCalculatorWebsiteReact.Controllers
{
	public class Data
	{
		readonly string prettyInput;
		readonly string prettySimplifiedInput;
		readonly string prettyOutput;
		readonly List<string> steps;
		readonly List<string> stepDescriptions;

		public Data(string prettyInput, string prettySimplifiedInput, string prettyOutput, List<string> steps, List<StepDescription> stepDescriptions)
		{
			this.prettyInput = prettyInput;
			this.prettySimplifiedInput = prettySimplifiedInput;
			this.prettyOutput = prettyOutput;
			this.steps = steps;

			if (stepDescriptions is null)
				throw new ArgumentException("Stepdescription should not be null!", nameof(stepDescriptions));
			this.stepDescriptions = stepDescriptions.Select(desc => JsonSerializer.Serialize(desc)).ToList();
		}
	}

	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public ActionResult<Data> Get()
		{
			string input = "sinx";

			Console.WriteLine("Backend called");

			Console.WriteLine("Backend called!");
			Console.WriteLine(input);

			string prettyInput;
			string simplifiedInput;
			string output;
			List<string> steps;
			List<StepDescription> stepDescriptions;

			try
			{
				output = DerivativeCalculator.DerivativeManager.DifferentiateString(input, out prettyInput, out simplifiedInput, out steps, out stepDescriptions);
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: " + e.Message);
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return null;
			}

			Data resultData = new Data(prettyInput, simplifiedInput, output, steps, stepDescriptions);

			if (resultData is null)
			{
				Console.WriteLine("ERROR: resultData is null!");
				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return null;
			}

			//Response.Headers.Add("Content-Type", "application/json");

			//string result = JsonSerializer.Serialize(resultData);

			Console.WriteLine("Returning as normal");

			return resultData;
		}
	}
}

//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Microsoft.AspNetCore.Mvc;
//using DerivativeCalculator;
//using System.Net;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;

//namespace DerivativeCalculatorWebsiteReact.Controllers
//{
//	[ApiController]
//	[Route("[controller]")]
//	public class DerivativeController : ControllerBase
//	{
//		public class Data
//		{
//			readonly string prettyInput;
//			readonly string prettySimplifiedInput;
//			readonly string prettyOutput;
//			readonly List<string> steps;
//			readonly List<string> stepDescriptions;

//			public Data(string prettyInput, string prettySimplifiedInput, string prettyOutput, List<string> steps, List<StepDescription> stepDescriptions)
//			{
//				this.prettyInput = prettyInput;
//				this.prettySimplifiedInput = prettySimplifiedInput;
//				this.prettyOutput = prettyOutput;
//				this.steps = steps;

//				if (stepDescriptions is null)
//					throw new ArgumentException("Stepdescription should not be null!", nameof(stepDescriptions));
//				this.stepDescriptions = stepDescriptions.Select(desc => JsonSerializer.Serialize(desc)).ToList();
//			}
//		}

//		private readonly ILogger<DerivativeController> _logger;

//		public DerivativeController(ILogger<DerivativeController> logger)
//		{
//			_logger = logger;
//		}

//		[HttpGet]
//		public Data Get()
//		{
//			string input = "sinx";

//			Console.WriteLine("Backend called!");
//			Console.WriteLine(input);

//			string prettyInput;
//			string simplifiedInput;
//			string output;
//			List<string> steps;
//			List<StepDescription> stepDescriptions;

//			try
//			{
//				output = DerivativeCalculator.DerivativeManager.DifferentiateString(input, out prettyInput, out simplifiedInput, out steps, out stepDescriptions);
//			}
//			catch (Exception e)
//			{
//				Response.StatusCode = (int)HttpStatusCode.BadRequest;
//				return null;
//			}

//			Data resultData = new Data(prettyInput, simplifiedInput, output, steps, stepDescriptions);

//			if (resultData is null)
//			{
//				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//				return null;
//			}

//			//Response.Headers.Add("Content-Type", "application/json");

//			//string result = JsonSerializer.Serialize(resultData);

//			return resultData;
//		}
//	}
//}