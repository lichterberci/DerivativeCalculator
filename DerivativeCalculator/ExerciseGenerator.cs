﻿using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;

namespace DerivativeCalculator
{
    
    public class PlaceHolderLeaf : TreeNode
    {
        public bool canBeConstant;
        public bool canBeX;
        public double minValue;
        public bool isMinValueInclusive;
        public double maxValue; // inclusive
        public bool cannotBeExpressionOnlyMultipleOfX; // 2x is fine, but 3x-2 or sin(x) are not

        public PlaceHolderLeaf(bool canBeConstant, bool canBeX, bool cannotBeExpressionOnlyMultipleOfX = false)
        {
            this.canBeConstant = canBeConstant;
            this.canBeX = canBeX;

            this.minValue = double.MinValue;
            this.isMinValueInclusive = true;
            this.maxValue = double.MaxValue;
            this.cannotBeExpressionOnlyMultipleOfX = cannotBeExpressionOnlyMultipleOfX;
        }

        public PlaceHolderLeaf(bool canBeConstant, bool canBeX, double minValue, bool isMinValueInclusive, bool cannotBeExpressionOnlyMultipleOfX = false)
		{
			this.canBeConstant = canBeConstant;
			this.canBeX = canBeX;
			this.minValue = minValue;
            this.isMinValueInclusive = isMinValueInclusive;

			this.maxValue = double.MaxValue;
            this.cannotBeExpressionOnlyMultipleOfX = cannotBeExpressionOnlyMultipleOfX;
		}

		public PlaceHolderLeaf(bool canBeConstant, bool canBeX, double minValue, bool isMinValueInclusive, double maxValue, bool cannotBeExpressionOnlyMultipleOfX = false)
		{
			this.canBeConstant = canBeConstant;
			this.canBeX = canBeX;
			this.minValue = minValue;
			this.isMinValueInclusive = isMinValueInclusive;

			this.maxValue = maxValue;
			this.cannotBeExpressionOnlyMultipleOfX = cannotBeExpressionOnlyMultipleOfX;
		}

