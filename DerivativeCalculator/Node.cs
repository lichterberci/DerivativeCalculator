﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
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

	public sealed class AbsoluteValueBar : Node
	{
		public AbsoluteValueBar() { }
		public override string ToString()
		{
			return "AbsoluteValueBar()";
		}
	}

	public sealed class NullTreeNode : TreeNode
	{
		public override string ToString() => "NULL_NODE";
	}

	public abstract class TreeNode : Node
	{
		public virtual string ToPrettyString() { return "Unimplemented!"; }
		public virtual TreeNode Eval(SimplificationParams simplificationParams = null) => throw new NotImplementedException();
		public virtual TreeNode Diff(char varToDiff) => throw new NotImplementedException();
		public bool IsConstant (char varToDiff) => TreeUtils.IsExpressionConstant(this, varToDiff);
		public virtual TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false) => this;
		public virtual string ToLatexString () => this.ToPrettyString();
		public TreeNode GetSimplestForm (SimplificationParams simplificationParams) => TreeUtils.GetSimplestForm(this, simplificationParams);
		public TreeNode Copy() => TreeUtils.CopyTree(this);
		public void PrintToConsole() => Console.WriteLine(TreeUtils.CollapseTreeToString(this));
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

		public override string ToLatexString()
		{
			bool leaveOutParenthesis = false;

			if (expression is not Operator || expression is Operator { numOperands: 1 } || expression is Pow)
				leaveOutParenthesis = true;
			
			return @$"\frac{{d}}{{d{varToDifferentiate}}}{(leaveOutParenthesis ? "" : @"\left(")}{expression.ToLatexString()}{(leaveOutParenthesis ? "" : @"\right)")}";
		}

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return expression.Diff(varToDiff);
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				expression = expression.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Constant : TreeNode
	{
		public static readonly Constant E = new Constant(Math.E);
		public static readonly Constant PI = new Constant(Math.PI);
		public double value;
		public Constant(double val)
		{
			this.value = val;
		}
		public override string ToPrettyString()
		{
			return value switch
			{
				Math.E => "e",
				Math.PI => "pi",
				_ => value.ToString("0.###")
			};
		}
		public override string ToString()
		{
			return $"Constant({value.ToString("0.###")})";
		}
		public override string ToLatexString()
		{
			return value switch
			{
				Math.E => "e",
				Math.PI => "\\pi",
				_ => value.ToString("0.###")
			};
		}
		public override TreeNode Eval(SimplificationParams simplificationParams = null)
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
		public override string ToLatexString()
		{
			return name.ToString();
		}
		public override string ToString()
		{
			return $"Var({name})";
		}

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			return new Constant(varToDiff == name ? 1 : 0);
		}
	}

	public class Wildcard : TreeNode
	{
		public char? name { get; private set; }

		public Wildcard(char? name)
		{
			this.name = name;
		}
		public override TreeNode Diff(char varToDiff)
		{
			return base.Diff(varToDiff);
		}
		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			return base.Eval(simplificationParams);
		}
		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			return base.Simplify(simplificationParams);
		}
		public override string ToPrettyString()
		{
			return "%%";
		}
		public override string ToLatexString()
		{
			return "%%";
		}
		public override string ToString()
		{
			return $"Wildcard({name})";
		}
	}
}
