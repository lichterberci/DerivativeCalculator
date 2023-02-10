using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		//TreeNode tree = 
		//TreeNode tree = new Cos(new Add(new Variable('a'), new Variable('b')));
        //x^(logx/loga)
        //TreeNode tree = new Pow(new Variable('x'),new Div(new Log(new Variable('x')),new Log(new Variable('a'))));
        //2sinxcosx
        //TreeNode tree = new Mult(new Mult(new Sin(new Variable('x')),new Cos(new Variable('x'))),new Constant(2));
        //cosacosb-sinasinb
        //TreeNode tree = new Sub(new Mult(new Cos(new Variable('a')),new Cos(new Variable('b'))), new Mult(new Sin(new Variable('a')), new Sin(new Variable('b'))));
        //ln(x*a), ln(x/a), ln(a*b)
        //TreeNode tree = new Ln(new Mult(new Variable('x'),new Variable('a')));
        //TreeNode tree = new Ln(new Div(new Variable('x'), new Variable('a')));
        //TreeNode tree = new Ln(new Mult(new Variable('a'), new Variable('b')));



        Console.WriteLine(TreeUtils.CollapseTreeToString(tree));


        tree = TreeUtils.GetSimplestForm(tree);

        Console.WriteLine(TreeUtils.CollapseTreeToString(tree));
	}
}
