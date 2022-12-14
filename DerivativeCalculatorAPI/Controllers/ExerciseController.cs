using Microsoft.AspNetCore.Mvc;
using DerivativeCalculator;
using System.Net;

namespace DerivativeCalculatorAPI.Controllers
{
	[ApiController]
	[Route("/")]
	public class ExerciseController : ControllerBase
	{
		[HttpPost("generate-exercise")]
		public ResponseData Post([FromBody] DifficultyMetrics difficulty)
		{
			// TODO: validate difficulty

			TreeNode tree;
			
			try
			{
				tree = ExerciseGenerator.GenerateRandomTree(difficulty); 
			} 
			catch (ExerciseCouldNotBeGeneratedException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EXERCISE GENERATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}

			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription> stepDescriptions;
			char varToDiff = 'x';

			try
			{
				outputAsLatex = DerivativeManager.DifferentiateTree(
					tree,
					'x', 
					out inputAsLatex, 
					out simplifiedInputAsLatex, 
					out stepsAsLatex, 
					out stepDescriptions
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EVALUATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}

		// without data, the request is invalid
		[HttpGet("generate-exercise")]
		public ResponseData GetWithoutInput()
		{
			var difficulty = DifficultyMetrics.Medium;

			TreeNode tree;

			try
			{
				tree = ExerciseGenerator.GenerateRandomTree(difficulty);
			}
			catch (ExerciseCouldNotBeGeneratedException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EXERCISE GENERATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}

			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription?> stepDescriptions;
			char varToDiff = 'x';

			try
			{
				outputAsLatex = DerivativeManager.DifferentiateTree(
					tree,
					'x',
					out inputAsLatex,
					out simplifiedInputAsLatex,
					out stepsAsLatex,
					out stepDescriptions
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EVALUATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}

		[HttpGet("generate-exercise/{level}")]
		public ResponseData Get(string level)
		{
			DifficultyMetrics difficulty = level.ToLower().Trim() switch
			{
				"easy" => DifficultyMetrics.Easy,
				"medium" => DifficultyMetrics.Medium,
				"hard" => DifficultyMetrics.Hard,
				"hardcore" => DifficultyMetrics.Hardcore,
				_ => DifficultyMetrics.Medium,
			};

			TreeNode tree;

			try
			{
				tree = ExerciseGenerator.GenerateRandomTree(difficulty);
			}
			catch (ExerciseCouldNotBeGeneratedException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EXERCISE GENERATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "UNKNOWN ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData();
			}

			string inputAsLatex, simplifiedInputAsLatex, outputAsLatex;
			List<string> stepsAsLatex;
			List<StepDescription> stepDescriptions;
			char varToDiff = 'x';

			try
			{
				outputAsLatex = DerivativeManager.DifferentiateTree(
					tree,
					'x',
					out inputAsLatex,
					out simplifiedInputAsLatex,
					out stepsAsLatex,
					out stepDescriptions
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "PARSING ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("x-exception-type", "DIFFERENTIATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("x-exception-type", "SIMPLIFICATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("Access-Control-Expose-Headers", "x-exception-type, x-exception-message");
				Response.Headers.Add("x-exception-type", "EVALUATION ERROR");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				Response.Headers.Add("x-exception-type", "UNKNOWN");
				Response.Headers.Add("x-exception-message", e.Message);

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData();
			}

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}
	}
}