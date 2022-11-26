﻿using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//var list = new List<TreeNode> {
		//	new Constant(0),
		//	new Constant(2),
		//	new Constant(1),
		//	new Constant(3)
		//};

		//AssociativeOperator assoc = new AssociativeOperator(OperatorType.Mult, list);

		//TreeNode tree = assoc.BuildBackBinaryTree();

		//TreeUtils.PrintTree(tree);

		Manager.DifferentiateFromConsole(false);


	}
}

namespace DerivativeCalculator {

	public static class Manager
	{
		public static string DifferentiateString(string input)
		{
			char varToDifferentiate = 'x';
			if (Regex.IsMatch(input, "^d/d([a-d]|[f-z]|[A-D]|[F-Z])"))
			{
				varToDifferentiate = input[3];
				input = input.Substring(4);
			}

			List<Node> nodes;
			TreeNode tree;

			try
			{
				nodes = Parser.ParseToList(input);
				//nodes.ForEach(n => Console.WriteLine(n));
				nodes = Parser.ReplaceVarEWithConstE(nodes);
				//nodes.ForEach(n => Console.WriteLine(n));
				nodes = Parser.HandleNegativeSigns(nodes);
				nodes = Parser.AddHiddenMultiplications(nodes);
				nodes = Parser.ApplyParentheses(nodes);
				//nodes.ForEach(n => Console.WriteLine(n));

				if (nodes.Count == 0)
				{
					Console.WriteLine("Input is empty, or the parser is unable to parse it!");
					return "";
				}

				tree = Parser.MakeTreeFromList(nodes);

				if (tree == null)
				{
					Console.WriteLine("Parsing error: tree is empty!");
					return "";
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Parsing error!");
				return "";
			}

			try
			{
				TreeNode diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate);
			}
			catch (Exception e)
			{
				Console.WriteLine($"An error occured while differentiating! ({e.Message})" +
					$"{e.StackTrace}");
				return "";
			}


			return Differentiator.steps.Last();
		}

		public static void DifferentiateFromConsole(bool withSteps = true)
		{
			Console.Write("> ");
			string input = Console.ReadLine().ToLower().Trim();

			char varToDifferentiate = 'x';
			if (Regex.IsMatch(input, "^d/d([a-d]|[f-z]|[A-D]|[F-Z])"))
			{
				varToDifferentiate = input[3];
				input = input.Substring(4);
			}

			List<Node> nodes;
			TreeNode tree;

			try
			{
				nodes = Parser.ParseToList(input);
				//nodes.ForEach(n => Console.WriteLine(n));
				nodes = Parser.ReplaceVarEWithConstE(nodes);
				//nodes.ForEach(n => Console.WriteLine(n));
				nodes = Parser.HandleNegativeSigns(nodes);
				nodes = Parser.AddHiddenMultiplications(nodes);
				nodes = Parser.ApplyParentheses(nodes);
				//nodes.ForEach(n => Console.WriteLine(n));

				if (nodes.Count == 0)
				{
					Console.WriteLine("Input is empty, or the parser is unable to parse it!");
					return;
				}

				tree = Parser.MakeTreeFromList(nodes);

				if (tree == null)
				{
					Console.WriteLine("Parsing error: tree is empty!");
					return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Parsing error!");
				return;
			}

			TreeNode diffTree;

			try
			{
				if (withSteps)
					diffTree = Differentiator.DifferentiateWithStepsRecorded(tree, varToDifferentiate);
				else
					diffTree = Differentiator.Differentiate(tree, varToDifferentiate);
			}
			catch (Exception e)
			{
				Console.WriteLine($"An error occured while differentiating! ({e.Message})" +
					$"{e.StackTrace}");
				return;
			}

			if (withSteps)
			{
				Console.WriteLine("");
				for (int i = 0; i < Differentiator.steps.Count; i++)
				{
					Console.WriteLine($"Step {i + 1}: {Differentiator.steps[i]}\n");
				}
			} 
			else
			{
				diffTree = TreeUtils.GetSimplestForm(diffTree);

				Console.Write("> ");
				Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));
			}
		}
	} 
}