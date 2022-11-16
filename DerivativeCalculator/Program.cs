using System.Linq.Expressions;
using System.Text.RegularExpressions;

public partial class Program
{
	public static void Main ()
	{
		Console.Write("> ");
		string input = Console.ReadLine().ToLower().Trim();

		char varToDifferentiate = 'x';
		if (Regex.IsMatch(input, "^d/d([a-d]|[f-z]|[A-D]|[F-Z])"))
		{
			varToDifferentiate = input[3];
			input = input.Substring(4);
		}

		List<Node> nodes;

		try
		{
			nodes = Parser.ParseToList(input);
			nodes = Parser.ReplaceVarEWithConstE(nodes);
			nodes = Parser.HandleNegativeSigns(nodes);
			nodes = Parser.AddHiddenMultiplications(nodes);
			nodes = Parser.ApplyParentheses(nodes);
		} 
		catch (Exception e)
		{
			Console.WriteLine("Parsing error!");
			return;
		}

		TreeNode tree;
		Derivator derivator;

		try
		{
			tree = Parser.MakeTreeFromList(nodes);
			derivator = new Derivator(varToDifferentiate);
			TreeNode diffTree = derivator.DifferentiateWithStepsRecorded(tree);
		} 
		catch
		{
			Console.WriteLine("An error occured while differentiating!");
			return;
		}

		Console.WriteLine("");
		for (int i = 0; i < derivator.steps.Count; i++)
		{
			Console.WriteLine($"Step {i + 1}: {derivator.steps[i]}\n");
		}

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

	public static TreeNode Simplify(TreeNode root)
	{
		if (root is Constant || root is Variable || root is DerivativeSymbol)
			return root;

		// root is operator
		Operator op = root as Operator;

		if (Operator.GetNumOperands(op.type) == 1)
		{
			op.rightOperand = Simplify(op.rightOperand);

			if ((op.rightOperand is Constant) == false)
			{
				return root;
			}
		}
		else
		{
			op.rightOperand = Simplify(op.rightOperand);
			op.leftOperand = Simplify(op.leftOperand);

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

	public static TreeNode Calculate (TreeNode root)
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
				return new Constant(Math.Sin(rightValue));
			case OperatorType.Cos:
				return new Constant(Math.Cos(rightValue));
			case OperatorType.Tan:
				return new Constant(Math.Tan(rightValue));
			case OperatorType.Log:
				return new Constant(Math.Log10(rightValue));
			case OperatorType.Ln:
				return new Constant(Math.Log(rightValue));
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

	public static bool AreTreesEqual (TreeNode a, TreeNode b)
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

	public static TreeNode CopyTree (TreeNode root)
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

	public static List<Node> ReplaceVarEWithConstE (List<Node> nodes)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			Node node = nodes[i];
			if (node is Variable)
				if ((node as Variable).name == 'e' || (node as Variable).name == 'E')
					nodes[i] = Constant.E;
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

	public static List<Node> HandleNegativeSigns(List<Node> nodes)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			Node? prevNode = i > 1 ? nodes[i - 1] : null;
			Node currentNode = nodes[i];

			if (currentNode is Operator)
			{
				if ((currentNode as Operator).type == OperatorType.Sub)
				{
					if (
						prevNode == null
						|| prevNode is Operator
						|| ((prevNode is Parenthesis) && (prevNode as Parenthesis).isOpeningParinthesis == false)
					)
					{
						// it is a negative sign, so we replace '-' with a '(-1)*'
						nodes[i] = new Operator(OperatorType.Mult); // add a *
						nodes.Insert(i, new Constant(-1)); // add a -1 in front of it
					}
				}
			}
		}

		return nodes;
	}
}

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

	private bool IsExpressionConstant (TreeNode root)
	{
		if (root is null)
			return true;

		if (root is Constant)
			return true;

		if (root is Variable)
			return (root as Variable).name != varToDifferentiate;

		return IsExpressionConstant((root as Operator).rightOperand) && IsExpressionConstant((root as Operator).leftOperand);
	}

	public TreeNode DifferentiateWithStepsRecorded (TreeNode root)
	{
		TreeNode diffTree = root;
		TreeNode prevTree = null;
		TreeNode prettyTree = null;

		maxSteps = 0;

		// initial step
		steps.Add(
			TreeUtils.CollapseTreeToString(
				TreeUtils.Simplify(TreeUtils.Calculate(TreeUtils.Simplify(new DerivativeSymbol(root, varToDifferentiate))))
			)	
		);

		while (TreeUtils.AreTreesEqual(prevTree, diffTree) == false)
		{
			numStapsTaken = 0;
			maxSteps++;

			prevTree = TreeUtils.CopyTree(diffTree);

			diffTree = DifferentiateTree(root);

			prettyTree = TreeUtils.Simplify(diffTree);
			prettyTree = TreeUtils.Calculate(prettyTree);
			prettyTree = TreeUtils.Simplify(prettyTree);
			steps.Add(TreeUtils.CollapseTreeToString(prettyTree));
		}

		// last 2 steps are the same
		if (steps.Count >= 2)
			steps.RemoveAt(steps.Count - 1);

		return diffTree;
	}

	public TreeNode DifferentiateTree (TreeNode root)
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
	public TreeNode rightOperand;
	public TreeNode? leftOperand;

	public Operator(OperatorType type, TreeNode rightOperand, TreeNode? leftOperand = null, int priority = 0)
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
			case OperatorType.Ln: // base e
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