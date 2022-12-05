using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
    public enum DifficultyOfPower
    {
        Polinom, PolinomOrExponential, BothCanBeDependent
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


        public static readonly DifficultyMetrics Easy;
        public static readonly DifficultyMetrics Medium;
        public static readonly DifficultyMetrics Hard;
        public static readonly DifficultyMetrics Hardcore;

        static DifficultyMetrics ()
        {
            Easy = new()
            {
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
			    {
				    {OperatorType.Add,  6},
				    {OperatorType.Sub,  3},
				    {OperatorType.Mult, 4},
				    {OperatorType.Div,  1},
				    {OperatorType.Pow,  2}
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
				shouldYieldNonConstDiff = false
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
				difficultyOfPower = DifficultyOfPower.PolinomOrExponential,
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
				shouldYieldNonConstDiff = true
			};

			Hard = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{OperatorType.Add,  6},
					{OperatorType.Sub,  3},
					{OperatorType.Mult, 5},
					{OperatorType.Div,  3},
					{OperatorType.Pow,  5},
					{OperatorType.Sin,  2},
				    {OperatorType.Cos,  2},
				    {OperatorType.Tan,  2},
				    {OperatorType.Ln,   2}
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
				shouldYieldNonConstDiff = true
			};

			Hardcore = new()
			{
				numAllowedFromEachOperatorType = new Dictionary<OperatorType, int>()
				{
					{OperatorType.Add,  10},
					{OperatorType.Sub,  10},
					{OperatorType.Mult, 10},
					{OperatorType.Div,  10},
					{OperatorType.Pow,  10},
					{OperatorType.Sin,  10},
					{OperatorType.Cos,  10},
					{OperatorType.Tan,  10},
					{OperatorType.Ln,   10}
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
				shouldYieldNonConstDiff = true
			};
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
                OperatorType.Sin, OperatorType.Cos, OperatorType.Tan, OperatorType.Ln, OperatorType.Log, OperatorType.Pow // pow is special !!!
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
                throw new ArgumentException("There are not enough operators to choose from!", nameof(difficulty.numAllowedFromEachOperatorType));
            }

            for (int i = 0; i < numOperatorsToChoose; i++)
            {
                int randomIndex = random.Next() % sumOfAllowedOperands;
                int runningSum = 0;

                foreach ((OperatorType key, int value) in numOperandsRemainingFromType)
                {
                    if (runningSum + value >= randomIndex)
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

        private static (TreeNode resultTree, bool wasSuccessful) AddOperatorToTree (TreeNode root, OperatorType type, DifficultyMetrics difficulty)
        {
			Operator newOp = Operator.GetClassInstanceFromType(type);

			if (type == OperatorType.Pow)
			{
				(newOp.operand1, newOp.operand2) = difficulty.difficultyOfPower switch
				{
					DifficultyOfPower.Polinom => (new Wildcard('x'), new Wildcard('c')),
					DifficultyOfPower.PolinomOrExponential => random.Next() % 2 == 0 ?
																(new Wildcard('x'), new Wildcard('c')) :
																(new Wildcard('c'), new Wildcard('x')),
					_ => (new Wildcard('m'), new Wildcard('m'))
				};
			}
            else if (type == OperatorType.Mult)
            {
                (newOp.operand1, newOp.operand2) = difficulty.difficultyOfMultiplication switch
                {
                    DifficultyOfMultiplication.OnlyConstant => (new Wildcard('c'), new Wildcard('x')),
                    _ => (new Wildcard('m'), new Wildcard('x'))
                };
            }
            else
            {
                if (newOp.numOperands == 1)
                {
                    newOp.operand1 = new Wildcard('x');
                }
                else
                {
                    if (random.Next() % 2 == 0)
                    {
						newOp.operand1 = new Wildcard('m');
						newOp.operand2 = new Wildcard('x');
					}
                    else
                    {
						newOp.operand1 = new Wildcard('x');
						newOp.operand2 = new Wildcard('m');
					}
                }
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

				if (op.operand1 is Wildcard { name: 'x' } || op.operand1 is Wildcard { name: 'm' })
                {
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

                    Wildcard w = op.operand2 as Wildcard;

                    if (w.name == 'x' || w.name == 'm')
                    {
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

                Wildcard w1 = op.operand1 as Wildcard;

                if (w1.name == 'x' || w1.name == 'm')
                {
                    op.operand1 = newOp;
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

                Wildcard w2 = op.operand2 as Wildcard;

                if (w2.name == 'x' || w2.name == 'm')
                {
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
			    newDepth = depth + 1;
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
                if (root is Wildcard w)
                {
                    if (w.name == 'x')
                    {
                        return (0, 1, 0); // x
                    } 
                    else if (w.name == 'm') // can be both
                    {
                        return (1, 0, 0);
                    } 
                    else if (w.name == 'c') // const or param
                    {
                        return (0, 0, 1);
                    }
                }

                throw new ArgumentException("root is of invalid type!", nameof(root));
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

            while (parameterNameList.Count < numParametersToChoose)
            {
                char chosenParameterName = (char)((random.Next() % 26) + 'a');

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

				if (random.NextSingle() % 1.0f < difficulty.parameterChance && numParametersToChoose > 0)
				{
					int chosenParamIndex = random.Next() % parameterNameList.Count;
					char chosenParamName = parameterNameList[chosenParamIndex];
					resList.Add(new Variable(chosenParamName));
				}
				else
				{
					if (difficulty.constIsOnlyInt)
					{
						int value = difficulty.maxConstValue != difficulty.minConstValue ? random.Next() % (difficulty.maxConstValue - difficulty.minConstValue) + difficulty.minConstValue : difficulty.minConstValue;
						resList.Add(new Constant(value));
					}
					else
					{
						double value = difficulty.maxConstValue != difficulty.minConstValue ? random.NextDouble() % (difficulty.maxConstValue - difficulty.minConstValue) + difficulty.minConstValue : difficulty.minConstValue;
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

        private static (TreeNode resulTree, bool wasSuccessful) AddLeafToTree (TreeNode root, TreeNode leafNode, bool onlyX, bool onlyConst)
        {
            bool leafIsX = leafNode is Variable { name: 'x' };

            if (root is null)
                return (root, false);

            if (root is not Operator op)
                return (root, false);

            if (op.numOperands == 1)
            {
                if (op.operand1 is Wildcard w1)
                {
                    if (w1.name == 'm' && (!onlyX && !onlyConst))
                    {
                        op.operand1 = leafNode;
                        return (root, true);
                    }

                    if (w1.name == 'x' && leafIsX)
					{
						op.operand1 = leafNode;
						return (root, true);
					}

					if (w1.name == 'c' && leafIsX == false)
					{
						op.operand1 = leafNode;
						return (root, true);
					}
				}
            }
            else
            {
                if (op.operand1 is Wildcard w1)
                {
					if (w1.name == 'm' && (!onlyX && !onlyConst))
					{
						op.operand1 = leafNode;
						return (root, true);
					}

					if (w1.name == 'x' && leafIsX)
					{
						op.operand1 = leafNode;
						return (root, true);
					}

					if (w1.name == 'c' && leafIsX == false)
					{
						op.operand1 = leafNode;
						return (root, true);
					}
				}

                if (op.operand2 is Wildcard w2)
                {
					if (w2.name == 'm' && (!onlyX && !onlyConst))
					{
						op.operand2 = leafNode;
						return (root, true);
					}

					if (w2.name == 'x' && leafIsX)
					{
						op.operand2 = leafNode;
						return (root, true);
					}

					if (w2.name == 'c' && leafIsX == false)
					{
						op.operand2 = leafNode;
						return (root, true);
					}
				}
            }

            bool wasOperand1Successful;

            (var resTree, wasOperand1Successful) = AddLeafToTree(op.operand1, leafNode, onlyX, onlyConst);

            if (wasOperand1Successful)
            {
                op.operand1 = resTree;
                return (root, true);
            }

            if (op.numOperands == 2)
            {
				(var resTree2, bool wasOperand2Successful) = AddLeafToTree(op.operand2, leafNode, onlyX, onlyConst);

                if (wasOperand2Successful)
                {
                    op.operand2 = resTree2;
                    return (root, true);
                }
			}

            return (root, false);
        }

        public static TreeNode GenerateRandomTree (DifficultyMetrics difficulty)
        {
            var opTypeList = GenerateOperatorList(difficulty);

            TreeNode tree = null;

            while (opTypeList.Count > 0)
            {
                int chosenOpIndex = random.Next() % opTypeList.Count;
    
                (tree, bool wasSuccessful) = AddOperatorToTree(tree, opTypeList[chosenOpIndex], difficulty);

                if (wasSuccessful)
                    opTypeList.RemoveAt(chosenOpIndex);
            }

            int compositionLevel = MaxLevelOfComposition(tree);

			if (compositionLevel > difficulty.numMaxLevelOfComposition || compositionLevel < difficulty.numMinLevelOfComposition)
            {
                return GenerateRandomTree(difficulty); // this is bad, generate a new one!
            }

            (int numMix, int numX, int numConst) = CountNumLeaves(tree, difficulty);

			var leafList = MakeLeafList(difficulty, numMix, numX, numConst);

            List<TreeNode> xList = leafList.Where(leaf => leaf is Variable { name: 'x' }).ToList();
			List<TreeNode> constList = leafList.Where(leaf => leaf is not Variable { name: 'x' }).ToList();

			for (int i = 0; i < numX; i++)
			{
				int chosenLeafIndex = random.Next() % xList.Count;

				(tree, bool wasSuccessful) = AddLeafToTree(tree, xList[chosenLeafIndex], true, false);

				if (wasSuccessful)
					xList.RemoveAt(chosenLeafIndex);
                else
                    i--;
			}

			for (int i = 0; i < numConst; i++)
			{
				int chosenLeafIndex = random.Next() % constList.Count;

				(tree, bool wasSuccessful) = AddLeafToTree(tree, constList[chosenLeafIndex], false, true);

				if (wasSuccessful)
					constList.RemoveAt(chosenLeafIndex);
				else
					i--;
			}

            List<TreeNode> remainingList = xList.Concat(constList).ToList();

			while (remainingList.Count > 0)
            {
                int chosenLeafIndex = random.Next() % remainingList.Count;

                (tree, bool wasSuccessful) = AddLeafToTree(tree, remainingList[chosenLeafIndex], false, false);

                if (wasSuccessful)
					remainingList.RemoveAt(chosenLeafIndex);
            }


			try
            {
                tree = TreeUtils.GetSimplestForm(tree);

			    if (TreeUtils.GetSimplestForm(tree.Diff('x')) is Constant diffConstant)
                    if (difficulty.shouldYieldNonConstDiff)
                        return GenerateRandomTree(difficulty);
                    else if (diffConstant is Constant { value: 0 } && difficulty.shouldYieldNonZeroDiff) 
			            return GenerateRandomTree(difficulty);
            } 
            catch  // x/0 or something random
            {
				return GenerateRandomTree(difficulty);
			}

			return tree;
        }
    }
}