		public override string ToString()
        {
			string min = minValue == double.MinValue ? "MIN" : minValue.ToString();
			string max = maxValue == double.MaxValue ? "MAX" : maxValue.ToString();

			string range = $"{(isMinValueInclusive ? "[" : "(")}{min}; {max}]";

            if (cannotBeExpressionOnlyMultipleOfX)
            {
			    if (canBeX && canBeConstant)
                    return $"Placeholder(x | c {range})";
                else if (canBeX)
                    return "Placeholder(x)";
                else
                    return $"Placeholder(c {range})";
            }
            else
            {
				if (canBeX && canBeConstant)
					return $"Placeholder(f(x) | c {range})";
				else if (canBeX)
					return "Placeholder(f(x))";
				else
					return $"Placeholder(c {range})";
			}
        }

        public bool IsConstantValid (TreeNode root)
        {
            if (canBeConstant == false)
                return true;

            if (root is not Constant)
                return true;

            double value = (root as Constant).value;

            return (
                isMinValueInclusive 
                ? value >= minValue 
                : value > minValue
            ) 
            && value <= maxValue;
        }

        public Constant GenerateValidConstant (Random random, double extMin, double extMax, bool onlyInt)
        {
            if (onlyInt)
            {
                int min = (int)Math.Max(extMin, minValue);
                int max = (int)Math.Min(extMax, maxValue);

                if (min > max)
                    return new Constant((int)minValue);

                if (min == max)
                    return new Constant(min);

                int value = random.Next() % (max - min) + min;

                if (isMinValueInclusive == false && value == minValue)
                    value += 1;

				return new Constant(value);
            }
            else
            {
                double min = Math.Max(extMin, minValue);
                double max = Math.Min(extMax, maxValue);

                if (min > max)
					return new Constant(minValue);

				if (min == max)
					return new Constant(min);

                double value = isMinValueInclusive ? random.NextDouble() * (max - min) + min : random.NextDouble() * (max - min) + min + 1e-30;

				return new Constant(value);
            }
		}
	}

    public static class ExerciseGenerator
    {
        private static Random random;
        private static readonly OperatorType[] operatorTypesThatCountAsComposition;

		static ExerciseGenerator ()
        {
            random = new Random(DateTime.Now.Millisecond);

            operatorTypesThatCountAsComposition = new OperatorType[]
            {
                OperatorType.Mult,
                OperatorType.Div,
				OperatorType.Pow,
                OperatorType.Ln, 
                OperatorType.Log, 
                OperatorType.Sin, 
                OperatorType.Cos, 
                OperatorType.Tan, 
                OperatorType.Cot,
				OperatorType.Arcsin,
				OperatorType.Arccos,
				OperatorType.Arctan,
				OperatorType.Arccot,
				OperatorType.Sinh,
				OperatorType.Cosh,
				OperatorType.Tanh,
				OperatorType.Coth,
				OperatorType.Arsinh,
				OperatorType.Arcosh,
				OperatorType.Artanh,
				OperatorType.Arcoth,
				OperatorType.Abs
			};
		}

        private static List<OperatorType> GenerateOperatorList (DifficultyMetrics difficulty)
        {
            Dictionary<OperatorType, int> numOperandsRemainingFromType = new Dictionary<OperatorType, int>();

            // clone dictionary
            foreach ((var key, var value) in difficulty.numAllowedFromEachOperatorType)
                numOperandsRemainingFromType[key] = value;

            List<OperatorType> result = new List<OperatorType>();

            int sumOfAllowedOperands = numOperandsRemainingFromType.Values.Sum();

            int numOperatorsToChoose = Math.Min(difficulty.numMaxOperators, sumOfAllowedOperands) != difficulty.numMinOperators ? random.Next() % (Math.Min(difficulty.numMaxOperators, sumOfAllowedOperands) - difficulty.numMinOperators) + difficulty.numMinOperators : difficulty.numMinOperators;

            if (sumOfAllowedOperands < numOperatorsToChoose)
            {
                throw new ExerciseCouldNotBeGeneratedException("There are not enough operators to choose from!");
            }

            for (int i = 0; i < numOperatorsToChoose; i++)
            {
                int randomIndex = random.Next() % sumOfAllowedOperands;
                int runningSum = 0;

                foreach ((OperatorType key, int value) in numOperandsRemainingFromType)
                {
                    if (runningSum + value > randomIndex)
                    {
                        // we have found it
                        result.Add(key);
                        numOperandsRemainingFromType[key]--;
                        break;
                    }
                    else
                    {
                        runningSum += value;
                    }
                }

                sumOfAllowedOperands--;
            }

            return result;
        }

        private static (TreeNode resultTree, bool wasSuccessful) AddOperatorToTree(TreeNode root, OperatorType type, DifficultyMetrics difficulty)
        {		

			Operator newOp = Operator.GetClassInstanceFromType(type);

            switch (type)
            {
                case OperatorType.Pow:
                    (newOp.operand1, newOp.operand2) = difficulty.difficultyOfPower switch
                    {
                        DifficultyOfPower.Polinom => (new PlaceHolderLeaf(false, true), new PlaceHolderLeaf(true, false)),
                        DifficultyOfPower.PolinomOrSimpleExponential => random.Next() % 2 == 0 ?
                                                                    (new PlaceHolderLeaf(false, true), new PlaceHolderLeaf(true, false)) :
                                                                    (new PlaceHolderLeaf(true, false, 0, false), new PlaceHolderLeaf(false, true, true)),
                        _ => (new PlaceHolderLeaf(true, true, 0, false), new PlaceHolderLeaf(true, true))
                    };
                    break;
                case OperatorType.Mult:
                    (newOp.operand1, newOp.operand2) = difficulty.difficultyOfMultiplication switch
                    {
                        DifficultyOfMultiplication.OnlyConstant => (new PlaceHolderLeaf(true, true, true), new PlaceHolderLeaf(false, true)),
                        _ => (new PlaceHolderLeaf(true, true), new PlaceHolderLeaf(false, true))
                    };
                    break;
                case OperatorType.Div:
                    newOp.operand1 = new PlaceHolderLeaf(true, true);
                    newOp.operand2 = new PlaceHolderLeaf(true, true, 0, false);
                    break;
				default:
                    if (newOp.numOperands == 1)
                    {
                        if (difficulty.absTrigHypLogFunctionsCanOnlyContainMultiplesOfXOrX)
                            newOp.operand1 = new PlaceHolderLeaf(false, true, true);
                        else
                            newOp.operand1 = new PlaceHolderLeaf(false, true);
                    }
                    else
                    {
                        if (random.Next() % 2 == 0)
                        {
                            newOp.operand1 = new PlaceHolderLeaf(true, true);
                            newOp.operand2 = new PlaceHolderLeaf(false, true);
                        }
                        else
                        {
                            newOp.operand1 = new PlaceHolderLeaf(false, true);
                            newOp.operand2 = new PlaceHolderLeaf(true, true);
                        }
                    }
                    break;
            }

			if (root is null)
				return (newOp, true);

			if (root is not Operator op)
				return (root, false);

			if (op.numOperands == 1)
            {
				if (op.operand1 is Operator)
				{
					(var tree, bool wasSuccessful) = AddOperatorToTree(op.operand1, type, difficulty);

					if (wasSuccessful)
                    {
                        op.operand1 = tree;
						return (root, true);
                    }
				}

				if (op.operand1 is PlaceHolderLeaf { canBeX: true })
                {
                    // can only place c*x, so everything elso must not be considered
                    if (op.operand1 is PlaceHolderLeaf { cannotBeExpressionOnlyMultipleOfX: true })
                    {
                        if (newOp is not Mult)
                            return (root, false);

						// regardless of the current configuration, it will be a c*x

						newOp.operand1 = new PlaceHolderLeaf(true, false, false);
						newOp.operand2 = new PlaceHolderLeaf(false, true, true);
					}

                    op.operand1 = newOp;
                    return (root, true);
                }
            }
            else
            {

                bool checkOperand2First = random.Next() % 2 == 0;

                if (checkOperand2First)
                {
                    if (op.operand2 is Operator)
                    {
                        (var tree, bool wasSuccessful) = AddOperatorToTree(op.operand2, type, difficulty);

                        if (wasSuccessful)
                        {
    						op.operand2 = tree;
                            return (root, true);
                        }
                    }

                    if (op.operand2 is PlaceHolderLeaf { canBeX: true, cannotBeExpressionOnlyMultipleOfX: false })
                    {
                        op.operand2 = newOp;
                        return (root, true);
					}

                    if (op.operand2 is PlaceHolderLeaf { canBeX: true, cannotBeExpressionOnlyMultipleOfX: true } && newOp is Mult)
                    {
						// regardless of the current configuration, it will be a c*x

						newOp.operand1 = new PlaceHolderLeaf(true, false, false);
						newOp.operand2 = new PlaceHolderLeaf(false, true, true);

						op.operand2 = newOp;
                        return (root, true);
					}
                }

                if (op.operand1 is Operator)
                {
                    (var tree, bool wasSuccessful) = AddOperatorToTree(op.operand1, type, difficulty);

					if (wasSuccessful)
                    {
					    op.operand1 = tree;
                        return (root, true);
                    }
                }

				if (op.operand1 is PlaceHolderLeaf { canBeX: true, cannotBeExpressionOnlyMultipleOfX: false })
				{
					op.operand1 = newOp;
					return (root, true);
				}

				if (op.operand1 is PlaceHolderLeaf { canBeX: true, cannotBeExpressionOnlyMultipleOfX: true } && newOp is Mult)
				{
                    // regardless of the current configuration, it will be a c*x

                    newOp.operand1 = new PlaceHolderLeaf(true, false, false);
                    newOp.operand2 = new PlaceHolderLeaf(false, true, true);

					op.operand1  = newOp;
					return (root, true);
				}

				if (op.operand2 is Operator)
                {
                    (var tree, bool wasSuccessful) = AddOperatorToTree(op.operand2, type, difficulty);

					if (wasSuccessful)
                    {
                        op.operand2 = tree;
                        return (root, true);
                    }
                }

				if (op.operand2 is PlaceHolderLeaf { canBeX: true, cannotBeExpressionOnlyMultipleOfX: false })
				{
					op.operand2 = newOp;
					return (root, true);
				}

				if (op.operand2 is PlaceHolderLeaf { canBeX: true, cannotBeExpressionOnlyMultipleOfX: true } && newOp is Mult)
				{
					// regardless of the current configuration, it will be a c*x

					newOp.operand1 = new PlaceHolderLeaf(true, false, false);
					newOp.operand2 = new PlaceHolderLeaf(false, true, true);

					op.operand2 = newOp;
					return (root, true);
				}
			}

			return (root, false);
		}
    
		private static int MaxLevelOfComposition (TreeNode root, int depth = 0)
        {
            if (root is null)
                return depth;

            if (root is not Operator op)
                return depth;

            int newDepth = depth;

            if (operatorTypesThatCountAsComposition.Contains(op.type))
            {
                if (op.type == OperatorType.Mult)
                {   
                    if (op.operand1 is Operator && op.operand2 is Operator)
						newDepth = depth + 1;
                }
                else if (op.type == OperatorType.Div)
                {
                    if (op.operand2 is not Constant && op.operand2 is not PlaceHolderLeaf { canBeX: false, canBeConstant: true })
                        newDepth = depth + 1;
                }
                else
                {
			        newDepth = depth + 1;
                }
			}

			if (op.numOperands == 1)
            {
                return MaxLevelOfComposition(op.operand1, newDepth);
            }
            else
            {
                return Math.Max(MaxLevelOfComposition(op.operand1, newDepth), MaxLevelOfComposition(op.operand2, newDepth));
            }
        }

		private static (int numMix, int numX, int numConst) CountNumLeaves (TreeNode root, DifficultyMetrics difficulty)
        {
            if (root is null)
                return (0, 0, 0);

            if (root is not Operator op)
            {
                if (root is not PlaceHolderLeaf ph)
                    throw new ExerciseCouldNotBeGeneratedException("root is of invalid type!");

                return (ph.canBeConstant, ph.canBeX, ph.cannotBeExpressionOnlyMultipleOfX) switch
                {
                    (_,     _,     true ) => (0, 1, 0),
                    (false, true,  false) => (0, 1, 0),
                    (true,  false, false) => (0, 0, 1),
                    (true,  true,  false) => (1, 0, 0),
                    (false, false, false) => (0, 0, 0)
				};
            }

            if (op.numOperands == 1)
            {
                return CountNumLeaves(op.operand1, difficulty);
            }
            else
            {
                (int numMix1, int numX1, int numConst1) = CountNumLeaves(op.operand1, difficulty);
				(int numMix2, int numX2, int numConst2)  = CountNumLeaves(op.operand2, difficulty);

                return (numMix1 + numMix2, numX1 + numX2, numConst1 + numConst2);
            }
		}

        private static List<TreeNode> MakeLeafList (DifficultyMetrics difficulty, int numMix, int numX, int numConst)
        {
            List<TreeNode> result = new List<TreeNode>();

            List<char> parameterNameList = new List<char>();
    
            int numParametersToChoose = difficulty.numMaxParameters != difficulty.numMinParameters ? random.Next() % (difficulty.numMaxParameters - difficulty.numMinParameters) + difficulty.numMinParameters : difficulty.numMinParameters;

            const int numLetters = 26;

            while (parameterNameList.Count < numParametersToChoose)
            {
                char chosenParameterName = (char)((random.Next() % numLetters) + 'a');

                if (chosenParameterName == 'x' || chosenParameterName == 'e')
                    continue;

                if (parameterNameList.Contains(chosenParameterName))
                    continue;

                parameterNameList.Add(chosenParameterName);
            }

            Func<List<TreeNode>, List<TreeNode>> AddConstOrParamToList = resList =>
            {
                int numParametersUsed = resList.Where(node => node is Variable && node is not Variable { name: 'x' }).Count();

                if (numParametersUsed < numParametersToChoose)
                {
                    resList.Add(new Variable(parameterNameList[numParametersUsed]));
                    return resList;
                }

				if (random.NextSingle() < difficulty.parameterChance && numParametersToChoose > 0)
				{
					int chosenParamIndex = random.Next() % parameterNameList.Count;
					char chosenParamName = parameterNameList[chosenParamIndex];
					resList.Add(new Variable(chosenParamName));
				}
				else
				{
					if (difficulty.constIsOnlyInt)
					{
						int value = difficulty.maxConstValue != difficulty.minConstValue 
                                ? random.Next() % (difficulty.maxConstValue - difficulty.minConstValue) + difficulty.minConstValue 
                                : difficulty.minConstValue;
						resList.Add(new Constant(value));
					}
					else
					{
						double value = difficulty.maxConstValue != difficulty.minConstValue 
                                ? random.NextDouble() * (difficulty.maxConstValue - difficulty.minConstValue) + difficulty.minConstValue 
                                : difficulty.minConstValue;
						resList.Add(new Constant(value));
					}
				}

                return resList;
			};

            for (int i = 0; i < numX; i++)
                result.Add(new Variable('x'));

            for (int i = 0; i < numConst; i++)
            {
                result = AddConstOrParamToList(result);
			}

            for (int i = 0; i < numMix; i++)
            {
                if (random.Next() % 2 == 0)
                {
                    result.Add(new Variable('x'));
                }
                else
                {
                    result = AddConstOrParamToList(result);
                }
            }

            return result;
		}

        private static (TreeNode resulTree, bool wasSuccessful) AddLeafToTree (TreeNode root, TreeNode leafNode, bool onlyX, bool onlyConst, DifficultyMetrics difficulty)
        {
            bool leafIsX = leafNode is Variable { name: 'x' };

            if (root is null)
                return (root, false);

            if (root is not Operator op)
                return (root, false);

            if (op.numOperands == 1)
            {
                if (op.operand1 is PlaceHolderLeaf ph1)
                {   
                    if (leafIsX == false && ph1.IsConstantValid(leafNode) == false)
						leafNode = ph1.GenerateValidConstant(random, difficulty.minConstValue, difficulty.maxConstValue, difficulty.constIsOnlyInt);

                    if (onlyX)
                    {
						if (
                            (ph1.canBeX && ph1.canBeConstant == false && leafIsX)
                            || (ph1.cannotBeExpressionOnlyMultipleOfX && leafIsX)
                        )
						{
							//Console.WriteLine($"{ph1} -> {leafNode}");
							op.operand1 = leafNode;
							return (root, true);
						}
					}
                    else if (onlyConst)
                    {
						if (ph1.canBeConstant && ph1.canBeX == false && leafIsX == false)
						{
							//Console.WriteLine($"{ph1} -> {leafNode}");
							op.operand1 = leafNode;
							return (root, true);
						}
                    }
                    else if (ph1.cannotBeExpressionOnlyMultipleOfX)
                    {
                        if (leafIsX)
                        {
                            op.operand1 = leafNode;
                            return (root, true);
                        }
					}
                    else
                    {
						if (
						    (ph1.canBeX && leafIsX)
						    || (ph1.canBeConstant && !leafIsX)
					    )
						{
							//Console.WriteLine($"{ph1} -> {leafNode}");
							op.operand1 = leafNode;
							return (root, true);
						}
                    }
				}
            }
            else
            {

				if (op.operand1 is PlaceHolderLeaf ph1)
                {
				    if (leafIsX == false && ph1.IsConstantValid(leafNode) == false)
					    leafNode = ph1.GenerateValidConstant(random, difficulty.minConstValue, difficulty.maxConstValue, difficulty.constIsOnlyInt);

					if (onlyX)
					{
						if (
                            (ph1.canBeX && ph1.canBeConstant == false && leafIsX)
							|| (ph1.cannotBeExpressionOnlyMultipleOfX && leafIsX)
						)
						{
							//Console.WriteLine($"{ph1} -> {leafNode}");
							op.operand1 = leafNode;
							return (root, true);
						}
					}
					else if (onlyConst)
					{
						if (ph1.canBeConstant && ph1.canBeX == false && leafIsX == false)
						{
							//Console.WriteLine($"{ph1} -> {leafNode}");
							op.operand1 = leafNode;
							return (root, true);
						}
					}
					else if (ph1.cannotBeExpressionOnlyMultipleOfX)
					{
						if (leafIsX)
						{
							op.operand1 = leafNode;
							return (root, true);
						}
					}
					else
					{
						if (
							(ph1.canBeX && leafIsX)
							|| (ph1.canBeConstant && !leafIsX)
						)
						{
							//Console.WriteLine($"{ph1} -> {leafNode}");
							op.operand1 = leafNode;
							return (root, true);
						}
					}
				}


				if (op.operand2 is PlaceHolderLeaf ph2)
				{
				    if (leafIsX == false && ph2.IsConstantValid(leafNode) == false)
					    leafNode = ph2 .GenerateValidConstant(random, difficulty.minConstValue, difficulty.maxConstValue, difficulty.constIsOnlyInt);

					if (onlyX)
					{
						if (
                            (ph2.canBeX && ph2.canBeConstant == false && leafIsX)
                            || (ph2.cannotBeExpressionOnlyMultipleOfX && leafIsX)
						)
						{
							//Console.WriteLine($"{ph2} -> {leafNode}");
							op.operand2 = leafNode;
							return (root, true);
						}
					}
					else if (onlyConst)
					{
						if (ph2.canBeConstant && ph2.canBeX == false && leafIsX == false)
						{
							//Console.WriteLine($"{ph2} -> {leafNode}");
							op.operand2 = leafNode;
							return (root, true);
						}
					}
					else if (ph2.cannotBeExpressionOnlyMultipleOfX)
					{
						if (leafIsX)
						{
							op.operand2 = leafNode;
							return (root, true);
						}
					}
					else
					{
						if (
							(ph2.canBeX && leafIsX)
							|| (ph2.canBeConstant && !leafIsX)
						)
						{
							//Console.WriteLine($"{ph2} -> {leafNode}");
							op.operand2 = leafNode;
							return (root, true);
						}
					}
				}
            }

            bool wasOperand1Successful;

            (var resTree, wasOperand1Successful) = AddLeafToTree(op.operand1, leafNode, onlyX, onlyConst, difficulty);

            if (wasOperand1Successful)
            {
                op.operand1 = resTree;
                return (root, true);
            }

            if (op.numOperands == 2)
            {
				(var resTree2, bool wasOperand2Successful) = AddLeafToTree(op.operand2, leafNode, onlyX, onlyConst, difficulty);

                if (wasOperand2Successful)
                {
                    op.operand2 = resTree2;
                    return (root, true);
                }
			}

            return (root, false);
        }

        private static bool IsTreeOk (TreeNode tree, DifficultyMetrics difficulty, SimplificationParams simplificationParams)
        {
            if (DoesTreeContainNull(tree))
                return false;

			var diffTree = TreeUtils.GetSimplestForm(tree.Diff('x'), simplificationParams);

            if (DoesTreeContainNull(diffTree))
                return false;

            if (diffTree is Constant diffConstant)
                if (difficulty.shouldYieldNonConstDiff)
                    return false;
                else if (diffConstant is Constant { value: 0 } && difficulty.shouldYieldNonZeroDiff)
                    return false;

            if (TreeUtils.DoesTreeContainNan(tree) || TreeUtils.DoesTreeContainNan(diffTree))
                return false;

            if (difficulty.constIsOnlyInt && DoesTreeContainNonInt(tree))
                return false;

            if (DoesTreeConstainBadConstant(tree, difficulty.minConstValue, difficulty.maxConstValue))
                return false;

            int compLevel = MaxLevelOfComposition(tree);

            if (compLevel < difficulty.numMinLevelOfComposition || compLevel > difficulty.numMaxLevelOfComposition)
                return false;

            // copy, because it is a sideeffect
            var tempDict = new Dictionary<OperatorType, int>(difficulty.numAllowedFromEachOperatorType);

			if (DoesTreeContainInvalidOp(tree, ref tempDict))
                return false;

            int operatorCount = OperatorCountOfTree(tree);

            if (operatorCount > difficulty.numMaxOperators || operatorCount < difficulty.numMinOperators)
                return false;

            if (IsMultDivPowDifficultyOk(tree, difficulty) == false)
                return false;

			return true;
		}

		private static int OperatorCountOfTree(TreeNode root)
		{
			if (root is not Operator op)
				return 0;

			if (op.numOperands == 1)
			{
				return 1 + OperatorCountOfTree(op.operand1);
			}
			else
			{
				return 1 + OperatorCountOfTree(op.operand1) + OperatorCountOfTree(op.operand2);
			}
		}

		private static bool DoesTreeContainInvalidOp(TreeNode tree, ref Dictionary<OperatorType, int> numAllowedOps)
		{
			if (tree is not Operator)
				return false;

			Operator op = tree as Operator;

			if (numAllowedOps.ContainsKey(op.type) == false)
				return true;

			if (numAllowedOps[op.type] <= 0)
				return true;

			numAllowedOps[op.type] -= 1;

			if (op.numOperands == 1)
			{
				return DoesTreeContainInvalidOp(op.operand1, ref numAllowedOps);
			}
			else
			{
				return DoesTreeContainInvalidOp(op.operand1, ref numAllowedOps) || DoesTreeContainInvalidOp(op.operand2, ref numAllowedOps);
			}
		}

		private static bool DoesTreeConstainBadConstant(TreeNode root, double min, double max)
		{
			if (root is null)
				return false;

			if (root is Constant c)
				return c.value < min || c.value > max;

			if (root is not Operator op)
				return false;

			if (op.numOperands == 1)
			{
				return DoesTreeConstainBadConstant(op.operand1, min, max);
			}
			else
			{
				return DoesTreeConstainBadConstant(op.operand1, min, max) || DoesTreeConstainBadConstant(op.operand2, min, max);
			}
		}

		private static bool DoesTreeContainNull(TreeNode root)
		{
			if (root is null)
				return true;

			if (root is Constant { value: Double.NaN })
				return false;

			if (root is not Operator op)
				return false;

			if (op.numOperands == 1)
				return DoesTreeContainNull(op.operand1);
			else
				return DoesTreeContainNull(op.operand1) || DoesTreeContainNull(op.operand2);
		}

		private static bool DoesTreeContainNonInt(TreeNode root)
		{
			if (root is null)
				return false;

			if (root is Constant c)
				return c.value != Math.Floor(c.value);

			if (root is not Operator op)
				return false;

			if (op.numOperands == 1)
				return DoesTreeContainNonInt(op.operand1);
			else
				return DoesTreeContainNonInt(op.operand1) || DoesTreeContainNonInt(op.operand2);
		}

        private static bool IsMultDivPowDifficultyOk (TreeNode root, DifficultyMetrics difficulty)
        {
            if (root is not Operator op)
                return true;

            if (op is Mult)
                return (difficulty.difficultyOfMultiplication, op.operand1.IsConstant('x'), op.operand2.IsConstant('x')) switch
                {
                    (DifficultyOfMultiplication.OnlyConstant, true, false) => true,
                    (DifficultyOfMultiplication.OnlyConstant, false, true) => true,
                    (DifficultyOfMultiplication.OnlyConstant, _, _) => false,
                    (DifficultyOfMultiplication.BothCanBeDependent, _, _) => true
				};

            if (op is Div)
				return (difficulty.difficultyOfMultiplication, op.operand1.IsConstant('x'), op.operand2.IsConstant('x')) switch
				{
					(DifficultyOfMultiplication.OnlyConstant, _, true) => true,
					(DifficultyOfMultiplication.OnlyConstant, _, _) => false,
					(DifficultyOfMultiplication.BothCanBeDependent, _, _) => true
				};

            if (op is Pow)
				return (difficulty.difficultyOfPower, op.operand1.IsConstant('x'), op.operand2.IsConstant('x')) switch
				{
					(DifficultyOfPower.Polinom, _, true) => true,
					(DifficultyOfPower.Polinom, _, _) => false,
					(DifficultyOfPower.PolinomOrSimpleExponential, false, true) => true,
					(DifficultyOfPower.PolinomOrSimpleExponential, true, false) => true,
					(DifficultyOfPower.PolinomOrSimpleExponential, true, true) => true,
					(DifficultyOfPower.PolinomOrSimpleExponential, false, false) => false,
					(DifficultyOfPower.BothCanBeDependent, _, _) => true,
				};

            if (op.numOperands == 1)
                return IsMultDivPowDifficultyOk(op.operand1, difficulty);
            else
                return IsMultDivPowDifficultyOk(op.operand1, difficulty) && IsMultDivPowDifficultyOk(op.operand2, difficulty);
		}

		public static TreeNode GenerateRandomTree (DifficultyMetrics difficulty, SimplificationParams simplificationParams)
        {
            bool isTreeGenerationSuccessfull = false;

			TreeNode tree = null;

            const int loopIterLimit = 200;
            int loopCounter = 0; // for safety

            simplificationParams = simplificationParams with
            {
                varToDiff = 'x'
            };

			do
            {
                tree = null;

                //Console.WriteLine("Generating tree...");

                var opTypeList = GenerateOperatorList(difficulty);

                loopCounter = 0;
                while (opTypeList.Count > 0)
                {
                    int chosenOpIndex = random.Next() % opTypeList.Count;

                    (tree, bool wasSuccessful) = AddOperatorToTree(tree, opTypeList[chosenOpIndex], difficulty);

                    if (wasSuccessful)
                        opTypeList.RemoveAt(chosenOpIndex);

                    if (++loopCounter > loopIterLimit)
                        break;
                }

                if (loopCounter > loopIterLimit)
                    continue;
                else
                    loopCounter = 0;

                int compositionLevel = MaxLevelOfComposition(tree);

                if (compositionLevel > difficulty.numMaxLevelOfComposition || compositionLevel < difficulty.numMinLevelOfComposition)
                    continue;

                (int numMix, int numX, int numConst) = CountNumLeaves(tree, difficulty);

                var leafList = MakeLeafList(difficulty, numMix, numX, numConst);

                List<TreeNode> xList = leafList.Where(leaf => leaf is Variable { name: 'x' }).ToList();
                List<TreeNode> constList = leafList.Where(leaf => leaf is not Variable { name: 'x' }).ToList();

                for (int i = 0; i < numX; i++)
                {
                    int chosenLeafIndex = random.Next() % xList.Count;

                    (tree, bool wasSuccessful) = AddLeafToTree(tree, xList[chosenLeafIndex], true, false, difficulty);

                    if (wasSuccessful)
                        xList.RemoveAt(chosenLeafIndex);
                    else
                        i--;
                }

                for (int i = 0; i < numConst; i++)
                {
                    int chosenLeafIndex = random.Next() % constList.Count;

                    (tree, bool wasSuccessful) = AddLeafToTree(tree, constList[chosenLeafIndex], false, true, difficulty);

                    if (wasSuccessful)
                        constList.RemoveAt(chosenLeafIndex);
                    else
                        i--;
                }

                List<TreeNode> remainingList = xList.Concat(constList).ToList();

                loopCounter = 0;
                while (remainingList.Count > 0)
                {
                    int chosenLeafIndex = random.Next() % remainingList.Count;

                    (tree, bool wasSuccessful) = AddLeafToTree(tree, remainingList[chosenLeafIndex], false, false, difficulty);

                    if (wasSuccessful)
                        remainingList.RemoveAt(chosenLeafIndex);

                    if (++loopCounter > loopIterLimit)
                        break;
                }

				if (loopCounter > loopIterLimit)
					continue;
				else
					loopCounter = 0;

				if (DoesTreeContainNull(tree))
                    continue;

                try
                {
                    tree = TreeUtils.GetSimplestForm(tree, simplificationParams);

                    if (IsTreeOk(tree, difficulty, simplificationParams) == false)
                        continue;
				}
                catch (Exception e) // x/0 or something random
                {
                    continue;
                }

                isTreeGenerationSuccessfull = true;

            } while (isTreeGenerationSuccessfull == false);

			return tree;
        }
    }
}

