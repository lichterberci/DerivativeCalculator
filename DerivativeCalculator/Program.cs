using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		do
		{
			for (int i = 0; i < 100; i++)
			{
				if (i % 100 == 0)
					Console.WriteLine($"{i}/{1000}");

				var tree = ExerciseGenerator.GenerateRandomTree(DifficultyMetrics.Hardcore);

				string treeString = tree.ToLatexString();
				string diffString = tree.Diff('x').ToLatexString();

				//Console.WriteLine(treeString);
				//Console.WriteLine(diffString);

				if (treeString.ToLower().Contains("nan")) {

					Console.WriteLine("Found tree with error in treeString!");
					Console.WriteLine(treeString);

					TreeUtils.PrintTree(tree);

					Environment.Exit(0);
				}

				if (diffString.ToLower().Contains("nan")) {

					Console.WriteLine("Found tree with error in diffString!");
					Console.WriteLine(treeString);
					Console.WriteLine(diffString);

					TreeUtils.PrintTree(tree);

					Environment.Exit(0);
				}
			}

			Console.WriteLine("Batch is done without error!");
		}
		while (Console.ReadKey().Key != ConsoleKey.Q);
	}
}
