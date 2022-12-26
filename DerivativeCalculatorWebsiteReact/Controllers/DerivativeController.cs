using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using DerivativeCalculator;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DerivativeCalculatorWebsiteReact.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DerivativeController : ControllerBase
	{
		private class Data
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

		private readonly ILogger<DerivativeController> _logger;

		public DerivativeController(ILogger<DerivativeController> logger)
		{
			_logger = logger;
		}

		[HttpPost]
		[Route("api/frominput")]
		public ActionResult<string> Post(HttpRequestMessage request, string input)
		{
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
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return "";
			}

			Data resultData = new Data(prettyInput, simplifiedInput, output, steps, stepDescriptions);

			if (resultData is null)
			{
				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return "";
			}

			Response.Headers.Add("Content-Type", "application/json");

			string result = JsonSerializer.Serialize(resultData);

			return result;
		}
	}
}