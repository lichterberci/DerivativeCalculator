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

		try
		{
			nodes = Parser.ParseToList(input);
			//nodes.ForEach(n => Console.WriteLine(n));
			nodes = Parser.ReplaceVarEWithConstE(nodes);
			nodes = Parser.HandleNegativeSigns(nodes);
			nodes = Parser.AddHiddenMultiplications(nodes);
			//nodes.ForEach(n => Console.WriteLine(n));
			nodes = Parser.ApplyParentheses(nodes);
		} 
		catch (Exception e)
		{
			Console.WriteLine("Parsing error!");
			return;
		}

		TreeNode tree;
		Derivator derivator;

		try
		{
			tree = Parser.MakeTreeFromList(nodes);
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