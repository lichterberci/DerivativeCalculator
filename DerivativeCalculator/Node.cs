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
		public virtual string ToPrettyString() { return "Unimplemented!"; }
		public virtual TreeNode Eval() => throw new NotImplementedException();
		public virtual TreeNode Diff(char varToDiff) => throw new NotImplementedException();
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

		public override string ToPrettyString()
		{
			return $"d/d{varToDifferentiate}({TreeUtils.CollapseTreeToString(expression)})";
		}

		public override TreeNode Eval()
		{
			return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return expression.Diff(varToDiff);
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

		public override string ToPrettyString()
		{
			return value == Math.E ? "e" : value.ToString("0.###");
		}

		public override string ToString()
		{
			return $"Constant({value.ToString("0.###")})";
		}

		public override TreeNode Eval()
		{
			return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Constant(0);
		}
	}

	public sealed class Variable : TreeNode
	{
		public char name;
		public Variable(char _name)
		{
			this.name = _name;
		}

		public override string ToPrettyString()
		{
			return name.ToString();
		}
		public override string ToString()
		{
			return $"Var({name})";
		}

		public override TreeNode Eval()
		{
			return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Constant(varToDiff == name ? 1 : 0);
		}
	}
}
