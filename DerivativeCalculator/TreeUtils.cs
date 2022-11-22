using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public static class TreeUtils
	{
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

		public static TreeNode SimplifyAssociatives (TreeNode root)
		{
			if (root is Constant || root is Variable || root is DerivativeSymbol)
				return root;

			// root is operator
			Operator op = root as Operator;

			if (op.numOperands == 1)
			{
				op.operand1 = SimplifyAssociatives(op.operand1);

				return root;
			}

			if (op.associativeIndex == -1)
				return root;

			// we are at a + or *

			if (op.operand1 is Operator op1 && op1.associativeIndex == op.associativeIndex)
			{
				// we can switch

				throw new NotImplementedException();
			}

			return root;
		}

		public static TreeNode SimplifyWithPatterns(TreeNode root)
		{
			if (root is Constant || root is Variable || root is DerivativeSymbol)
				return root;

			// root is operator
			Operator op = root as Operator;

			if (op.numOperands == 1)
			{
				op.operand1 = SimplifyWithPatterns(op.operand1);

				// TODO: add sin, cos, tan, etc. simplifications

				return root;
			}
			else
			{
				op.operand1 = SimplifyWithPatterns(op.operand1);
				op.operand2 = SimplifyWithPatterns(op.operand2);

				if ((op.operand1 is Constant) == false || (op.operand2 is Constant == false))
				{
					// simplify +0 and f(x) + f(x) = 2f(x)
					if (op.type == OperatorType.Add)
					{
						if (op.operand1 is Constant c1)
							if (c1.value == 0)
								return op.operand2;

						if (op.operand2 is Constant c2)
							if (c2.value == 0)
								return op.operand1;
						if (AreTreesEqual(op.operand1, op.operand2))
							return new Operator(OperatorType.Mult,
								new Constant(2),
								op.operand1
							);
					}
					// simplify -0 and f(x) - f(x) = 0
					if (op.type == OperatorType.Sub)
					{
						if (op.operand2 is Constant c2)
							if (c2.value == 0)
								return op.operand1;
						if (AreTreesEqual(op.operand1, op.operand2))
							return new Constant(0);
					}
					// simplify *0 and *1 and f(x)*f(x) and x*x^A
					if (op.type == OperatorType.Mult)
					{
						if (op.operand1 is Constant c1)
						{
							if (c1.value == 0)
								return new Constant(0);
							if (c1.value == 1)
								return op.operand2;
						}

						if (op.operand2 is Constant c2)
						{
							if (c2.value == 0)
								return new Constant(0);
							if (c2.value == 1)
								return op.operand1;
						}

						if (AreTreesEqual(op.operand1, op.operand2))
						{
							return new Operator(OperatorType.Pow,
								op.operand1,
								new Constant(2)
							);
						}

						if (op.operand1 is Variable v1)
						{
							if (op.operand2 is Operator op2 && op2.type == OperatorType.Pow)
							{
								if (op2.operand1 is Variable op2base && op2base.name == v1.name)
								{
									// x * x ^ A
									// --> x ^ (A + 1)
									return new Operator(OperatorType.Pow,
										v1,
										new Operator(OperatorType.Add,
											op2.operand2,
											new Constant(1)
										)
									);
								}
							}
						}

						if (op.operand2 is Variable v2)
						{
							if (op.operand1 is Operator op1 && op1.type == OperatorType.Pow)
							{
								if (op1.operand1 is Variable op1base && op1base.name == v2.name)
								{
									// x * x ^ A
									// --> x ^ (A + 1)
									return new Operator(OperatorType.Pow,
										v2,
										new Operator(OperatorType.Add,
											op1.operand2,
											new Constant(1)
										)
									);
								}
							}
						}
					}
					// simplify x^1
					if (op.type == OperatorType.Pow)
					{
						if (op.operand2 is Constant c2)
						{
							if (c2.value == 1)
								return op.operand1;
							if (c2.value == 0)
								return new Constant(1);
						}
					}
					// simplify exp(a*ln(b)) = a^b
					if (op.type == OperatorType.Pow)
					{
						if (op.operand1 is Constant c1)
							if (c1 == Constant.E)
							{
								var exponent = op.operand2;
								if (exponent is Operator operatorInExponent)
									if (operatorInExponent.type == OperatorType.Mult)
									{
										TreeNode? exponentMultLeft = operatorInExponent.operand1;
										TreeNode exponentMultRight = operatorInExponent.operand2;

										if (exponentMultRight is Operator exponentMultRightOp)
											if (exponentMultRightOp.type == OperatorType.Ln)
											{
												// we are ready, it is in the form of e^(a*ln(b))

												return new Operator(OperatorType.Pow,
													exponentMultRightOp.operand1,
													exponentMultLeft
												);
											}


										if (exponentMultLeft is Operator exponentMultLeftOp)
											if (exponentMultLeftOp.type == OperatorType.Ln)
											{
												// we are ready, it is in the form of e^(ln(a)*b)

												return new Operator(OperatorType.Pow,
													exponentMultLeftOp.operand1,
													exponentMultRight
												);
											}
									}
							}
					}
				}
			}

			return root;
		}

		public static TreeNode Calculate(TreeNode root, bool calculateIrrationals = false)
		{
			if (root is Constant || root is Variable || root is DerivativeSymbol)
				return root;

			// root is operator
			Operator op = root as Operator;

			if (op.numOperands == 1)
			{
				op.operand1 = Calculate(op.operand1);

				if ((op.operand1 is Constant) == false)
				{
					return root;
				}
			}
			else
			{
				op.operand1 = Calculate(op.operand1);
				op.operand2 = Calculate(op.operand2);

				if ((op.operand1 is Constant) == false || (op.operand2 is Constant == false))
				{
					return root;
				}
			}

			// it can be calculated

			Constant? left = op.operand1 as Constant;
			Constant right = op.operand2 as Constant;

			double rightValue = right.value;
			double leftValue = left?.value ?? 0;

			switch (op.type)
			{
				case OperatorType.Add:
					return new Constant(leftValue + rightValue);
				case OperatorType.Sub:
					return new Constant(leftValue - rightValue);
				case OperatorType.Mult:
					return new Constant(leftValue * rightValue);
				case OperatorType.Div:
					return new Constant(leftValue / rightValue);
				case OperatorType.Pow:
					return new Constant(Math.Pow(leftValue, rightValue));
				case OperatorType.Sin:
					if (calculateIrrationals)
						return new Constant(Math.Sin(leftValue));
					else
						return root;
				case OperatorType.Cos:
					if (calculateIrrationals)
						return new Constant(Math.Cos(leftValue));
					else
						return root;
				case OperatorType.Tan:
					if (calculateIrrationals)
						return new Constant(Math.Tan(rightValue));
					else
						return root;
				case OperatorType.Log:
					if (calculateIrrationals)
						return new Constant(Math.Log10(leftValue));
					else
						return root;
				case OperatorType.Ln:
					if (calculateIrrationals)
						return new Constant(Math.Log(leftValue));
					else
						return root;
				default:
					throw new ArgumentException("Operator type unhandled!");
			}
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

		public static bool IsThereConstantInAssociativeSubtree (TreeNode root, int associativeIndex)
		{
			if (root is Constant)
				return true;

			if (root is Variable || root is DerivativeSymbol)
				return false;

			Operator op = root as Operator;

			if (op.numOperands == 1)
				return false;
			
			if (op.associativeIndex != associativeIndex)
				return false;

			return IsThereConstantInAssociativeSubtree(op.operand1, associativeIndex) 
				|| IsThereConstantInAssociativeSubtree(op.operand2, associativeIndex);	
		}

		public static bool IsThereGivenVarInAssociativeSubtree (TreeNode root, int associativeIndex, char varName)
		{
			if (root is Variable var)
				return var.name == varName;

			if (root is Constant || root is DerivativeSymbol)
				return false;

			Operator op = root as Operator;

			if (op.numOperands == 1)
				return false;

			if (op.associativeIndex != associativeIndex)
				return false;

			return IsThereGivenVarInAssociativeSubtree(op.operand1, associativeIndex, varName)
				|| IsThereGivenVarInAssociativeSubtree(op.operand2, associativeIndex, varName);
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
					result += root.ToShortString();
					result += CollapseTreeToString(op.operand1, depth + 1);
				}
				else
				{
					result += CollapseTreeToString(op.operand1, depth + 1);
					result += ' ';
					result += root.ToShortString();
					result += ' ';
					result += CollapseTreeToString(op.operand2, depth + 1);
				}

				if (depth > 0)
					result += ')';

				return result;
			}
			else
			{
				return root.ToShortString();
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

			// their types are different
			return false;
		}

		public static TreeNode CopyTree(TreeNode root)
		{
			if (root is null) return null;
			if (root is Constant c) return new Constant(c.value);
			if (root is Variable v) return new Variable(v.name);
			if (root is Operator op) return new Operator(
				op.type,
				CopyTree(op.operand1),
				CopyTree(op.operand2),
				op.prioirty
			);
			if (root is DerivativeSymbol d) return new DerivativeSymbol(
				CopyTree(d.expression),
				d.varToDifferentiate
			);

			throw new ArgumentException($"Unexpected argument type: {root.GetType()}");
		}
	}

}
