﻿using Microsoft.AspNetCore.Mvc;
using DerivativeCalculator;
using System.Net;
using System.Reflection.Emit;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DerivativeCalculatorAPI.Controllers
{
	[ApiController]
	[Route("/")]
	public class ExerciseController : ControllerBase
	{
		private readonly ILogger<ExerciseController> _logger;

		public ExerciseController(ILogger<ExerciseController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// A general query to generate exercises
		/// IMPORTANT: ONLY THIS QUERY SHOULD BE USED
		/// </summary>
		/// <param name="body">
		/// Contains either the level or the difficultyMetrics, with the latter having priority.
		/// Preferences can also be set here.
		/// </param>
		/// <returns>The generated exercise</returns>
		[HttpPost("generate-exercise")]
		public ResponseData Post([FromBody] ExerciseQueryBody body)
		{
			// TODO: validate difficulty

			_logger.LogInformation("Generating exercise from body!");

			DifficultyMetrics difficulty;

			if (body.difficultyMetrics is not null)
			{
				try
				{
					difficulty = (DifficultyMetrics)body.difficultyMetrics;
				} 
				catch (Exception e)
				{
					Console.WriteLine("Difficulty metrics invalid!");

					_logger.LogWarning($"Difficulty metrics could not be converted! body: \n{JsonSerializer.Serialize(body)} \n${e.Message} \n${e.StackTrace}");

					Response.StatusCode = (int)HttpStatusCode.BadRequest;
					return new ResponseData("EXERCISE GENERATION ERROR", "A nehézségi beállítások invalidak!");
				}
			}
			else if (body.level is not null)
			{
				difficulty = body.level.ToLower().Trim() switch
				{
					"easy" => DifficultyMetrics.Easy,
					"medium" => DifficultyMetrics.Medium,
					"hard" => DifficultyMetrics.Hard,
					"hardcore" => DifficultyMetrics.Hardcore,
					_ => DifficultyMetrics.Medium,
				};
			}
			else
			{
				Console.WriteLine("Difficulty metrics and level are null!");

				_logger.LogWarning($"Difficulty metrics and level are null! body: \n{JsonSerializer.Serialize(body)}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("EXERCISE GENERATION ERROR", "A nehézségi beállítások és a nehézségi szint is null!");
			}

			var preferences = body.preferences ?? Preferences.Default;

			var simplificationParams = SimplificationParams.Default with
			{
				opsNotToEval = preferences.simplificationPreferences.GetOpsNotToEval()
			};

			TreeNode tree;
			
			try
			{
				tree = ExerciseGenerator.GenerateRandomTree(difficulty, simplificationParams); 
			} 
			catch (ExerciseCouldNotBeGeneratedException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ExerciseCouldNotBeGeneratedException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("EXERCISE GENERATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unknown error: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN", e.Message);
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
					out stepDescriptions,
					simplificationParams
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ParsingError: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("PARSING ERROR", e.Message);
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"DifferentiationException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("DIFFERENTIATION ERROR", e.Message);
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"SimplificationException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("SIMPLIFICATION ERROR", e.Message);
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"NotFiniteNumberException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("EVALUATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unknown error: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN", e.Message);
			}

			_logger.LogInformation($"Exercise generated successfully! (level={body.level}, difficultyMetrics={JsonSerializer.Serialize(body.difficultyMetrics)}, exercise={inputAsLatex})");

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}

		/// <summary>
		/// Medium level query
		/// IMPORTANT: SHOULD NOT BE USED
		/// </summary>
		/// <returns>The generated exercise</returns>
		[HttpGet("generate-exercise")]
		public ResponseData GetWithoutInput()
		{
			var difficulty = DifficultyMetrics.Medium;

			var simplificationParams = SimplificationParams.Default;

			TreeNode tree;

			_logger.LogInformation("Generating exercise without body or URI params!");

			try
			{
				tree = ExerciseGenerator.GenerateRandomTree(difficulty, simplificationParams);
			}
			catch (ExerciseCouldNotBeGeneratedException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ExerciseCouldNotBeGeneratedException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("EXERCISE GENERATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unknown error: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN", e.Message);
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
					out stepDescriptions,
					simplificationParams
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ParsingError: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("PARSING ERROR", e.Message);
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"DifferentiationException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("DIFFERENTIATION ERROR", e.Message);
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"SimplificationException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("SIMPLIFICATION ERROR", e.Message);
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"NotFiniteNumberException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("EVALUATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unkown error: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN", e.Message);
			}

			_logger.LogInformation($"Exercise generated successfully! (exercise={inputAsLatex})");

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}

		/// <summary>
		/// Variable level, with optional preferences from query
		/// IMPORTANT: SHOULD NOT BE USED
		/// </summary>
		/// <param name="level">URI param</param>
		/// <param name="preferences">Query params</param>
		/// <returns>The generated exercise</returns>
		[HttpGet("generate-exercise/{level}")]
		public ResponseData Get(string level, [FromQuery] Preferences preferences)
		{
			DifficultyMetrics difficulty = level.ToLower().Trim() switch
			{
				"easy" => DifficultyMetrics.Easy,
				"medium" => DifficultyMetrics.Medium,
				"hard" => DifficultyMetrics.Hard,
				"hardcore" => DifficultyMetrics.Hardcore,
				_ => DifficultyMetrics.Medium,
			};

			var simplificationParams = SimplificationParams.Default with
			{
				opsNotToEval = preferences.simplificationPreferences.GetOpsNotToEval()
			};

			TreeNode tree;

			try
			{
				tree = ExerciseGenerator.GenerateRandomTree(difficulty, simplificationParams);
			}
			catch (ExerciseCouldNotBeGeneratedException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ExerciseCouldNotBeGeneratedException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("EXERCISE GENERATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unknown error: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN ERROR", e.Message);
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
					out stepDescriptions,
					simplificationParams
				);
			}
			catch (ParsingError e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"ParsingError: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("PARSING ERROR", e.Message);
			}
			catch (DifferentiationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"DifferentiationException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("DIFFERENTIATION ERROR", e.Message);
			}
			catch (SimplificationException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"SimplificationException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("SIMPLIFICATION ERROR", e.Message);
			}
			catch (NotFiniteNumberException e)
			{
				Console.WriteLine(e.Message);

				_logger.LogWarning($"NotFiniteNumberException: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return new ResponseData("EVALUATION ERROR", e.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				_logger.LogError($"Unkown error: {e.Message} {e.StackTrace}");

				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return new ResponseData("UNKNOWN", e.Message);
			}

			_logger.LogInformation($"Exercise generated succesfully! (level={level}, exercise={inputAsLatex})");

			return new ResponseData(inputAsLatex, simplifiedInputAsLatex, outputAsLatex, stepsAsLatex, stepDescriptions, varToDiff);
		}
	}
}