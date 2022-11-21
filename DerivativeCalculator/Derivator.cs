using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{

	public class Derivator
	{
		public readonly char varToDifferentiate;
		public List<string> steps { get; private set; }

		private int numStapsTaken = 0;
		private int maxSteps = 0;

		public Derivator(char varToDifferentiate)
		{
			this.varToDifferentiate = varToDifferentiate;
			steps = new List<string>();
		}

		private bool IsExpressionConstant(TreeNode root)
		{
			if (root is null)
				return true;

			if (root is Constant)
				return true;

			if (root is Variable)
				return (root as Variable).name != varToDifferentiate;

			return IsExpressionConstant((root as Operator).rightOperand) && IsExpressionConstant((root as Operator).leftOperand);
		}

		public TreeNode DifferentiateWithStepsRecorded(TreeNode root)
		{
			TreeNode diffTree = root;
			TreeNode prevTree = null;
			TreeNode prettyTree = null;

			maxSteps = 0;

			// initial step
			steps.Add(
				TreeUtils.CollapseTreeToString(
					TreeUtils.SimplifyIdentities(TreeUtils.Calculate(TreeUtils.SimplifyIdentities(new DerivativeSymbol(root, varToDifferentiate))))
				)
			);

			while (TreeUtils.AreTreesEqual(prevTree, diffTree) == false)
			{
				numStapsTaken = 0;
				maxSteps++;

				prevTree = TreeUtils.CopyTree(diffTree);

				diffTree = DifferentiateTree(root);

				prettyTree = TreeUtils.SimplifyIdentities(diffTree);
				prettyTree = TreeUtils.Calculate(prettyTree);
				prettyTree = TreeUtils.SimplifyIdentities(prettyTree);
				steps.Add(TreeUtils.CollapseTreeToString(prettyTree));
			}

			// last 2 steps are the same
			if (steps.Count >= 2)
				steps.RemoveAt(steps.Count - 1);

			return diffTree;
		}

		public TreeNode DifferentiateTree(TreeNode root)
		{
			if (root is Variable)
				return (root as Variable).name == varToDifferentiate ? new Constant(1) : new Constant(0);

			if (root is Constant)
				return new Constant(0);

			if (IsExpressionConstant(root))
				return new Constant(0);

			Operator op = root as Operator;

			var type = op.type;
			TreeNode right = op.rightOperand;
			TreeNode left = op.leftOperand;

			if (numStapsTaken++ >= maxSteps)
				return new DerivativeSymbol(root, varToDifferentiate);

			switch (type)
			{
				case OperatorType.Add:
					return new Operator(OperatorType.Add, DifferentiateTree(right), DifferentiateTree(left));
				case OperatorType.Sub:
					return new Operator(OperatorType.Sub, DifferentiateTree(right), DifferentiateTree(left));
				case OperatorType.Mult:
					return new Operator(OperatorType.Add,
						new Operator(OperatorType.Mult, DifferentiateTree(right), left),
						new Operator(OperatorType.Mult, right, DifferentiateTree(left))
					);
				case OperatorType.Div:
					return new Operator(OperatorType.Div,
						new Operator(OperatorType.Pow,
							new Constant(2),
							right
						),
						new Operator(OperatorType.Sub,
							new Operator(OperatorType.Mult, DifferentiateTree(right), left),
							new Operator(OperatorType.Mult, right, DifferentiateTree(left))
						)
					);
				case OperatorType.Pow:

					// we want to break down these to 3 cases:
					// 1) f(x)^c --> c*(f(x)^(c-1))*f'(x)
					// 2) c^f(x) --> ln(c)*(x^c)*f'(x)
					// 3) f(x)^g(x) --> (A^B)' = (exp(B*ln(A)))' = exp(B*ln(A)) * (B*ln(A))'

					bool rightIsConst = IsExpressionConstant(right);
					bool leftIsConst = IsExpressionConstant(left);

					if (leftIsConst && rightIsConst)
						return root;

					// 1) x^c --> c*x^(c-1)
					if (rightIsConst)
					{
						return new Operator(OperatorType.Mult,
							DifferentiateTree(left),
							new Operator(OperatorType.Mult,
								new Operator(OperatorType.Pow,
									new Operator(OperatorType.Sub,
										new Constant(1),
										right
									),
									left
								),
								right
							)
						);
					}

					// 2) c^x --> ln(c)*c^x
					if (leftIsConst)
					{
						return new Operator(OperatorType.Mult,
							DifferentiateTree(right),
							new Operator(OperatorType.Mult,
								new Operator(OperatorType.Pow,
									right,
									left
								),
								new Operator(OperatorType.Ln,
									left
								)
							)
						);
					}

					// (A^B)' = (exp(B*ln(A)))' = exp(B*ln(A)) * (B*ln(A))'
					return new Operator(OperatorType.Mult,
						new Operator(OperatorType.Pow,
							new Operator(OperatorType.Mult,
								right,
								new Operator(OperatorType.Ln,
									left
								)
							),
							Constant.E
						),
						DifferentiateTree(
							new Operator(OperatorType.Mult,
								right,
								new Operator(OperatorType.Ln,
									left
								)
							)
						)
					);
				case OperatorType.Sin:
					return new Operator(OperatorType.Mult,
						new Operator(OperatorType.Cos, right),
						DifferentiateTree(right)
					);
				case OperatorType.Cos:
					return new Operator(OperatorType.Mult,
						new Constant(-1),
						new Operator(OperatorType.Mult,
							new Operator(OperatorType.Sin, right),
							DifferentiateTree(right)
						)
					);
				case OperatorType.Tan:
					return new Operator(OperatorType.Mult,
						new Operator(OperatorType.Div,
							new Operator(OperatorType.Cos,
								new Operator(OperatorType.Pow,
									new Constant(2),
									right
								)
							),
							new Constant(1)
						),
						DifferentiateTree(right)
					);
				case OperatorType.Log:
					return new Operator(OperatorType.Div,
						new Operator(OperatorType.Mult,
							right,
							new Constant((Math.Log(10)))
						),
						new Constant(1)
					);
				case OperatorType.Ln:
					return new Operator(OperatorType.Mult,
						new Operator(OperatorType.Div,
							right,
							new Constant(1)
						),
						DifferentiateTree(right)
					);
				default:
					throw new ArgumentException("Operator has invalid type!");
			}
		}
	}
}
