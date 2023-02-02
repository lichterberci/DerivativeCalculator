using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public enum DifficultyOfPower
	{
		Polinom, PolinomOrSimpleExponential, BothCanBeDependent
	}

	public enum DifficultyOfMultiplication
	{
		OnlyConstant, BothCanBeDependent
	}

	public struct DifficultyMetrics
	{
		public Dictionary<OperatorType, int> numAllowedFromEachOperatorType;
		public DifficultyOfPower difficultyOfPower;
		public DifficultyOfMultiplication difficultyOfMultiplication;
		public int numMinOperators;
		public int numMaxOperators;
		public int numMinLevelOfComposition;
		public int numMaxLevelOfComposition;
		public int numMinParameters;
		public int numMaxParameters;
		public int minConstValue;
		public int maxConstValue;
		public bool constIsOnlyInt;
		public float parameterChance;
		public bool shouldYieldNonZeroDiff;
		public bool shouldYieldNonConstDiff;
		public bool absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX;

		public static readonly DifficultyMetrics Easy;
		public static readonly DifficultyMetrics Medium;
		public static readonly DifficultyMetrics Hard;
		public static readonly DifficultyMetrics Hardcore;

		public static readonly DifficultyMetrics Test;

		static DifficultyMetrics()
		{
			Easy = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{ OperatorType.Add,		6 },
					{ OperatorType.Sub,		3 },
					{ OperatorType.Mult,	5 },
					{ OperatorType.Div,		1 },
					{ OperatorType.Pow,		2 }
				},
				difficultyOfPower = DifficultyOfPower.Polinom,
				difficultyOfMultiplication = DifficultyOfMultiplication.OnlyConstant,
				numMinOperators = 2,
				numMaxOperators = 7,
				numMinLevelOfComposition = 0,
				numMaxLevelOfComposition = 1,
				numMinParameters = 0,
				numMaxParameters = 0,
				minConstValue = -5,
				maxConstValue = 5,
				constIsOnlyInt = true,
				parameterChance = 0.0f,
				shouldYieldNonZeroDiff = true,
				shouldYieldNonConstDiff = false,
				absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX = true
			};

			Medium = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{OperatorType.Add,  10},
					{OperatorType.Sub,  4},
					{OperatorType.Mult, 7},
					{OperatorType.Div,  3},
					{OperatorType.Pow,  5}
				},
				difficultyOfPower = DifficultyOfPower.PolinomOrSimpleExponential,
				difficultyOfMultiplication = DifficultyOfMultiplication.BothCanBeDependent,
				numMinOperators = 5,
				numMaxOperators = 10,
				numMinLevelOfComposition = 0,
				numMaxLevelOfComposition = 1,
				numMinParameters = 0,
				numMaxParameters = 0,
				minConstValue = -10,
				maxConstValue = 10,
				constIsOnlyInt = true,
				parameterChance = 0.0f,
				shouldYieldNonZeroDiff = true,
				shouldYieldNonConstDiff = true,
				absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX = true
			};

			Hard = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{OperatorType.Add,  6},
					{OperatorType.Sub,  2},
					{OperatorType.Mult, 10},
					{OperatorType.Div,  3},
					{OperatorType.Pow,  4},
					{OperatorType.Sin,  1},
					{OperatorType.Cos,  1},
					{OperatorType.Tan,  1},
					{OperatorType.Ln,   1},
					{OperatorType.Abs,  1}
				},
				difficultyOfPower = DifficultyOfPower.BothCanBeDependent,
				difficultyOfMultiplication = DifficultyOfMultiplication.BothCanBeDependent,
				numMinOperators = 7,
				numMaxOperators = 12,
				numMinLevelOfComposition = 1,
				numMaxLevelOfComposition = 2,
				numMinParameters = 0,
				numMaxParameters = 1,
				minConstValue = -20,
				maxConstValue = 20,
				constIsOnlyInt = true,
				parameterChance = 0.3f,
				shouldYieldNonZeroDiff = true,
				shouldYieldNonConstDiff = true,
				absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX = false
			};

			Hardcore = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{OperatorType.Add,      10},
					{OperatorType.Sub,      5},
					{OperatorType.Mult,     10},
					{OperatorType.Div,      7},
					{OperatorType.Pow,      7},
					{OperatorType.Log,      3},
					{OperatorType.Ln,       3},
					{OperatorType.Sin,      3},
					{OperatorType.Cos,      3},
					{OperatorType.Tan,      3},
					{OperatorType.Cot,      3},
					{OperatorType.Arcsin,   2},
					{OperatorType.Arccos,   2},
					{OperatorType.Arctan,   2},
					{OperatorType.Arccot,   2},
					{OperatorType.Sinh,     2},
					{OperatorType.Cosh,     2},
					{OperatorType.Tanh,     2},
					{OperatorType.Coth,     2},
					{OperatorType.Arsinh,   2},
					{OperatorType.Arcosh,   2},
					{OperatorType.Artanh,   2},
					{OperatorType.Arcoth,   2},
					{OperatorType.Abs,		2},
				},
				difficultyOfPower = DifficultyOfPower.BothCanBeDependent,
				difficultyOfMultiplication = DifficultyOfMultiplication.BothCanBeDependent,
				numMinOperators = 10,
				numMaxOperators = 20,
				numMinLevelOfComposition = 2,
				numMaxLevelOfComposition = 5,
				numMinParameters = 1,
				numMaxParameters = 4,
				minConstValue = -20,
				maxConstValue = 20,
				constIsOnlyInt = false,
				parameterChance = 0.6f,
				shouldYieldNonZeroDiff = true,
				shouldYieldNonConstDiff = true,
				absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX = false
			};

			Test = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{OperatorType.Mult,  2 },
					{OperatorType.Ln,    1 }
				},
				difficultyOfPower = DifficultyOfPower.PolinomOrSimpleExponential,
				difficultyOfMultiplication = DifficultyOfMultiplication.BothCanBeDependent,
				numMinOperators = 3,
				numMaxOperators = 3,
				numMinLevelOfComposition = 1,
				numMaxLevelOfComposition = 2,
				numMinParameters = 0,
				numMaxParameters = 0,
				minConstValue = -10,
				maxConstValue = 10,
				constIsOnlyInt = true,
				parameterChance = 0.0f,
				shouldYieldNonZeroDiff = true,
				shouldYieldNonConstDiff = true,
				absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX = true
			};
		}
	}
}
