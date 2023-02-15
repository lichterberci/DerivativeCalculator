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

			if (string.IsNullOrEmpty(inputAsLatex))
				throw new ParsingError("A bemenet üres!");

			char varToDifferentiate = 'x';

			if (Regex.IsMatch(input, "^d/d([a-d]|[f-z])"))
			{
				varToDifferentiate = input[3];
				input = input.Substring(4);
			}

			varToDiff = varToDifferentiate;

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
					throw new ParsingError("Az AAST fa üres!");
				}
			}
			catch (ParsingError parsingError)
			{
				throw parsingError;
			}
			catch
			{
				Console.WriteLine("Parsing error!");
				throw new ParsingError("Nem sikerült beolvasni!");
			}

			return DifferentiateTree(tree, varToDifferentiate, out inputAsLatex, out simplifiedInputAsLatex, out stepsAsLatex, out stepDescriptions, simplificationParams);
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

			if (TreeUtils.DoesTreeContainNan(tree))
				throw new NotFiniteNumberException("A bemenet invalid értéket tartalmaz!");

			tree = TreeUtils.GetSimplestForm(tree, simplificationParams);

			if (TreeUtils.DoesTreeContainNan(tree))
				throw new NotFiniteNumberException("Az egyszerűsített bemenet invalid értéket tartalmaz!");

			simplifiedInputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

			var diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate, simplificationParams);

			if (TreeUtils.DoesTreeContainNan(tree))
				throw new NotFiniteNumberException("A derivált invalid értéket tartalmaz!");

			stepsAsLatex = Differentiator.steps;
			stepDescriptions = Differentiator.stepDescriptions;

			diffTree = TreeUtils.GetSimplestForm(diffTree, simplificationParams);

			if (TreeUtils.DoesTreeContainNan(tree))
				throw new NotFiniteNumberException("Az egyszerűsített derivált invalid értéket tartalmaz!");

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
