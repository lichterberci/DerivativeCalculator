using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

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

		public static TreeNode Calculate(TreeNode root)
		{
			return root.Eval();
		}

		public static bool IsTreeCalculatable (TreeNode root)
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
	
		public static bool MatchPattern (TreeNode tree, TreeNode? pattern, out Dictionary<char, TreeNode?> wildcards)
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

				operand1Match = MatchPattern(treeOp.operand2, patternOp.operand1, out operand1Wildcards);
				operand2Match = MatchPattern(treeOp.operand1, patternOp.operand2, out operand2Wildcards);

				if (operand1Match == false || operand2Match == false)
					return false; // well, we tried...
			}

			var zippedDict = new Dictionary<char, TreeNode>();

			if (operand1Wildcards is null && operand2Wildcards is null)
			{
				return true;
			}
			if (operand1Wildcards is null)
			{
				wildcards = operand2Wildcards;
				return true;
			}
			if (operand2Wildcards is null)
			{
				wildcards = operand1Wildcards;
				return true;
			}

			foreach (var key in operand1Wildcards.Keys.Concat(operand2Wildcards.Keys).Distinct())
			{
				if (operand1Wildcards.ContainsKey(key) && operand2Wildcards.ContainsKey(key))
				{
					bool doMatch = MatchPattern(operand1Wildcards[key], operand2Wildcards[key], out _);

					if (doMatch == false)
						return false;

					zippedDict.Add(key, operand1Wildcards[key]);
				}
				else if (operand1Wildcards.ContainsKey(key))
					zippedDict.Add(key, operand1Wildcards[key]);
				else if (operand2Wildcards.ContainsKey(key))
					zippedDict.Add(key, operand2Wildcards[key]);
			}

			wildcards = zippedDict;
			return true;
		}
	
		public static TreeNode GetSimplestForm (TreeNode tree, SimplificationParams parameters)
		{
			TreeNode _tree = CopyTree(tree);

			string prevLatexString = "";

			const int maxIterations = 10;
			const int minIterations = 1;

			for (int i = 0; i < maxIterations; i++)
			{
				_tree = _tree.Eval().Simplify(parameters).Eval();

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

		public static bool ContainsNullOperand (TreeNode root)
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

		public static bool DoesTreeContainNan (TreeNode root)
		{
			if (root is null)
				return false;

			if (root is Constant c)
				return double.IsNaN(c.value) || double.IsInfinity(c.value);

			if (root is not Operator op)
				return false;

			if (op.numOperands == 1)
				return DoesTreeContainNan(op.operand1);
			else
				return DoesTreeContainNan(op.operand1) || DoesTreeContainNan(op.operand2);
		}

		public static bool DoesTreeContainNonInt (TreeNode root)
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

		public static bool DoesTreeContainNull (TreeNode root)
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

		public static bool DoesTreeConstainBadConstant(TreeNode root, double min, double max)
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

		public static List<TreeNode> SortNodesByVarNames (List<TreeNode> list, char? varToLeaveLast = null)
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

			foreach(var node in list)
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

		public static List<(TreeNode, TreeNode)> SortBasePowPairsByVarNames(List<(TreeNode, TreeNode)> list, char? varToLeaveLast = null)
		{
			if (list.Count <= 1)
				return list;

			Func<TreeNode, char?> GetVarName = key =>
			{
				if (key is Variable v)
					return v.name;

				if (key is Mult { operand1: Variable } mult1)
					return (mult1.operand1 as Variable).name;

				if (key is Mult { operand2: Variable } mult2)
					return (mult2.operand2 as Variable).name;

				if (key is Pow { operand1: Variable } pow1)
					return (pow1.operand1 as Variable).name;

				return null;
			};

			(TreeNode, TreeNode) pivot = (null, null);
			char pivotName = 'a';

			foreach ((var key, var pow) in list)
			{
				char? varName = GetVarName(key);

				if (varName is not null)
				{
					pivot = (key, pow);
					pivotName = (char)varName;
					list.Remove((key, pow));
					break;
				}
			}

			if (pivot == (null, null))
				return list;

			List<(TreeNode, TreeNode)> constList = new();
			List<(TreeNode, TreeNode)> leftList = new();
			List<(TreeNode, TreeNode)> rightList = new();
			List<(TreeNode, TreeNode)> expressionList = new();

			foreach ((var key, var pow) in list)
			{
				char? varName = GetVarName(key);

				if (varName is not null)
				{
					if (varName == varToLeaveLast)
					{
						rightList.Add((key, pow));
						continue;
					}

					if (varName > pivotName)
						rightList.Add((key, pow));
					else
						leftList.Add((key, pow));

					continue;
				}

				if (key is Constant)
				{
					constList.Add((key, pow));

					continue;
				}

				expressionList.Add((key, pow));
			}

			leftList = SortBasePowPairsByVarNames(leftList, varToLeaveLast);
			rightList = SortBasePowPairsByVarNames(rightList, varToLeaveLast);

			if (pivotName == varToLeaveLast)
				return constList.Concat(leftList).Concat(rightList).Concat(new List<(TreeNode, TreeNode)> { pivot }).Concat(expressionList).ToList();

			return constList.Concat(leftList).Concat(new List<(TreeNode, TreeNode)> { pivot }).Concat(rightList).Concat(expressionList).ToList();
		}
	}
}
