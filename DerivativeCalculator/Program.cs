using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main ()
	{
		DifferentiateFromConsole();
	}

	public static void DifferentiateFromConsole ()
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
			nodes.ForEach(n => Console.WriteLine(n));

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

		Derivator derivator;

		try
		{
			derivator = new Derivator(varToDifferentiate);
			TreeNode diffTree = derivator.DifferentiateWithStepsRecorded(tree);
		} 
		catch (Exception e)
		{
			Console.WriteLine($"An error occured while differentiating! ({e.Message})" +
				$"{e.StackTrace}");
			return;
		}

		Console.WriteLine("");
		for (int i = 0; i < derivator.steps.Count; i++)
		{
			Console.WriteLine($"Step {i + 1}: {derivator.steps[i]}\n");
		}

	}
}