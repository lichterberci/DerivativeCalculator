using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public static class DerivativeManager
	{
		public static string DifferentiateString(string input, out string inputAsLatex, out string simplifiedInputAsLatex, out List<string> stepsAsLatex, out List<StepDescription> stepDescriptions, out char varToDiff)
		{
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

			var simplificationParameters = new SimplificationParams(varToDiff, null);

			try
			{
				nodes = Parser.ParseToList(input);
				
				nodes = Parser.ReplaceVarEWithConstE(nodes);
				nodes = Parser.HandleNegativeSigns(nodes);
				nodes = Parser.AddHiddenMultiplications(nodes);
				nodes = Parser.ApplyParentheses(nodes);

				if (nodes.Count == 0)
				{
					Console.WriteLine("Input is empty, or the parser is unable to parse it!");
					throw new ParsingError("Input is empty, or the parser is unable to parse it!");
				}

				tree = Parser.MakeTreeFromList(nodes);

				inputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

				tree = TreeUtils.GetSimplestForm(tree, simplificationParameters);

				simplifiedInputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

				if (tree == null)
				{
					Console.WriteLine("Parsing error: tree is empty!");
					throw new ParsingError("Parsing error: tree is empty!");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Parsing error!");
				throw new ParsingError("Parsing error!");
			}

			TreeNode diffTree;

			//diffTree = tree;

			try
			{
				diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate);

				Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));

				stepsAsLatex = Differentiator.steps;
				stepDescriptions = Differentiator.stepDescriptions;

				diffTree = TreeUtils.GetSimplestForm(diffTree, simplificationParameters);
			}
			catch (Exception e)
			{
				Console.WriteLine($"An error occured while differentiating! ({e.Message}) {e.StackTrace}");
				throw new DifferentiationException("An error occured while differentiating!");
			}

			return diffTree.ToLatexString();
		}

		public static string DifferentiateTree (TreeNode input, char varToDifferentiate, out string inputAsLatex, out string simplifiedInputAsLatex, out List<string> stepsAsLatex, out List<StepDescription?> stepDescriptions)
		{
			TreeNode tree = TreeUtils.CopyTree(input);

			inputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

			var simplificationParameters = new SimplificationParams();

			tree = TreeUtils.GetSimplestForm(tree, simplificationParameters);

			simplifiedInputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

			TreeNode diffTree;

			try
			{
				diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate);

				Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));

				stepsAsLatex = Differentiator.steps;
				stepDescriptions = Differentiator.stepDescriptions;

				diffTree = TreeUtils.GetSimplestForm(diffTree, simplificationParameters);
			}
			catch (Exception e)
			{
				Console.WriteLine($"An error occured while differentiating! ({e.Message}) {e.StackTrace}");
				throw new DifferentiationException("An error occured while differentiating!");
			}

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
