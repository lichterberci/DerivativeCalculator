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

			if (root is Operator)
			{
				if ((root as Operator).leftOperand != null)
				{
					PrintTree((root as Operator).leftOperand as TreeNode, depth + 1);
				}

				Console.WriteLine(indentation + root.ToString());

				PrintTree((root as Operator).rightOperand as TreeNode, depth + 1);
			}
			else
			{
				Console.WriteLine(indentation + root.ToString());
			}
		}

		public static TreeNode SimplifyCommutatives (TreeNode root)
		{
			if (root is Constant || root is Variable || root is DerivativeSymbol)
				return root;

			// root is operator
			Operator op = root as Operator;

			if (Operator.GetNumOperands(op.type) == 1)
			{
				op.rightOperand = SimplifyCommutatives(op.rightOperand);

				return root;
			}

			if (Operator.CommutativeIndex(op.type) == -1)
				return root;

			throw new NotImplementedException();
		}

		public static TreeNode SimplifyIdentities(TreeNode root)
		{
			if (root is Constant || root is Variable || root is DerivativeSymbol)
				return root;

			// root is operator
			Operator op = root as Operator;

			if (Operator.GetNumOperands(op.type) == 1)
			{
				op.rightOperand = SimplifyIdentities(op.rightOperand);

				if ((op.rightOperand is Constant) == false)
				{
					return root;
				}
			}
			else
			{
				op.rightOperand = SimplifyIdentities(op.rightOperand);
				op.leftOperand = SimplifyIdentities(op.leftOperand);

				if ((op.rightOperand is Constant) == false || (op.leftOperand is Constant == false))
				{
					// simplify +0
					if (op.type == OperatorType.Add)
					{
						if (op.leftOperand is Constant)
							if ((op.leftOperand as Constant).value == 0)
								return op.rightOperand;

						if (op.rightOperand is Constant)
							if ((op.rightOperand as Constant).value == 0)
								return op.leftOperand;
					}
					// simplify -0
					if (op.type == OperatorType.Sub)
					{
						if (op.leftOperand is Constant)
							if ((op.leftOperand as Constant).value == 0)
								return op.rightOperand;

						if (op.rightOperand is Constant)
							if ((op.rightOperand as Constant).value == 0)
								return op.leftOperand;
					}
					// simplify *0 and *1
					if (op.type == OperatorType.Mult)
					{
						if (op.leftOperand is Constant)
							if ((op.leftOperand as Constant).value == 0)
								return new Constant(0);

						if (op.rightOperand is Constant)
							if ((op.rightOperand as Constant).value == 0)
								return new Constant(0);

						if (op.leftOperand is Constant)
							if ((op.leftOperand as Constant).value == 1)
								return op.rightOperand;

						if (op.rightOperand is Constant)
							if ((op.rightOperand as Constant).value == 1)
								return op.leftOperand;
					}
					// simplify x^1
					if (op.type == OperatorType.Pow)
					{
						if (op.rightOperand is Constant)
							if ((op.rightOperand as Constant).value == 1)
								return op.leftOperand;
					}
					// simplify exp(a*ln(b)) = a^b
					if (op.type == OperatorType.Pow)
					{
						if (op.leftOperand is Constant)
							if ((op.leftOperand as Constant) == Constant.E)
							{
								var exponent = op.rightOperand;
								if (exponent is Operator)
									if ((exponent as Operator).type == OperatorType.Mult)
									{
										TreeNode exponentMultRight = (exponent as Operator).rightOperand;
										TreeNode? exponentMultLeft = (exponent as Operator).leftOperand;

										if (exponentMultRight is Operator)
											if ((exponentMultRight as Operator).type == OperatorType.Ln)
											{
												// we are ready, it is in the form of e^(a*ln(b))

												return new Operator(
													OperatorType.Pow,
													exponentMultLeft,
													exponentMultRight
												);
											}


										if (exponentMultLeft is Operator)
											if ((exponentMultLeft as Operator).type == OperatorType.Ln)
											{
												// we are ready, it is in the form of e^(a*ln(b))

												return new Operator(
													OperatorType.Pow,
													exponentMultRight,
													(exponentMultLeft as Operator).rightOperand
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

			if (Operator.GetNumOperands(op.type) == 1)
			{
				op.rightOperand = Calculate(op.rightOperand);

				if ((op.rightOperand is Constant) == false)
				{
					return root;
				}
			}
			else
			{
				op.rightOperand = Calculate(op.rightOperand);
				op.leftOperand = Calculate(op.leftOperand);

				if ((op.rightOperand is Constant) == false || (op.leftOperand is Constant == false))
				{
					return root;
				}
			}

			// it can be calculated

			Constant right = op.rightOperand as Constant;
			Constant? left = op.leftOperand as Constant;

			double rightValue = right.value;
			double? leftValue = left?.value;

			switch (op.type)
			{
				case OperatorType.Add:
					return new Constant(rightValue + (double)leftValue);
				case OperatorType.Sub:
					return new Constant((double)leftValue - rightValue);
				case OperatorType.Mult:
					return new Constant((double)leftValue * rightValue);
				case OperatorType.Div:
					return new Constant((double)leftValue / rightValue);
				case OperatorType.Pow:
					return new Constant(Math.Pow((double)leftValue, rightValue));
				case OperatorType.Sin:
					if (calculateIrrationals)
						return new Constant(Math.Sin(rightValue));
					else
						return root;
				case OperatorType.Cos:
					if (calculateIrrationals)
						return new Constant(Math.Cos(rightValue));
					else
						return root;
				case OperatorType.Tan:
					if (calculateIrrationals)
						return new Constant(Math.Tan(rightValue));
					else
						return root;
				case OperatorType.Log:
					if (calculateIrrationals)
						return new Constant(Math.Log10(rightValue));
					else
						return root;
				case OperatorType.Ln:
					if (calculateIrrationals)
						return new Constant(Math.Log(rightValue));
					else
						return root;
				default:
					throw new ArgumentException("Operator type unhandled!");
			}
		}

		public static string CollapseTreeToString(TreeNode root, int depth = 0)
		{
			if (root == null) return "";

			if (root is Operator)
			{
				string result = "";

				if (depth > 0)
					result += '(';

				if ((root as Operator).leftOperand != null)
				{
					result += CollapseTreeToString((root as Operator).leftOperand, depth + 1);
					result += ' ';
				}

				result += root.ToShortString();

				if ((root as Operator).leftOperand != null)
				{
					result += ' ';
				}
				result += CollapseTreeToString((root as Operator).rightOperand, depth + 1);


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

			if (a is Constant) return (a as Constant).value == (b as Constant).value;
			if (a is Variable) return (a as Variable).name == (b as Variable).name;

			if (a is Operator)
			{
				if ((a as Operator).type != (b as Operator).type) return false;

				return AreTreesEqual((a as Operator).leftOperand, (b as Operator).leftOperand) && AreTreesEqual((a as Operator).rightOperand, (b as Operator).rightOperand);
			}

			if (a is DerivativeSymbol)
			{
				if ((a as DerivativeSymbol).varToDifferentiate != (b as DerivativeSymbol).varToDifferentiate) return false;

				return AreTreesEqual((a as DerivativeSymbol).expression, (b as DerivativeSymbol).expression);
			}

			throw new ArgumentException($"Unexpected argument type: {a.GetType()}, {b.GetType()}");
		}

		public static TreeNode CopyTree(TreeNode root)
		{
			if (root is null) return null;
			if (root is Constant) return new Constant((root as Constant).value);
			if (root is Variable) return new Variable((root as Variable).name);
			if (root is Operator) return new Operator(
				(root as Operator).type,
				CopyTree((root as Operator).rightOperand),
				CopyTree((root as Operator).leftOperand),
				(root as Operator).prioirty
			);
			if (root is DerivativeSymbol) return new DerivativeSymbol(
				CopyTree((root as DerivativeSymbol).expression),
				(root as DerivativeSymbol).varToDifferentiate
			);

			throw new ArgumentException($"Unexpected argument type: {root.GetType()}");
		}
	}

}
