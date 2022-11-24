using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public enum OperatorType
	{
		Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log, Ln
	}

	public abstract class Operator : TreeNode
	{
		public int prioirty;
		public OperatorType type;
		public TreeNode operand1;
		public TreeNode? operand2;

		public Operator(OperatorType type, TreeNode operand1, TreeNode? operand2 = null, int? priority = null)
		{
			this.type = type;
			this.operand1 = operand1;
			this.operand2 = operand2;
			this.prioirty = priority ?? Operator.GetBasePriority(type);
		}
		public Operator(OperatorType _type, int priorityOffset = 0)
		{
			this.type = _type;
			this.prioirty = GetBasePriority(_type) + priorityOffset;
			this.operand1 = Node.NULL_TREENODE;
			this.operand2 = null;
		}
		public override string ToPrettyString()
		{
			return GetStringForType(type);
		}
		public override string ToString()
		{
			return $"Operator({GetStringForType(type)}, priority = {prioirty})";
		}
		public static Operator GetClassInstanceFromType (OperatorType _type, TreeNode? operand1 = null, TreeNode? operand2 = null, int? priority = null)
		{
			switch (_type)
			{
				case OperatorType.Add:
					return new Add(operand1, operand2, priority);
				case OperatorType.Sub:
					return new Sub(operand1, operand2, priority);
				case OperatorType.Mult:
					return new Mult(operand1, operand2, priority);
				case OperatorType.Div:
					return new Div(operand1, operand2, priority);
				case OperatorType.Pow:
					return new Pow(operand1, operand2, priority);
				case OperatorType.Sin:
					return new Sin(operand1, priority);
				case OperatorType.Cos:
					return new Cos(operand1, priority);
				case OperatorType.Tan:
					return new Tan(operand1, priority);
				case OperatorType.Log: // base 10
					return new Log(operand1, priority);
				case OperatorType.Ln: // base e
					return new Ln(operand1, priority);
				default:
					throw new ArgumentException($"Type {_type} is not handled!");
			}
		}
		public static int GetBasePriority(OperatorType _type)
		{
			switch (_type)
			{
				case OperatorType.Add:
					return 1;
				case OperatorType.Sub:
					return 1;
				case OperatorType.Mult:
					return 2;
				case OperatorType.Div:
					return 2;
				case OperatorType.Pow:
					return 3;
				case OperatorType.Sin:
					return 3;
				case OperatorType.Cos:
					return 3;
				case OperatorType.Tan:
					return 3;
				case OperatorType.Log: // base 10
					return 3;
				case OperatorType.Ln: // base e
					return 3;
				default:
					return 1;
			}
		}
		public int basePriority
		{
			get
			{
				return GetBasePriority(this.type);
			}
		}
		public static int GetNumOperands(OperatorType _type)
		{
			switch (_type)
			{
				case OperatorType.Add:
					return 2;
				case OperatorType.Sub:
					return 2;
				case OperatorType.Mult:
					return 2;
				case OperatorType.Div:
					return 2;
				case OperatorType.Pow:
					return 2;
				case OperatorType.Sin:
					return 1;
				case OperatorType.Cos:
					return 1;
				case OperatorType.Tan:
					return 1;
				case OperatorType.Log: // base 10
					return 1;
				case OperatorType.Ln: // base 10
					return 1;
				default:
					return 2;
			}
		}
		public int numOperands
		{
			get
			{
				return GetNumOperands(this.type);
			}
		}
		public static string GetStringForType(OperatorType _type)
		{
			switch (_type)
			{
				case OperatorType.Add:
					return "+";
				case OperatorType.Sub:
					return "-";
				case OperatorType.Mult:
					return "*";
				case OperatorType.Div:
					return "/";
				case OperatorType.Pow:
					return "^";
				case OperatorType.Sin:
					return "sin";
				case OperatorType.Cos:
					return "cos";
				case OperatorType.Tan:
					return "tan";
				case OperatorType.Log: // base 10
					return "log";
				case OperatorType.Ln: // base e
					return "ln";
				default:
					return "UNKNOWN_OPERATOR";
			}
		}
		public string GetTypeString()
		{
			return GetStringForType(this.type);
		}
		public static OperatorType? ParseFromString(string str)
		{
			foreach (var op in Enum.GetValues(typeof(OperatorType)).Cast<OperatorType>())
			{
				if (GetStringForType(op) == str)
					return op;
			}

			return null;
		}
	}

	public sealed class Add : Operator
	{
		public Add(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Add, left, right, priority) { }

		public override TreeNode Eval ()
		{
			var left = operand1.Eval();
			var right = operand2.Eval();

			if (left is Constant l && right is Constant r)
				return new Constant(l.value + r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Add(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}

		public override TreeNode Simplify()
		{
			return base.Simplify();
		}
	}

	public sealed class Sub : Operator
	{
		public Sub(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Sub, left, right, priority) { }

		public override TreeNode Eval()
		{
			var left = operand1.Eval();
			var right = operand2.Eval();

			if (left is Constant l && right is Constant r)
				return new Constant(l.value - r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Sub(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}
	}

	public sealed class Mult : Operator
	{
		public Mult(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Mult, left, right, priority) { }

		public override TreeNode Eval()
		{
			var left = operand1.Eval();
			var right = operand2.Eval();

			if (left is Constant l && right is Constant r)
				return new Constant(l.value * r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (operand1.IsConstant(varToDiff))
				return new Mult(operand1, operand2.Diff(varToDiff));

			if (operand2.IsConstant(varToDiff))
				return new Mult(operand2, operand1.Diff(varToDiff));

			return new Add(
				new Mult(operand1.Diff(varToDiff), operand2),
				new Mult(operand1, operand2.Diff(varToDiff))
			);
		}
	}

	public sealed class Div : Operator
	{
		public Div(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Div, left, right, priority) { }
		
		public override TreeNode Eval()
		{
			var left = operand1.Eval();
			var right = operand2.Eval();

			if (left is Constant l && right is Constant r)
				return new Constant(l.value / r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (operand2.IsConstant(varToDiff))
				return new Div(operand1.Diff(varToDiff), operand2);

			return new Div(
				new Sub(
					new Mult(operand1.Diff(varToDiff), operand2),
					new Mult(operand1, operand2.Diff(varToDiff))
				),
				new Pow (operand2, new Constant(2))
			);
		}
	}

	public sealed class Pow : Operator
	{
		public Pow(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Pow, left, right, priority) { }

		public override TreeNode Eval()
		{
			var left = operand1.Eval();
			var right = operand2.Eval();

			if (left is Constant l && right is Constant r)
				return new Constant(Math.Pow(l.value, r.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			// c^f(x) --> ln(c)*c^f(x)*f'(x)
			if (operand1.IsConstant(varToDiff))
			{
				return new Mult(
					new Mult(
						new Ln(operand1),
						new Pow(operand1, operand2)
					),
					operand2.Diff(varToDiff)
				);
			}

			// x^c --> c*x^c-1
			if (operand2.IsConstant(varToDiff))
			{
				return new Mult(
					operand2,
					new Pow(
						operand1,
						new Sub(
							operand2, 
							new Constant(1)
						)
					)
				);
			}

			// (A^B)' = (exp(B*ln(A)))' = exp(B*ln(A)) * (B*ln(A))'
			return new Mult(
				new Pow(
					Constant.E,
					new Mult(
						operand2,
						new Ln(operand1)
					)
				),
				new Mult(
					operand2,
					 new Ln(operand1)
			   ).Diff(varToDiff)
			);
		}
	}

	public sealed class Sin : Operator
	{
		public Sin(TreeNode? operand = null, int? priority = null) : base(OperatorType.Sin, operand, null, priority) { }

		public override TreeNode Eval()
		{
			var operand = operand1.Eval();

			if (operand is Constant c)
				return new Constant(Math.Sin(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Mult(
				new Cos(operand1),
				operand1.Diff(varToDiff)
			);
		}
	}

	public sealed class Cos : Operator
	{
		public Cos(TreeNode? operand = null, int? priority = null) : base(OperatorType.Cos, operand, null, priority) { }

		public override TreeNode Eval()
		{
			var operand = operand1.Eval();

			if (operand is Constant c)
				return new Constant(Math.Cos(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Mult(
				new Mult(
					new Constant(-1),
					new Sin(operand1)
				),
				operand1.Diff(varToDiff)
			);
		}
	}

	public sealed class Tan : Operator
	{
		public Tan(TreeNode? operand = null, int? priority = null) : base(OperatorType.Tan, operand, null, priority) { }

		public override TreeNode Eval()
		{
			var operand = operand1.Eval();

			if (operand is Constant c)
				return new Constant(Math.Tan(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Div(
				operand1.Diff(varToDiff),
				new Pow(
					new Cos(operand1),
					new Constant(2)
				)
			);
		}
	}

	public sealed class Ln : Operator
	{
		public Ln(TreeNode? operand = null, int? priority = null) : base(OperatorType.Ln, operand, null, priority) { }

		public override TreeNode Eval()
		{
			var operand = operand1.Eval();

			if (operand is Constant c)
				return new Constant(Math.Log(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Div(
				operand1.Diff(varToDiff),
				operand1
			);
		}
	}

	public sealed class Log : Operator
	{
		public Log(TreeNode? operand = null, int? priority = null) : base(OperatorType.Log, operand, null, priority) { }

		public override TreeNode Eval()
		{
			var operand = operand1.Eval();

			if (operand is Constant c)
				return new Constant(Math.Log10(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Div(
				operand1.Diff(varToDiff),
				new Mult(
					new Constant(Math.Log(10)),
					operand1
				)
			);
		}
	}
}
