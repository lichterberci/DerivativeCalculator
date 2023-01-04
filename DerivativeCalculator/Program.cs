using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		TreeNode tree = new Div(
			new Pow(
				new Variable('a'),
				new Variable('c')
			),
			new Pow(
				new Variable('b'),
				new Variable('c')
			)
		);

		//tree = new Mult(new Constant(1), new Mult(new Constant(-21), new Variable('x')));

		TreeUtils.PrintTree(tree);

		tree = TreeUtils.GetSimplestForm(tree);

		TreeUtils.PrintTree(tree);
	}
}
