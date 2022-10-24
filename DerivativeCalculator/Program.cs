public partial class Program
{
	public static void Main ()
	{
		Console.Write("> ");
		string input = Console.ReadLine().ToLower().Trim();
		Console.WriteLine(input);
		var nodes = Parser.ApplyParentheses(Parser.AddHiddenMultiplications(Parser.ParseToList(input)));
		var tree = Parser.MakeTreeFromList(nodes);
		TreeUtils.PrintTree(tree);
		Console.WriteLine("-----------------------------------------");
		TreeNode diffTree = new Derivator('x').DifferentiateTree(tree);
		TreeUtils.PrintTree(diffTree);
		Console.WriteLine(TreeUtils.CollapseTreeToString(diffTree));
	}
}

// TODO: add -1, -(x+1) support to the parser

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

	public static string CollapseTreeToString(TreeNode root)
	{
		if (root == null) return "";

		if (root is Operator)
		{
			string result = "";

			result += '(';

			if ((root as Operator).leftOperand != null)
			{
				result += CollapseTreeToString((root as Operator).leftOperand);
			}

			result += root.ToShortString();

			result += CollapseTreeToString((root as Operator).rightOperand);

			result += ')';

			return result;
		}
		else
		{
			return root.ToShortString();
		}
	}
}

public static class Parser
{
	public static List<Node> ParseToList (string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return new List<Node>();

		List<Node> nodes = new List<Node> ();

		input = input.Replace('.', ',');

		input += ' '; // add a space to the end, so the for-each loop will catch all nodes

		//Console.WriteLine(input);

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

		return nodes;
	}

	public static List<Node> AddHiddenMultiplications (List<Node> nodes)
	{
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
					&& ((prevNode is Parenthesis) && (prevNode as Parenthesis).isOpeningParinthesis) == false // prenode can be ')' but not '('
					&& (currentNode is Variable)
				)
				|| (
					(prevNode is Operator) == false
					&& ((prevNode is Parenthesis) && (prevNode as Parenthesis).isOpeningParinthesis) == false // prenode can be ')' but not '('
					&& (currentNode is Constant)
				)
			)
			{
				nodes.Insert(i, new Operator(OperatorType.Mult));
			}
		}

		//nodes.ForEach(node => Console.WriteLine(node));

		return nodes;
	}

	public static List<Node> ApplyParentheses(List<Node> nodes)
	{
		if (nodes.Count == 0)
			return nodes;

		int priorityOffset = 0;

		for (int i = 0; i < nodes.Count; i++)
		{
			Node node = nodes[i];

			if (node is Parenthesis)
			{
				if ((node as Parenthesis).isOpeningParinthesis)
					priorityOffset += 10;
				else
					priorityOffset -= 10;

				nodes.RemoveAt(i);
				i--; // cuz we deleted it from the list, the next element will be at the same exact index
			}
			else if (node is Operator)
				(node as Operator).prioirty += priorityOffset;
		}

		if (priorityOffset != 0)
			throw new ArgumentException($"Parentheses are not alligned correctly! (offset at the end: {priorityOffset})");

		return nodes;
	}

	public static TreeNode MakeTreeFromList (List<Node> nodes)
	{
		// get the operator with the lowest prioirty, and rightmost of those
		int minOpIndex = -1;
		int minOpPriority = 9999999;

		for (int i = nodes.Count - 1; i >= 0; i--)
		{
			Node node = nodes[i];
			if (node is Operator)
			{
				if ((node as Operator).prioirty < minOpPriority)
				{
					minOpPriority = (node as Operator).prioirty;
					minOpIndex = i;
				}
			}
		}

		if (minOpIndex == -1) // there are no operators
			if (nodes.Count == 1)
				if (nodes[0] is Variable || nodes[0] is Constant)
					return nodes[0] as TreeNode;
			else
				throw new ArgumentException($"Branch size invalid! (count: {nodes.Count})");


		Operator op = nodes[minOpIndex] as Operator;

		List<Node> leftList = new List<Node>(nodes.Where((node, i) => i < minOpIndex));
		List<Node> rightList = new List<Node>(nodes.Where((node, i) => i > minOpIndex));

		if (Operator.GetNumOperands(op.type) == 1)
		{
			op.rightOperand = MakeTreeFromList(rightList);
			return op;
		}
		else
		{
			op.rightOperand = MakeTreeFromList(rightList);
			op.leftOperand = MakeTreeFromList(leftList);
			return op;
		}
	}
}

public class Derivator
{
	public readonly char varToDifferentiate;
	public Derivator(char varToDifferentiate)
	{
		this.varToDifferentiate = varToDifferentiate;
	}

	public TreeNode DifferentiateTree (TreeNode root)
	{
		if (root is Variable)
			return (root as Variable).name == varToDifferentiate ? new Constant(1) : new Constant(0);

		if (root is Constant)
			return new Constant(0);

		Operator op = root as Operator;

		var type = op.type;
		TreeNode right = op.rightOperand;
		TreeNode left = op.leftOperand;

		switch (type)
		{
			case OperatorType.Add:
				return new Operator(OperatorType.Add, right, left);
			case OperatorType.Sub:
				return new Operator(OperatorType.Sub, right, left);
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
			case OperatorType.Pow: // (A^B)' = (exp(B*ln(A)))' = exp(B*ln(A)) * (B*ln(A))'
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
						new Operator(OperatorType.Cos, right),
						DifferentiateTree(right)
					)
				);
			case OperatorType.Tan:
				return new Operator(OperatorType.Div,
					new Operator(OperatorType.Pow,
						new Constant(2),
						right
					),
					new Constant(1)
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
	public static bool IsParenthesis (char c)
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
	public virtual string ToShortString () { return "Unimplemented!"; }
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
		return value.ToString();
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
	public TreeNode rightOperand;
	public TreeNode? leftOperand;

	public Operator(OperatorType type, TreeNode rightOperand, TreeNode? leftOperand = null)
	{
		this.type = type;
		this.rightOperand = rightOperand;
		this.leftOperand = leftOperand;
		this.prioirty = -1;
	}

	public Operator(OperatorType _type, int prioirtyOffset = 0)
	{
		this.type = _type;
		this.prioirty = GetBasePriority(_type) + prioirtyOffset;
		this.rightOperand = Node.NULL_TREENODE;
		this.leftOperand = null;
	}
	public override string ToShortString()
	{
		return GetStringForType(type);
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
			case OperatorType.Ln: // base 10
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
			case OperatorType.Ln: // base 10
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
			case OperatorType.Ln: // base 10
				return "ln";
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