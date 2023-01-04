﻿using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		//TreeNode tree = new Div(
		//	new Pow(
		//		new Variable('a'),
		//		new Variable('c')
		//	),
		//	new Pow(
		//		new Variable('b'),
		//		new Variable('c')
		//	)
		//);

		////tree = new Mult(new Constant(1), new Mult(new Constant(-21), new Variable('x')));

		//TreeUtils.PrintTree(tree);

		//tree = TreeUtils.GetSimplestForm(tree);

		//TreeUtils.PrintTree(tree);

		var sw = new Stopwatch();

		const int iters = 10;

		double[] times = new double[iters];

		for (int i = 0; i < iters; i++)
		{
			Console.WriteLine($"{i + 1}/{iters}");

			sw.Start();

			var tree = ExerciseGenerator.GenerateRandomTree(DifficultyMetrics.Hardcore);

			tree.ToLatexString();

			sw.Stop();

			times[i] = sw.ElapsedMilliseconds / 1000;

			sw.Reset();
		}

		Console.WriteLine($"min: {times.Min()}s");
		Console.WriteLine($"max: {times.Max()}s");
		Console.WriteLine($"avg: {times.Sum() / iters}s");
		Console.WriteLine($"std: {Math.Sqrt(times.Select(t => Math.Pow(t - (times.Sum() / iters), 2)).ToArray().Sum())}s");
	}
}
