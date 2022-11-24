using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public static class Parser
	{
		public static List<Node> ParseToList(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return new List<Node>();

			List<Node> nodes = new List<Node>();

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
					|| Parenthesis.IsParenthesis(c) // number, var, etc followed by a '(' or ')'
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
							nodes.Add(Operator.GetClassInstanceFromType((OperatorType)type));
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
							else
							{
								// every combination of xyz, xsinx, etc. will be treated as a product!!

								while (tmp.Length > 0)
								{
									type = Operator.ParseFromString(tmp[0].ToString());

									if (type == null)
									{
										nodes.Add(new Variable(tmp[0]));
										tmp = tmp.Substring(1);
									}
									else
									{
										nodes.Add(Operator.GetClassInstanceFromType((OperatorType)type));
										tmp = tmp.Substring(1);
									}
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

		public static List<Node> ReplaceVarEWithConstE(List<Node> nodes)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];
				if (node is Variable var)
					if (var.name == 'e' || var.name == 'E')
						nodes[i] = Constant.E;
			}
			return nodes;
		}

		public static List<Node> AddHiddenMultiplications(List<Node> nodes)
		{
			for (int i = 1; i < nodes.Count; i++)
			{
				Node prevNode = nodes[i - 1];
				Node currentNode = nodes[i];

				if (
					(
						// )(, x(, 2(, but not (( or +(
						(
							prevNode is Parenthesis { isOpeningParinthesis: false }
							|| prevNode is Variable
							|| prevNode is Constant
						)
						&& currentNode is Parenthesis { isOpeningParinthesis: true }
					)
					|| (
						// )x, )2, )sin, but not )) or )+
						prevNode is Parenthesis { isOpeningParinthesis: false } // prevnode can be ')' but not '('
						&& (
							currentNode is Variable
							|| currentNode is Constant 
							|| currentNode is Operator { numOperands: 1 }
						)
					)
					|| (
						// 2x
						prevNode is Constant && currentNode is Variable
					)
					|| (
						// x2
						prevNode is Variable && currentNode is Constant
					)
					|| (
						// xy
						prevNode is Variable && currentNode is Variable
					)
					|| (
						// xsin, 2sin, )sin
						(
							prevNode is Variable 
							|| prevNode is Constant
							|| prevNode is Parenthesis { isOpeningParinthesis: false }
						) && (
							currentNode is Operator { numOperands: 1 }
						)
					)
				)
				{
					nodes.Insert(i, new Mult());
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

				if (node is Parenthesis par)
				{
					if (par.isOpeningParinthesis)
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

		public static TreeNode MakeTreeFromList(List<Node> nodes)
		{
			// get the operator with the lowest prioirty, and rightmost of those
			int minOpIndex = -1;
			int minOpPriority = 9999999;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				Node node = nodes[i];
				if (node is Operator nodeOp)
				{
					if (nodeOp.prioirty < minOpPriority)
					{
						minOpPriority = nodeOp.prioirty;
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

			if (op.numOperands == 1)
			{
				if (rightList.Count == 0)
				{
					Console.WriteLine($"Parsing error: {op} has no operand!");
					return null;
				}
				op.operand1 = MakeTreeFromList(rightList);
				return op;
			}
			else
			{
				if (leftList.Count == 0)
				{
					Console.WriteLine($"Parsing error: {op} has no left operand!");
					return null;
				}
				if (rightList.Count == 0)
				{
					Console.WriteLine($"Parsing error: {op} has no right operand!");
					return null;
				}
				op.operand1 = MakeTreeFromList(leftList);
				op.operand2 = MakeTreeFromList(rightList);
				return op;
			}
		}

		public static List<Node> HandleNegativeSigns(List<Node> nodes)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				Node? prevNode = i > 0 ? nodes[i - 1] : null;
				Node currentNode = nodes[i];

				if (currentNode is Operator op)
				{
					if (op.type == OperatorType.Sub)
					{
						if (
							prevNode == null
							|| prevNode is Operator
							|| ((prevNode is Parenthesis) && (prevNode as Parenthesis).isOpeningParinthesis == false)
						)
						{
							// it is a negative sign, so we replace '-' with a '(-1)*'
							nodes[i] = new Mult(); // add a *
							nodes.Insert(i, new Constant(-1)); // add a -1 in front of it
						}
					}
				}
			}

			return nodes;
		}
	}

}
