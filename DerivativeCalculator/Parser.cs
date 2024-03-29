﻿
using System.Globalization;
using System.Xml.Linq;

namespace DerivativeCalculator
{
	public static class Parser
	{
		public static string PrepareStringForParsing (string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ParsingError("Input is empty or whitespace!");

			if (input.Count(c => c == '|') > 2)
				throw new ParsingError("The number of vertical bars ('|') should not exceed 2, because it would be ambiguous! Use abs() instead!");

			input = input.Trim();

			input = input.ToLower();

			input = input.Replace('.', ',');

			input = input.Replace(':', '/');

			input = input.Replace("pi", "P"); // capital P is \pi

			input += ' '; // add a space to the end, so the for-each loop will catch all nodes

			return input;
		}

		public static List<Node> ParseToList(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ParsingError("Input is empty or whitespace!");

			List<Node> nodes = new List<Node>();

			bool isInNumber = false;
			string tmp = "";
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];

				tmp = tmp.Trim();

				if (
					   char.IsWhiteSpace(c) // if there is a space / enter
					|| (
						(char.IsDigit(c) || c == ',')
						^ isInNumber) // if we are starting / ending a number
					|| (
						tmp.Length == 1 
						&& char.IsLetter(tmp[0]) 
						&& !char.IsLetter(c)) // if we are in var, but it is closed
					|| Parenthesis.IsParenthesis(c) // number, var, etc followed by a '(' or ')'
					|| c == '|'
					|| (
						tmp.Length > 0
						&& Parenthesis.IsParenthesis(tmp[0])) // if next is parenthesis
					|| (
						tmp.Length > 0
						&& tmp[0] == '|') // absolute value bar
					|| (
						Operator.ParseFromString(tmp) != null 
						&& Operator.ParseFromString(tmp + c) == null
					) // if we have collected an operator, but for sinh, we must check for the next char as well
				)
				{

					// handle end of symbol

					if (isInNumber)
					{
						double value = double.Parse(tmp, NumberStyles.Any, CultureInfo.GetCultureInfo("hu"));
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
								else if (tmp[0] == '|')
								{
									nodes.Add(new AbsoluteValueBar());
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
								// it will check for operators at the beginning of the stirng, if it finds none
								// the first character will be treated as a variable

								while (tmp.Length > 0)
								{
									bool operatorFound = false;

									for (int j = 1; j < tmp.Length; j++)
									{
										// check for an operator in the start
										type = Operator.ParseFromString(tmp.Substring(0, j));

										if (type != null)
										{
											nodes.Add(Operator.GetClassInstanceFromType((OperatorType)type));
											tmp = tmp.Substring(j);

											operatorFound = true;
											break;
										}
									}

									if (operatorFound == false)
									{
										nodes.Add(new Variable(tmp[0]));
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

		public static List<Node> HandleAbsoluteValueBars (List<Node> nodes)
		{
			int i = 0, j = nodes.Count - 1;

			while (i < j)
			{
				if (nodes[i] is not AbsoluteValueBar)
				{
					i++;
					continue;
				}

				if (nodes[j] is not AbsoluteValueBar)
				{
					j--;
					continue;
				}

				// both i and j are on a valid bar
				nodes[i] = new Parenthesis('(');
				nodes[j] = new Parenthesis(')');
				
				nodes.Insert(j, new Parenthesis(')'));
				nodes.Insert(i, new Abs(null, 5));
				nodes.Insert(i, new Parenthesis('('));
				j++; // necessary, because the insert added an item to the list
			}

			return nodes;
		}

		public static List<Node> HandleNegativeSigns(List<Node> nodes)
		{
			for (int i = 0; i < nodes.Count - 1; i++)
			{
				Node? prevNode = i > 0 ? nodes[i - 1] : null;
				Node currentNode = nodes[i];
				Node nextNode = nodes[i + 1];

				if (currentNode is Operator op)
				{
					if (op.type == OperatorType.Sub)
					{
						if (
							prevNode == null
							|| prevNode is Operator
							|| prevNode is Parenthesis { isOpeningParinthesis: true }
						)
						{
							if (nextNode is Constant c)
							{
								nodes.Remove(currentNode);
								c.value *= -1;
							}
							else
							{
								// it is a negative sign, so we replace '-' with a '(-1)*'
								nodes[i] = new Mult(); // add a *
								nodes.Insert(i, new Constant(-1)); // add a -1 in front of it
							}
						}
					}
				}
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

		public static List<Node> ReplaceVarPWithConstPi(List<Node> nodes)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];
				if (node is Variable var)
					if (var.name == 'P')
						nodes[i] = Constant.PI;
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
				else if (node is Operator op)
					op.prioirty += priorityOffset;

				if (priorityOffset < 0)
					throw new ParsingError("Parenthesis are not facing correctly!");
			}

			if (priorityOffset != 0)
				throw new ParsingError($"Van zárójel, aminek nincs párja! (offset a végén: {priorityOffset})");

			return nodes;
		}

		public static TreeNode MakeTreeFromList(List<Node> nodes)
		{
			// get the rightmost operator with the lowest priority
			int minOpIndex = -1;
			int minOpPriority = int.MaxValue;

			// Note:
			// while we usually evaluate operations from left to right, thus we get the rightmost operator from 
			// the list (given equal priority),
			// x^x^x should be evaluated from right to left
			// --> if the rightmost operator is a pow, we try to go left until we cannot find an operator
			// that is a pow with the same priority

			// this is the prioirity of the last pow
			// but if there is an operator inbetween, this priority is reset
			int lastPowPriority = int.MaxValue;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				Node node = nodes[i];
				if (node is Operator nodeOp)
				{
					if (
						nodeOp.prioirty < minOpPriority
						|| (nodeOp is Pow && nodeOp.prioirty == lastPowPriority)	
					) 
					{
						minOpPriority = nodeOp.prioirty;
						minOpIndex = i;

						lastPowPriority = nodeOp is Pow ? nodeOp.prioirty : int.MaxValue;
					}
				}
			}

			if (minOpIndex == -1) // there are no operators
				if (nodes.Count == 1)
					if (nodes[0] is Variable || nodes[0] is Constant)
						return nodes[0] as TreeNode;
					else
						throw new ParsingError($"Az ág mérete invalid! (# = {nodes.Count})");


			Operator op = nodes[minOpIndex] as Operator;

			List<Node> leftList = new List<Node>(nodes.Where((node, i) => i < minOpIndex));
			List<Node> rightList = new List<Node>(nodes.Where((node, i) => i > minOpIndex));

			if (op.numOperands == 1)
			{
				if (rightList.Count == 0)
					throw new ParsingError($"A(z) '{op.ToPrettyString()}' operátornak nincs operandusa!");

				op.operand1 = MakeTreeFromList(rightList);

				return op;
			}
			else
			{
				if (leftList.Count == 0)
					throw new ParsingError($"A(z) '{op.ToPrettyString()}' operátornak nincs bal oldali operandusa!");

				if (rightList.Count == 0)
					throw new ParsingError($"A(z) '{op.ToPrettyString()}' operátornak nincs jobb oldali operandusa!");

				op.operand1 = MakeTreeFromList(leftList);
				op.operand2 = MakeTreeFromList(rightList);

				return op;
			}
		}

		public static TreeNode ParseString (string input)
		{
			input = PrepareStringForParsing(input);

			var nodes = ParseToList(input);
			nodes = HandleAbsoluteValueBars(nodes);
			nodes = ReplaceVarPWithConstPi(nodes);
			nodes = ReplaceVarEWithConstE(nodes);
			nodes = HandleNegativeSigns(nodes);
			nodes = AddHiddenMultiplications(nodes);
			nodes = ApplyParentheses(nodes);

			return MakeTreeFromList(nodes);
		}
	}

}
