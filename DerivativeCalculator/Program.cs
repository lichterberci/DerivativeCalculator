using System.Linq.Expressions;
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

		DerivativeManager.DifferentiateFromConsole(false);


	}
}
