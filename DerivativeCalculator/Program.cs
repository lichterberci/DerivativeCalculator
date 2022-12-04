using System.Linq.Expressions;
using System.Text.RegularExpressions;

using DerivativeCalculator;

public partial class Program
{
	public static void Main()
	{

		//DerivativeManager.DifferentiateFromConsole();

		DifficultyMetrics difficulty = new()
		{
			numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
			{
				{OperatorType.Add,  10},
				{OperatorType.Sub,  10},
				{OperatorType.Mult, 10},
				{OperatorType.Div,  2},
				{OperatorType.Pow,  3},
				{OperatorType.Sin,  2},
				{OperatorType.Cos,  0},
				{OperatorType.Tan,  0},
				{OperatorType.Ln,   0},
				{OperatorType.Log,  0}
			},
			difficultyOfPower = DifficultyOfPower.Polinom,
			difficultyOfMultiplication = DifficultyOfMultiplication.OnlyConstant,
			numMinOperators = 2,
			numMaxOperators = 7,
			numMinLevelOfComposition = 0,
			numMaxLevelOfComposition = 2,
			numMinParameters = 0,
			numMaxParameters = 0,
			minConstValue = -5,
			maxConstValue = 5,
			constIsOnlyInt = true,
			parameterChance = 0.3f,
			shouldYieldNonZeroDiff = true,
			shouldYieldNonConstDiff = true
		};


		for (int i = 0; i < 1000; i++)
		{

			try
			{
				TreeNode generatedTree = ExerciseGenerator.GenerateRandomTree(difficulty);

				

				Console.WriteLine(TreeUtils.CollapseTreeToString(TreeUtils.GetSimplestForm(generatedTree)));

				Console.WriteLine(TreeUtils.CollapseTreeToString(TreeUtils.GetSimplestForm(generatedTree.Diff('x'))));

				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine();
			} 
			catch (DivideByZeroException)
			{

			}

			
		}
	}
}
