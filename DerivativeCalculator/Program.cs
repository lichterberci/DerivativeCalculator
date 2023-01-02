using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		TreeNode tree = new Sub(
			new Constant(0), 
			  new Mult(
					new Constant(1),
					new Variable('x')
					)
			  );

		TreeUtils.PrintTree(tree);

		tree = TreeUtils.GetSimplestForm(tree);

		TreeUtils.PrintTree(tree);
	}
}
