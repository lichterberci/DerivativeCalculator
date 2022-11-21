using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public abstract class Node
	{
		public static readonly TreeNode NULL_TREENODE = new NullTreeNode();
	}

	public sealed class Parenthesis : Node
	{
		public bool isOpeningParinthesis;
		public Parenthesis(char c)
		{
			this.isOpeningParinthesis = c == '(';
		}
		public override string ToString()
		{
			return $"Parenthesis('{(isOpeningParinthesis ? "(" : ")")}')";
		}
		public static bool IsParenthesis(char c)
		{
			return c == '(' || c == ')';
		}
	}

	public sealed class NullTreeNode : TreeNode
	{
		public override string ToString()
		{
			return "NULL_NODE";
		}
	}

	public abstract class TreeNode : Node
	{
		public virtual string ToShortString() { return "Unimplemented!"; }
	}

	public sealed class DerivativeSymbol : TreeNode
	{
		public TreeNode expression;
		public readonly char varToDifferentiate;

		public DerivativeSymbol(TreeNode expression, char varToDifferentiate)
		{
			this.expression = expression;
			this.varToDifferentiate = varToDifferentiate;
		}

		public override string ToShortString()
		{
			return $"d/d{varToDifferentiate}({TreeUtils.CollapseTreeToString(expression)})";
		}
	}

	public sealed class Constant : TreeNode
	{
		public static readonly Constant E = new Constant(Math.E);
		public double value;
		public Constant(double val)
		{
			this.value = val;
		}

		public override string ToShortString()
		{
			return value == Math.E ? "e" : value.ToString("0.###");
		}

		public override string ToString()
		{
			return $"Constant({value.ToString("0.###")})";
		}
	}

	public sealed class Variable : TreeNode
	{
		public char name;
		public Variable(char _name)
		{
			this.name = _name;
		}

		public override string ToShortString()
		{
			return name.ToString();
		}
		public override string ToString()
		{
			return $"Var({name})";
		}
	}

	public enum OperatorType
	{
		Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log, Ln
	}

	public sealed class Operator : TreeNode
	{
		public int prioirty;
		public OperatorType type;
		public TreeNode operand1;
		public TreeNode? operand2;

		public Operator(OperatorType type, TreeNode operand1, TreeNode? operand2 = null, int priority = 0)
		{
			this.type = type;
			this.operand1 = operand1;
			this.operand2 = operand2;
			this.prioirty = priority;
		}

		public Operator(OperatorType _type, int priorityOffset = 0)
		{
			this.type = _type;
			this.prioirty = GetBasePriority(_type) + priorityOffset;
			this.operand1 = Node.NULL_TREENODE;
			this.operand2 = null;
		}
		public override string ToShortString()
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

		public int numOperands { 
			get { 
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

		public string GetTypeString ()
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

		public static int AssociativeIndex (OperatorType type)
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
}
