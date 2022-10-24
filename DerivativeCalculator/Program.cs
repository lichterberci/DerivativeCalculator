public partial class Program
{
	public static void Main ()
	{
		const string input = "(sin2)*5x(x^3)";
		Console.WriteLine(input);
		Parser.Parse (input);
	}
}

public static class Parser
{
	public static TreeNode? Parse (string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return null;

		List<Node> nodes = new List<Node> ();

		input.Replace('.', ',');

		input += ' '; // add a space to the end, so the for-each loop will catch all nodes

		bool isInNumber = false;
		string tmp = "";
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];

			tmp = tmp.Trim();

			if (
				   char.IsWhiteSpace(c) // if there is a space / enter
				|| ((char.IsDigit(c) || c == ',') ^ isInNumber) // if we are starting / ending a number
				|| (tmp.Length == 1 && char.IsLetter(tmp[0]) && !char.IsLetter(c)) // if we are in var, but it is closed
				|| Parenthesis.IsParenthesis(c) // nuber, var, etc followed by a '(' or ')'
				|| ((tmp.Length > 0) && Parenthesis.IsParenthesis(tmp[0])) // if next is parenthesis
				|| (Operator.ParseFromString(tmp) != null) // if we have collected an operator
			)
			{
				
				if (isInNumber)
				{
					double value = double.Parse(tmp);
					nodes.Add(new Constant(value));
					isInNumber = false;
					tmp = "";
				}
				else
				{					
					OperatorType? type = Operator.ParseFromString(tmp);

					if (type != null)
					{
						nodes.Add(new Operator((OperatorType)type));
						tmp = "";
					}
					else
					{
						if (tmp.Length == 1) // all variable have to have a length of 1
						{
							if (Parenthesis.IsParenthesis(tmp[0]))
							{
								nodes.Add(new Parenthesis(tmp[0]));
								tmp = "";
							} 
							else
							{
								nodes.Add(new Variable(tmp[0]));
								tmp = "";
							}
						}
					}
				}
			}

			tmp += c;

			if (isInNumber == false && tmp.Length > 0)
				if (char.IsDigit(tmp[0]))
					isInNumber = true;
		}

		// add invisible multiplication signs

		for (int i = 1; i < nodes.Count; i++)
		{
			Node prevNode = nodes[i - 1];
			Node currentNode = nodes[i];

			if (
				(
					(prevNode is Operator) == false 
					&& (currentNode is Parenthesis) 
					&& ((currentNode as Parenthesis).isOpeningParinthesis)
				)
				|| (
					(prevNode is Operator) == false
					&& (currentNode is Variable)
				)
				|| (
					(prevNode is Operator) == false
					&& (currentNode is Constant)
				)
			)
			{
				nodes.Insert(i, new Operator(OperatorType.Mult));
			}
		}

		nodes.ForEach(n => Console.WriteLine(n));

		return null;
	}
}

public abstract class Node
{
	public static readonly Node NULL = new NullNode();
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
	public static bool IsParenthesis (char c)
	{
		return c == '(' || c == ')';
	}
}

public sealed class NullNode : Node
{
	public override string ToString()
	{
		return "NULL_NODE";
	}
}

public abstract class TreeNode : Node
{

}

public sealed class Constant : TreeNode
{
	public double value;
	public Constant(double val)
	{
		this.value = val;
	}

	public override string ToString()
	{
		return $"Constant({value})";
	}
}

public sealed class Variable : TreeNode
{
	public char name;
	public Variable(char _name)
	{
		this.name = _name;
	}

	public override string ToString()
	{
		return $"Var({name})";
	}
}

public enum OperatorType
{
	Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log
}

public sealed class Operator : TreeNode
{
	public int prioirty;
	public OperatorType type;
	public Node operand1;
	public Node? operand2;

	public Operator(OperatorType _type, int prioirtyOffset = 0)
	{
		this.type = _type;
		this.prioirty = GetBasePriority(_type) + prioirtyOffset;
		this.operand1 = Node.NULL;
		this.operand2 = null;
	}

	public override string ToString()
	{
		return $"Operator({GetStringForType(type)}, priority = {prioirty})";
	}

	public static int GetBasePriority (OperatorType _type)
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
			default:
				return 1;
		}
	}

	public static int GetNumOperands (OperatorType _type)
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
			default:
				return 2;
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
			default:
				return "UNKNOWN_OPERATOR";
		}
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