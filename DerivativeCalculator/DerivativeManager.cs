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
		public static string DifferentiateString(string input, out string inputAsLatex, out string simplifiedInputAsLatex)
		{
			simplifiedInputAsLatex = "";
			inputAsLatex = "";

			input = input.Trim().ToLower();

			char varToDifferentiate = 'x';

			if (Regex.IsMatch(input, "^d/d([a-d]|[f-z])"))
			{
				varToDifferentiate = input[3];
				input = input.Substring(4);
			}

			List<Node> nodes;
			TreeNode tree;

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
					throw new Exception("Input is empty, or the parser is unable to parse it!");
				}

				tree = Parser.MakeTreeFromList(nodes);

				inputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

				tree = TreeUtils.GetSimplestForm(tree);

				simplifiedInputAsLatex = new DerivativeSymbol(tree, varToDifferentiate).ToLatexString();

				if (tree == null)
				{
					Console.WriteLine("Parsing error: tree is empty!");
					throw new Exception("Parsing error: tree is empty!");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Parsing error!");
				throw new Exception("Parsing error!");
			}

			TreeNode diffTree;

			try
			{
				diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate);

				Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));

				diffTree = TreeUtils.GetSimplestForm(diffTree);
			}
			catch (Exception e)
			{
				Console.WriteLine($"An error occured while differentiating! ({e.Message})" +
					$"{e.StackTrace}");
				throw new Exception("An error occured while differentiating!");
			}

			return diffTree.ToLatexString();
		}

		public static void DifferentiateFromConsole()
		{
			Console.Write("> ");
			string input = Console.ReadLine().ToLower().Trim();

			Console.Write(">");
			Console.WriteLine(DifferentiateString(input, out _, out _));
		}
	}
}
