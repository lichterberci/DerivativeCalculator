using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		string result = DerivativeManager.DifferentiateString("tgx cosx", out _, out _, out _, out _, out _);
		Console.WriteLine(result);

		//return;

		//TreeNode tree = new Pow(
		//	Constant.E,
		//	new Mult(
		//		new Mult(
		//			  new Ln(new Variable('a')),
		//			  new Variable('b')
		//		  ),
		//		new Variable('x')
		//	)
		//);

		//var tree = Parser.ParseString("x^(3x)");

		//Console.WriteLine(tree.ToLatexString());

		//TreeNode tree = new Div(
		//		new Mult(
		//			  new Ln(new Variable('x')),
		//			  new Variable('y')
		//		  ),
		//		new Variable('z')
		//	);

		//tree.PrintToConsole();

		//Dictionary<char, TreeNode> wildcards;

		//if (TreeUtils.MatchPattern(
		//		tree,
		//		new Div(
		//			new Ln(new Wildcard('a')),
		//			new Wildcard('b')
		//		),
		//		out wildcards
		//))
		//{
		//	Console.WriteLine("Match");

		//	foreach (var (key, value) in wildcards)
		//	{
		//		Console.Write("var '" + key + "' := ");
		//		value.PrintToConsole();
		//	}
		//}
		//else
		//{
		//	Console.WriteLine("No match");
		//}


		//tree = tree.Simplify(SimplificationParams.Default);
		////tree = tree.GetSimplestForm(SimplificationParams.Default);

		//tree.PrintToConsole();


		//var left = new Mult(
		//	new Div(
		//		new Variable('a'),
		//		new Constant(2)
		//	),
		//	new Variable('b')
		//);

		//var right = new Div(
		//	new Mult(
		//		new Variable('a'),
		//		new Variable('b')
		//	),
		//	new Constant(2)
		//);

		//left.PrintToConsole();
		//right.PrintToConsole();

		//Console.WriteLine(TreeUtils.MatchPattern(left, right, out _));

		//const int maxIter = 2000;

		//for (int i = 0; i < maxIter; i++)
		//{
		//	if (i % 100 == 0)
		//		Console.WriteLine($"{i}/{maxIter}");

		//	var exercise = ExerciseGenerator.GenerateRandomTree(
		//		DifficultyMetrics.Easy,
		//		new SimplificationParams(
		//			  'x'
		//			)
		//	);

		//	DerivativeManager.DifferentiateTree(exercise, 'x', out _, out _, out _, out _, null);

		//	exercise.PrintToConsole();
		//	//exercise.Diff('x').PrintToConsole();
		//}

		//var list = new List<TreeNode>()
		//{
		//	new Variable('b'),
		//	new Constant(3),
		//	new Variable('c'),
		//	new Pow(new Variable('a'), new Variable('v')),
		//	new Mult(new Variable('j'), new Variable('v')),
		//	new Constant(1),
		//	new Variable('a'),
		//	new Variable('g'),
		//	new Variable('j'),
		//	new Variable('r'),
		//	new Variable('j'),
		//	new Variable('x'),
		//};

		//list = TreeUtils.SortNodesByVarNames(list, 'x');

		//list.ForEach(n => Console.WriteLine(n));
		//TreeNode tree = new Mult(
		//	new Sin(new Variable('a')),
		//	new Pow(
		//		new Cos(new Variable('a')),
		//		new Constant(-1)
		//	)
		//);

		////tree = new Mult(new Constant(1), new Mult(new Constant(-21), new Variable('x')));

		//Console.WriteLine(TreeUtils.CollapseTreeToString(tree));

		//TreeUtils.PrintTree(tree);

		//tree = TreeUtils.GetSimplestForm(tree);

		//Console.WriteLine(TreeUtils.CollapseTreeToString(tree));

		//TreeUtils.PrintTree(tree);

		//var sw = new Stopwatch();

		//const int iters = 10;

		//double[] times = new double[iters];

		//for (int i = 0; i < iters; i++)
		//{
		//	Console.WriteLine($"{i + 1}/{iters}");

		//	sw.Start();

		//	var tree = ExerciseGenerator.GenerateRandomTree(DifficultyMetrics.Hardcore);

		//	tree.ToLatexString();

		//	sw.Stop();

		//	times[i] = sw.ElapsedMilliseconds / 1000;

		//	sw.Reset();
		//}

		//Console.WriteLine($"min: {times.Min()}s");
		//Console.WriteLine($"max: {times.Max()}s");
		//Console.WriteLine($"avg: {times.Sum() / iters}s");
		//Console.WriteLine($"std: {Math.Sqrt(times.Select(t => Math.Pow(t - (times.Sum() / iters), 2)).ToArray().Sum())}s");
	}
}
