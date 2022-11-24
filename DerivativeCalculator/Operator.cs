using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public enum OperatorType
	{
		Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log, Ln
	}

	public class Operator : TreeNode
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

		public static int AssociativeIndex(OperatorType type)
		{
			switch (type)
			{
				case OperatorType.Add:
					return 1;
				case OperatorType.Mult:
					return 2;
				default:
					return -1;
			}
		}

		public int associativeIndex
		{
			get
			{
				return AssociativeIndex(this.type);
			}
		}
	}

	public sealed class Addition : Operator
	{
		public Addition(TreeNode? left = null, TreeNode? right = null) : base(OperatorType.Add, left, right)
		{

		}

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
			return new Addition(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}
	}

	public sealed class Subtraction : Operator
	{
		public Subtraction(TreeNode? left = null, TreeNode? right = null) : base(OperatorType.Sub, left, right)
		{

		}

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
			return new Subtraction(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}
	}

	public class AssociativeOperator : Operator
	{
		List<TreeNode> operandList;

		public AssociativeOperator(OperatorType type, List<TreeNode> operandList) : base(type, null, null)
		{
			if (Operator.AssociativeIndex(type) == -1)
				throw new ArgumentException($"Operator type '{GetStringForType(type)}' is not associative!");
			this.operandList = operandList;
		}

		public TreeNode? BuildBackBinaryTree()
		{
			if (operandList.Count == 0)
				return null;

			if (operandList.Count == 1)
				return operandList.First();

			Operator result = new Operator(type, null, null);
			Operator head = result;
			List<TreeNode> _operandList = operandList;

			while (_operandList.Count > 1)
			{
				if (head.operand1 == null)
				{
					TreeNode a = _operandList.First();
					_operandList.Remove(a);

					head.operand1 = a;
				}

				if (_operandList.Count > 1)
				{
					// there are still 2 or more operands --> we need to insert an operator
					head.operand2 = new Operator(type, null, null);
					head = head.operand2 as Operator;
				}
			}

			head.operand2 = _operandList.First();

			return result;
		}
	}
}
