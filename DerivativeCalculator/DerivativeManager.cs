using System.Text.RegularExpressions;

namespace DerivativeCalculator
{
	public static class DerivativeManager
	{
		public static string DifferentiateString(
			string input, 
			out string inputAsLatex, 
			out string simplifiedInputAsLatex, 
			out List<string> stepsAsLatex, 
			out List<StepDescription> stepDescriptions, 
			out char varToDiff,
			SimplificationParams? simplificationParams = null
		)
		{
			if (simplificationParams == null)
				simplificationParams = SimplificationParams.Default;

			simplifiedInputAsLatex = "";
			inputAsLatex = "";
			stepsAsLatex = new List<string>();

			input = input.Trim().ToLower();

			char varToDifferentiate = 'x';

			if (Regex.IsMatch(input, "^d/d([a-d]|[f-z])"))
			{
				varToDifferentiate = input[3];
				input = input.Substring(4);
			}

			varToDiff = varToDifferentiate;

			List<Node> nodes;
			TreeNode tree;

			try
			{
				tree = Parser.ParseString(input);

				inputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

				tree = TreeUtils.GetSimplestForm(tree, simplificationParams);

				simplifiedInputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

				if (tree == null)
				{
					Console.WriteLine("Parsing error: tree is empty!");
					throw new ParsingError("Parsing error: tree is empty!");
				}
			}
			catch (ParsingError parsingError)
			{
				throw parsingError;
			}
			catch (Exception e)
			{
				Console.WriteLine("Parsing error!");
				throw new ParsingError($"{e.GetType()}: {e.Message}");
			}

			TreeNode diffTree;

			//diffTree = tree;

			try
			{
				diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate, simplificationParams);

				Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));

				stepsAsLatex = Differentiator.steps;
				stepDescriptions = Differentiator.stepDescriptions;

				diffTree = TreeUtils.GetSimplestForm(diffTree, simplificationParams);
			}
			catch (DifferentiationException differentiationException)
			{
				throw differentiationException;
			}
			catch (Exception e)
			{
				Console.WriteLine($"An error occured while differentiating! ({e.Message}) {e.StackTrace}");
				throw new DifferentiationException($"{e.GetType()}: {e.Message}");
			}

			return diffTree.ToLatexString();
		}

		public static string DifferentiateTree (
			TreeNode input, 
			char varToDifferentiate, 
			out string inputAsLatex, 
			out string simplifiedInputAsLatex, 
			out List<string> stepsAsLatex, 
			out List<StepDescription?> stepDescriptions,
			SimplificationParams? simplificationParams = null
		)
		{
			if (simplificationParams == null)
				simplificationParams = SimplificationParams.Default;

			simplificationParams = simplificationParams with
			{
				varToDiff = varToDifferentiate
			};

			TreeNode tree = TreeUtils.CopyTree(input);

			inputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

			tree = TreeUtils.GetSimplestForm(tree, simplificationParams);

			simplifiedInputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

			TreeNode diffTree;

			//try
			//{
				diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate, simplificationParams);

				//Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));

				stepsAsLatex = Differentiator.steps;
				stepDescriptions = Differentiator.stepDescriptions;

				diffTree = TreeUtils.GetSimplestForm(diffTree, simplificationParams);
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine($"An error occured while differentiating! ({e.Message}) {e.StackTrace}");
			//	throw new DifferentiationException("An error occured while differentiating!");
			//}

			return diffTree.ToLatexString();
		}

		public static void DifferentiateFromConsole()
		{
			Console.Write("> ");
			string input = Console.ReadLine().ToLower().Trim();

			List<string> steps;
			List<StepDescription?> stepDescriptions;

			string prettyInput, prettySimplifiedInput;

			Console.Write("> ");
			DifferentiateString(input, out prettyInput, out prettySimplifiedInput, out steps, out stepDescriptions, out _); // will call a nicer writeline

			Console.WriteLine($"Pretty input: {prettyInput}");
			Console.WriteLine($"Pretty simplified input: {prettySimplifiedInput}");

			for (int i = 0; i < steps.Count; i++)
			{
				Console.WriteLine($"Step {i}:");
				Console.WriteLine(steps[i]);
				Console.WriteLine($"Step description {i}:");
				if (stepDescriptions.Count <= i)
					Console.WriteLine("out of range");
				else
					if (stepDescriptions[i] == null)
						Console.WriteLine("null");
					else
						Console.WriteLine($"{stepDescriptions[i].ruleNameAsLatex} f(x)={stepDescriptions[i].fxAsLatex} g(x)={stepDescriptions[i].gxAsLatex}");
			}
		}
	}
}
