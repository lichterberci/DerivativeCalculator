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
				{OperatorType.Add,  4},
				{OperatorType.Sub,  3},
				{OperatorType.Mult, 2},
				{OperatorType.Div,  2},
				{OperatorType.Pow,  3},
				{OperatorType.Sin,  1},
				{OperatorType.Cos,  1},
				{OperatorType.Tan,  0},
				{OperatorType.Ln,   0},
				{OperatorType.Log,  0}
			},
			difficultyOfPower = DifficultyOfPower.BothCanBeDependent,
			difficultyOfMultiplication = DifficultyOfMultiplication.OnlyConstant,
			numMinOperators = 3,
			numMaxOperators = 10,
			numMinLevelOfComposition = 0,
			numMaxLevelOfComposition = 4,
			numMinParameters = 0,
			numMaxParameters = 3,
			minConstValue = -10,
			maxConstValue = 10,
			constIsOnlyInt = true,
			parameterChance = 0.3f
		};

		for (int i = 0; i < 100; i++)
		{
			TreeNode generatedTree = ExerciseGenerator.GenerateRandomTree(difficulty);
			Console.WriteLine(TreeUtils.CollapseTreeToString(generatedTree));
		}
	}
}
