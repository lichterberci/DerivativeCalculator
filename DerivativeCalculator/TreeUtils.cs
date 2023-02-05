namespace DerivativeCalculator
{
	public static class TreeUtils
	{
		public static bool IsExpressionConstant(TreeNode root, char varToDiff)
		{
			if (root is null)
				return true;

			if (root is Constant)
				return true;

			if (root is Variable)
				return (root as Variable).name != varToDiff;

			if (root is DerivativeSymbol)
				return false;

			return IsExpressionConstant((root as Operator).operand1, varToDiff) && IsExpressionConstant((root as Operator).operand2, varToDiff);
		}

		public static void PrintTree(TreeNode root, int depth = 0)
		{
			if (root == null) return;

			string indentation = "";
			for (int i = 0; i < depth; i++)
			{
				indentation += "\t";
			}

			if (root is Operator op)
			{
				if (op.numOperands == 1)
				{
					Console.WriteLine(indentation + root.ToString());
					PrintTree(op.operand1, depth + 1);
				}
				else
				{
					PrintTree(op.operand1, depth + 1);
					Console.WriteLine(indentation + root.ToString());
					PrintTree(op.operand2, depth + 1);
				}
			}
			else
			{
				Console.WriteLine(indentation + root.ToString());
			}
		}

		public static TreeNode SimplifyWithPatterns(SimplificationParams parameters, TreeNode root)
		{
			return root.Simplify(parameters);
		}

		public static TreeNode Calculate(TreeNode root, SimplificationParams? simplificationParams)
		{
			return root.Eval(simplificationParams);
		}

		public static bool IsTreeCalculatable(TreeNode root)
		{
			if (root is Constant)
				return true;

			if (root is Variable || root is DerivativeSymbol)
				return false;

			Operator op = root as Operator;

			if (op.numOperands == 1)
				return IsTreeCalculatable(op.operand1);
			else
				return IsTreeCalculatable(op.operand1) && IsTreeCalculatable(op.operand2);
		}

		public static string CollapseTreeToString(TreeNode root, int depth = 0)
		{
			if (root == null) return "";

			if (root is Operator op)
			{
				string result = "";

				if (depth > 0)
					result += '(';

				if (op.numOperands == 1)
				{
					result += root.ToPrettyString();
					result += CollapseTreeToString(op.operand1, depth + 1);
				}
				else
				{
					result += CollapseTreeToString(op.operand1, depth + 1);
					result += ' ';
					result += root.ToPrettyString();
					result += ' ';
					result += CollapseTreeToString(op.operand2, depth + 1);
				}

				if (depth > 0)
					result += ')';

				return result;
			}
			else
			{
				return root.ToPrettyString();
			}
		}

		public static bool AreTreesEqual(TreeNode a, TreeNode b)
		{
			if (a == null && b == null) return true;

			if (a == null || b == null) return false;

			if (a.GetType() != b.GetType()) return false;

			if (a is Constant aConst && b is Constant bConst) return aConst.value == bConst.value;
			if (a is Variable aVar && b is Variable bVar) return aVar.name == bVar.name;

			if (a is Operator aOp && b is Operator bOp)
			{
				if (aOp.type != bOp.type) return false;

				return AreTreesEqual(aOp.operand1, bOp.operand1) && AreTreesEqual(aOp.operand2, bOp.operand2);
			}

			if (a is DerivativeSymbol aDer && b is DerivativeSymbol bDer)
			{
				if (aDer.varToDifferentiate != bDer.varToDifferentiate) return false;

				return AreTreesEqual(aDer.expression, bDer.expression);
			}

			if (a is Wildcard w1 && b is Wildcard w2)
			{
				return w1.name == w2.name;
			}

			// their types are different
			return false;
		}

		public static TreeNode CopyTree(TreeNode root)
		{
			if (root is null) return null;
			if (root is Constant c) return new Constant(c.value);
			if (root is Variable v) return new Variable(v.name);
			if (root is Operator op) return Operator.GetClassInstanceFromType(
				op.type,
				CopyTree(op.operand1),
				CopyTree(op.operand2),
				op.prioirty
			);
			if (root is DerivativeSymbol d) return new DerivativeSymbol(
				CopyTree(d.expression),
				d.varToDifferentiate
			);
			if (root is Wildcard w) return new Wildcard(w.name);

			throw new ArgumentException($"Unexpected argument type: {root.GetType()}");
		}

		private static (Dictionary<char, TreeNode>? zippedDict, bool wasSuccessful) ZipDicionaries(Dictionary<char, TreeNode>? left, Dictionary<char, TreeNode>? right)
		{
			var zippedDict = new Dictionary<char, TreeNode>();

			if (left is null && right is null)
			{
				return (null, true);
			}
			if (left is null)
			{
				return (right, true);
			}
			if (right is null)
			{
				return (left, true);
			}

			if (left.Keys.Count == 0 && right.Keys.Count == 0)
				return (null, true);

			foreach (var key in left.Keys.Concat(right.Keys).Distinct())
			{
				if (left.ContainsKey(key) && right.ContainsKey(key))
				{
					bool doMatch = MatchPattern(left[key], right[key], out _);

					if (doMatch == false)
						return (null, false);

					zippedDict.Add(key, left[key]);
				}
				else if (left.ContainsKey(key))
					zippedDict.Add(key, left[key]);
				else if (right.ContainsKey(key))
					zippedDict.Add(key, right[key]);
			}

			return (zippedDict, true);
		}

		public static bool MatchPattern(TreeNode tree, TreeNode? pattern, out Dictionary<char, TreeNode?> wildcards)
		{
			wildcards = null;

			if (pattern is null && tree is null)
				return true;
			if (pattern is null || tree is null)
				return false;

			if (pattern is Wildcard w)
			{
				if (tree is not null)
				{
					if (w.name is not null)
						wildcards = new Dictionary<char, TreeNode>() { { (char)w.name, tree } };
					return true;
				}
				return false;
			}

			if (pattern.GetType() != tree.GetType())
				return false;

			if (pattern is Constant c1 && tree is Constant c2)
				return c1.value == c2.value;

			if (pattern is Variable v1 && tree is Variable v2)
				return v1.name == v2.name;

			if (pattern is DerivativeSymbol d1 && tree is DerivativeSymbol d2)
				return MatchPattern(
					d1.expression,
					d2.expression,
					out _ // could cause problems later...
				);

			if (tree is not Operator || pattern is not Operator)
				throw new ArgumentException("Unhandled type");

			var treeOp = tree as Operator;
			var patternOp = pattern as Operator;

			Dictionary<char, TreeNode>? operand1Wildcards, operand2Wildcards;
			bool operand1Match = MatchPattern(treeOp.operand1, patternOp.operand1, out operand1Wildcards);
			bool operand2Match = MatchPattern(treeOp.operand2, patternOp.operand2, out operand2Wildcards);

			if (operand1Match == false || operand2Match == false)
			{
				// it is either a miss, or we mixed up the order of a commutative operator's operands

				if (treeOp.isCommutative == false)
					return false;

				// cannot hash it, because of the occasional switched ordering and things like that

				if (treeOp.isCommutative && treeOp.isAssociative)
				{
					var treeOperandList = GetAssociativeOperands(treeOp, treeOp.type, treeOp.inverseType);
					var patternOperandList = GetAssociativeOperands(patternOp, treeOp.type, treeOp.inverseType);

					Dictionary<char, TreeNode> tempWildcards;

					List<Dictionary<char, TreeNode>> operandWildcards = new();

					for (int i = 0; i < treeOperandList.Count; i++)
					{
						var (treeOperand, isTreeOperandInverse) = treeOperandList[i];

						for (int j = 0; j < patternOperandList.Count; j++)
						{
							var (patternOperand, isPatternOperandInverse) = patternOperandList[j];

							if (isTreeOperandInverse != isPatternOperandInverse)
								continue;

							if (MatchPattern(treeOperand, patternOperand, out tempWildcards))
							{
								treeOperandList.RemoveAt(i);
								patternOperandList.RemoveAt(j);

								i--;  j--;

								if (tempWildcards != null)
									operandWildcards.Add(tempWildcards);

								break;
							}
						}
					}

					// if there are any elements left in the original lists, we don't have a match

					if (treeOperandList.Count != 0 || patternOperandList.Count != 0)
						return false;

					if (operandWildcards.Count == 0)
						return true;

					var resultWildcards = operandWildcards.First();
					operandWildcards.RemoveAt(0);

					foreach (var opW in operandWildcards)
					{
						(resultWildcards, bool wasSuccessful) = ZipDicionaries(resultWildcards, opW);

						if (wasSuccessful == false)
							return false;
					}

					wildcards = resultWildcards;

					return true;
				}

				// commutative, but not associative

				operand1Match = MatchPattern(treeOp.operand2, patternOp.operand1, out operand1Wildcards);
				operand2Match = MatchPattern(treeOp.operand1, patternOp.operand2, out operand2Wildcards);

				if (operand1Match == false || operand2Match == false)
					return false; // well, we tried...
			}

			var (zippedDict, couldZipDicts) = ZipDicionaries(operand1Wildcards, operand2Wildcards);

			if (couldZipDicts == false)
				return false;

			wildcards = zippedDict;

			return true;
		}

		public static TreeNode GetSimplestForm(TreeNode tree, SimplificationParams parameters)
		{
			TreeNode _tree = CopyTree(tree);

			string prevLatexString = "";

			const int maxIterations = 10;
			const int minIterations = 1;

			for (int i = 0; i < maxIterations; i++)
			{
				_tree = _tree.Eval(parameters).Simplify(parameters).Eval(parameters);

				if (i >= minIterations && _tree.ToLatexString() == prevLatexString)
				{
					//Console.WriteLine($"Simplification took {i} iterations");
					break;
				}

				prevLatexString = _tree.ToLatexString();
			}

			return _tree;
		}

		public static List<(TreeNode, bool)> GetAssociativeOperands(TreeNode root, OperatorType type, OperatorType? inverseType, bool isInverse = false)
		{
			if (root is null)
				return new List<(TreeNode, bool)> { (null, isInverse) };

			if (root is not Operator)
				return new List<(TreeNode, bool)> { (root, isInverse) };

			Operator op = root as Operator;

			if (op.type != type && op.type != inverseType)
				return new List<(TreeNode, bool)> { (root, isInverse) };

			// on inverses, we flip, but just on the right operand
			var leftList = GetAssociativeOperands(op.operand1, type, inverseType, isInverse);
			var rightList = GetAssociativeOperands(op.operand2, type, inverseType, op.type == inverseType ? !isInverse : isInverse);

			leftList.AddRange(rightList);

			return leftList;
		}

		public static bool ContainsNullOperand(TreeNode root)
		{
			if (root is null)
				return true;

			if (root is not Operator op)
				return false;

			if (op.numOperands == 1)
				return ContainsNullOperand(op.operand1);
			else
				return ContainsNullOperand(op.operand1) || ContainsNullOperand(op.operand2);
		}

		public static List<TreeNode> SortNodesByVarNames(List<TreeNode> list, char? varToLeaveLast = null)
		{
			// implement quicksort

			if (list.Count <= 1)
				return list;

			Variable pivot = null;

			foreach (var node in list)
			{
				if (node is Variable v)
				{
					pivot = v;
					list.Remove(node);
					break;
				}
			}

			// there are no variables, so there is nothing to sort
			if (pivot is null)
				return list;

			List<TreeNode> constList = new();
			List<TreeNode> leftList = new();
			List<TreeNode> rightList = new();
			List<TreeNode> expressionList = new();

			foreach (var node in list)
			{
				if (node is Variable v)
				{
					if (v.name == varToLeaveLast)
					{
						rightList.Add(node);
						continue;
					}

					if (v.name < pivot.name)
						leftList.Add(v);
					else
						rightList.Add(v);

					continue;
				}

				if (node is Pow { operand1: Variable } pow)
				{
					Variable baseVar = pow.operand1 as Variable;

					if (baseVar.name == varToLeaveLast)
					{
						rightList.Add(node);
						continue;
					}

					if (baseVar.name < pivot.name)
						leftList.Add(node);
					else
						rightList.Add(node);

					continue;
				}

				if (node is Mult { operand1: Variable } mult1)
				{
					Variable multOp1Var = mult1.operand1 as Variable;

					if (multOp1Var.name == varToLeaveLast)
					{
						rightList.Add(node);
						continue;
					}

					if (multOp1Var.name < pivot.name)
						leftList.Add(node);
					else
						rightList.Add(node);

					continue;
				}


				if (node is Mult { operand2: Variable } mult2)
				{
					Variable multOp2Var = mult2.operand2 as Variable;

					if (multOp2Var.name == varToLeaveLast)
					{
						rightList.Add(node);
						continue;
					}

					if (multOp2Var.name < pivot.name)
						leftList.Add(node);
					else
						rightList.Add(node);

					continue;
				}


				if (node is Constant)
				{
					constList.Add(node);
					continue;
				}

				expressionList.Add(node);
			}

			leftList = SortNodesByVarNames(leftList, varToLeaveLast);
			rightList = SortNodesByVarNames(rightList, varToLeaveLast);

			return constList.Concat(leftList).Concat(new List<TreeNode> { pivot }).Concat(rightList).Concat(expressionList).ToList();
		}		
	}
}
