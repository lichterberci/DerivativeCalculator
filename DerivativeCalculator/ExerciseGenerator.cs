using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;

namespace DerivativeCalculator
{
    
    public class PlaceHolderLeaf : TreeNode
    {
        public readonly bool canBeConstant;
        public readonly bool canBeX;
        public readonly double minValue;
        public readonly bool isMinValueInclusive;
        public readonly double maxValue; // inclusive

        public PlaceHolderLeaf(bool canBeConstant, bool canBeX)
        {
            this.canBeConstant = canBeConstant;
            this.canBeX = canBeX;

            this.minValue = double.MinValue;
            this.isMinValueInclusive = true;
            this.maxValue = double.MaxValue;
        }

		public PlaceHolderLeaf(bool canBeConstant, bool canBeX, double minValue, bool isMinValueInclusive)
		{
			this.canBeConstant = canBeConstant;
			this.canBeX = canBeX;
			this.minValue = minValue;
            this.isMinValueInclusive = isMinValueInclusive;

			this.maxValue = double.MaxValue;
		}

		public PlaceHolderLeaf(bool canBeConstant, bool canBeX, double minValue, bool isMinValueInclusive, double maxValue)
		{
			this.canBeConstant = canBeConstant;
			this.canBeX = canBeX;
			this.minValue = minValue;
			this.isMinValueInclusive = isMinValueInclusive;

			this.maxValue = maxValue;
		}

        public override string ToString()
        {
			string min = minValue == double.MinValue ? "MIN" : minValue.ToString();
			string max = maxValue == double.MaxValue ? "MAX" : maxValue.ToString();

			string range = $"{(isMinValueInclusive ? "[" : "(")}{min}; {max}]";

			if (canBeX && canBeConstant)
                return $"Placeholder(x | c {range})";
            else if (canBeX)
                return "Placeholder(x)";
            else
                return $"Placeholder(c {range})";
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
                throw new ExerciseCouldNotBeGeneratedException("There are not enough operators to choose from!");
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

        private static (TreeNode resultTree, bool wasSuccessful) AddOperatorToTree(TreeNode root, OperatorType type, DifficultyMetrics difficulty)
        {		

			Operator newOp = Operator.GetClassInstanceFromType(type);

            switch (type)
            {
                case OperatorType.Pow:
                    (newOp.operand1, newOp.operand2) = difficulty.difficultyOfPower switch
                    {
                        DifficultyOfPower.Polinom => (new PlaceHolderLeaf(false, true), new PlaceHolderLeaf(true, false)),
                        DifficultyOfPower.PolinomOrExponential => random.Next() % 2 == 0 ?
                                                                    (new PlaceHolderLeaf(false, true), new PlaceHolderLeaf(true, false)) :
                                                                    (new PlaceHolderLeaf(true, false, 0, false), new PlaceHolderLeaf(false, true)),
                        _ => (new PlaceHolderLeaf(true, true, 0, false), new PlaceHolderLeaf(true, true))
                    };
                    break;
                case OperatorType.Mult:
                    (newOp.operand1, newOp.operand2) = difficulty.difficultyOfMultiplication switch
                    {
                        DifficultyOfMultiplication.OnlyConstant => (new PlaceHolderLeaf(true, false), new PlaceHolderLeaf(false, true)),
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

                    if (op.operand2 is PlaceHolderLeaf { canBeX: true })
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

				if (op.operand1 is PlaceHolderLeaf { canBeX: true })
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

				if (op.operand2 is PlaceHolderLeaf { canBeX: true })
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
                if (root is not PlaceHolderLeaf ph)
                    throw new ExerciseCouldNotBeGeneratedException("root is of invalid type!");

                return (ph.canBeConstant, ph.canBeX) switch
                {
                    (false, true ) => (0, 1, 0),
                    (true,  false) => (0, 0, 1),
                    (true,  true ) => (1, 0, 0),
                    (false, false) => (0, 0, 0)
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
						if (ph1.canBeX && ph1.canBeConstant == false && leafIsX)
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
						if (ph1.canBeX && ph1.canBeConstant == false && leafIsX)
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
						if (ph2.canBeX && ph2.canBeConstant == false && leafIsX)
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
            if (TreeUtils.DoesTreeContainNull(tree))
                return false;

			var diffTree = TreeUtils.GetSimplestForm(tree.Diff('x'), simplificationParams);

            if (TreeUtils.DoesTreeContainNull(diffTree))
                return false;

            if (diffTree is Constant diffConstant)
                if (difficulty.shouldYieldNonConstDiff)
                    return false;
                else if (diffConstant is Constant { value: 0 } && difficulty.shouldYieldNonZeroDiff)
                    return false;

            if (TreeUtils.DoesTreeContainNan(tree) || TreeUtils.DoesTreeContainNan(diffTree))
                return false;

            if (difficulty.constIsOnlyInt && TreeUtils.DoesTreeContainNonInt(tree))
                return false;

            if (TreeUtils.DoesTreeConstainBadConstant(tree, difficulty.minConstValue, difficulty.maxConstValue))
                return false;

            int compLevel = MaxLevelOfComposition(tree);

            if (compLevel < difficulty.numMinLevelOfComposition || compLevel > difficulty.numMaxLevelOfComposition)
                return false;

            // copy, because it is a sideeffect
            var tempDict = new Dictionary<OperatorType, int>(difficulty.numAllowedFromEachOperatorType);

			if (TreeUtils.DoesTreeContainInvalidOp(tree, ref tempDict))
                return false;

            return true;
		}

        public static TreeNode GenerateRandomTree (DifficultyMetrics difficulty, SimplificationParams simplificationParams)
        {
            bool isTreeGenerationSuccessfull = false;

			TreeNode tree = null;

            simplificationParams = simplificationParams with
            {
                varToDiff = 'x'
            };

			do
            {
                tree = null;

                //Console.WriteLine("Generating tree...");

                var opTypeList = GenerateOperatorList(difficulty);
                
                while (opTypeList.Count > 0)
                {
                    int chosenOpIndex = random.Next() % opTypeList.Count;

                    (tree, bool wasSuccessful) = AddOperatorToTree(tree, opTypeList[chosenOpIndex], difficulty);

                    if (wasSuccessful)
                        opTypeList.RemoveAt(chosenOpIndex);
                }

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

                while (remainingList.Count > 0)
                {
                    int chosenLeafIndex = random.Next() % remainingList.Count;

                    (tree, bool wasSuccessful) = AddLeafToTree(tree, remainingList[chosenLeafIndex], false, false, difficulty);

                    if (wasSuccessful)
                        remainingList.RemoveAt(chosenLeafIndex);
                }


                if (TreeUtils.DoesTreeContainNull(tree))
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

