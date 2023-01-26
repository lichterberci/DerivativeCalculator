using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

using DerivativeCalculator;

using Microsoft.VisualBasic;

namespace DerivativeCalculator
{
	public enum OperatorType
	{
		Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log, Ln, Cot, Arcsin, Arccos, Arctan, Arccot, Sinh, Cosh, Tanh, Coth, Arsinh, Arcosh, Artanh, Arcoth, Abs
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
		public static Operator GetClassInstanceFromType(OperatorType _type, TreeNode? operand1 = null, TreeNode? operand2 = null, int? priority = null)
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
				case OperatorType.Cot:
					return new Cot(operand1, priority);
				case OperatorType.Arcsin:
					return new Arcsin(operand1, priority);
				case OperatorType.Arccos:
					return new Arccos(operand1, priority);
				case OperatorType.Arctan:
					return new Arctan(operand1, priority);
				case OperatorType.Arccot:
					return new Arccot(operand1, priority);
				case OperatorType.Sinh:
					return new Sinh(operand1, priority);
				case OperatorType.Cosh:
					return new Cosh(operand1, priority);
				case OperatorType.Tanh:
					return new Tanh(operand1, priority);
				case OperatorType.Coth:
					return new Coth(operand1, priority);
				case OperatorType.Arsinh:
					return new Arsinh(operand1, priority);
				case OperatorType.Arcosh:
					return new Arcosh(operand1, priority);
				case OperatorType.Artanh:
					return new Artanh(operand1, priority);
				case OperatorType.Arcoth:
					return new Arcoth(operand1, priority);
				case OperatorType.Abs:
					return new Abs(operand1, priority);
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
				default:
					return 3;
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
				case OperatorType.Sub:
				case OperatorType.Mult:
				case OperatorType.Div:
				case OperatorType.Pow:
					return 2;
				default:
					return 1;
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
				case OperatorType.Cot:
					return "cot";
				case OperatorType.Arcsin:
					return "arcsin";
				case OperatorType.Arccos:
					return "arccos";
				case OperatorType.Arctan:
					return "arctan";
				case OperatorType.Arccot:
					return "arccot";
				case OperatorType.Sinh:
					return "sinh";
				case OperatorType.Cosh:
					return "cosh";
				case OperatorType.Tanh:
					return "tanh";
				case OperatorType.Coth:
					return "coth";
				case OperatorType.Arsinh:
					return "arsinh";
				case OperatorType.Arcosh:
					return "arcosh";
				case OperatorType.Artanh:
					return "artanh";
				case OperatorType.Arcoth:
					return "arcoth";
				case OperatorType.Abs:
					return "abs";
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

			// aliases

			switch(str)
			{
				case "tg":
					return OperatorType.Tan;
				case "ctg":
					return OperatorType.Cot;
				case "arctg":
					return OperatorType.Arctan;
				case "arcctg":
					return OperatorType.Arccot;
				case "sh":
					return OperatorType.Sinh;
				case "ch":
					return OperatorType.Cosh;
				case "th":
					return OperatorType.Tanh;
				case "cth":
					return OperatorType.Coth;
				case "arsh":
					return OperatorType.Arsinh;
				case "arch":
					return OperatorType.Arcosh;
				case "arth":
					return OperatorType.Artanh;
				case "arcth":
					return OperatorType.Arcoth;
			}

			return null;
		}
		public static bool IsOperatorTypeCommutative(OperatorType type)
		{
			return type switch
			{
				OperatorType.Add or OperatorType.Mult => true,
				_ => false
			};
		}
		public bool isCommutative
		{
			get
			{
				return IsOperatorTypeCommutative(this.type);
			}
		}
		public bool isInverseCommutative
		{
			get
			{
				if (inverseType is null)
					return false;

				return IsOperatorTypeCommutative((OperatorType)inverseType);
			}
		}
		public static bool IsOperatorTypeAssociative (OperatorType type)
		{
			return type switch
			{
				OperatorType.Add or OperatorType.Mult => true,
				_ => false
			};
		}
		public bool isAssociative
		{
			get
			{
				return IsOperatorTypeAssociative(this.type);
			}
		}
		public bool isInverseAssociative
		{
			get
			{
				if (inverseType is null)
					return false;

				return IsOperatorTypeAssociative((OperatorType)inverseType);
			}
		}
		public static OperatorType? GetInverse(OperatorType type)
		{
			switch (type)
			{
				case OperatorType.Add:
					return OperatorType.Sub;
				case OperatorType.Sub:
					return OperatorType.Add;
				case OperatorType.Mult:
					return OperatorType.Div;
				case OperatorType.Div:
					return OperatorType.Mult;
				case OperatorType.Sin:
					return OperatorType.Arcsin;
				case OperatorType.Cos:
					return OperatorType.Arccos;
				case OperatorType.Tan:
					return OperatorType.Arctan;
				case OperatorType.Cot:
					return OperatorType.Arccot;
				case OperatorType.Arcsin:
					return OperatorType.Sin;
				case OperatorType.Arccos:
					return OperatorType.Cos;
				case OperatorType.Arctan:
					return OperatorType.Tan;
				case OperatorType.Arccot:
					return OperatorType.Cot;
				case OperatorType.Sinh:
					return OperatorType.Arsinh;
				case OperatorType.Cosh:
					return OperatorType.Arcosh;
				case OperatorType.Tanh:
					return OperatorType.Artanh;
				case OperatorType.Coth:
					return OperatorType.Arcoth;
				case OperatorType.Arsinh:
					return OperatorType.Sinh;
				case OperatorType.Arcosh:
					return OperatorType.Cosh;
				case OperatorType.Artanh:
					return OperatorType.Tanh;
				case OperatorType.Arcoth:
					return OperatorType.Coth;
				default:
					return null;
			}
		}
		public OperatorType? inverseType
		{
			get
			{
				return GetInverse(this.type);
			}
		}
		public override string ToLatexString()
		{
			if (numOperands == 1)
				return $@"{this.GetTypeString()}\left({{{operand1.ToLatexString()}}}\right)";
			else
				return $@"\left({{{operand1.ToLatexString()}}}\right){this.GetTypeString()}\left({{{operand2.ToLatexString()}}}\right)";
		}
		protected bool ShouldEval (SimplificationParams? simplificationParams)
		{
			if (simplificationParams is null)
				return true;

			if (simplificationParams.opsNotToEval is null)
				return true;

			return simplificationParams.opsNotToEval.Any(type => type == this.type) == false;
		}
	}

	public sealed class Add : Operator
	{
		public Add(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Add, left, right, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);
			operand2 = operand2.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant l && operand2 is Constant r)
				return new Constant(l.value + r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{

				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\left(f(x) + g(x)\right) = \frac{{d}}{{dx}}f(x) + \frac{{d}}{{dx}}g(x)",
				operand1.ToLatexString(),
				operand2.ToLatexString()
			));

			return new Add(
				operand1.Diff(varToDiff), 
				operand2.Diff(varToDiff)
			);
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
				operand2 = operand2.Simplify(simplificationParams);
			}

			// associative things

			var operands = TreeUtils.GetAssociativeOperands(this, type, inverseType);

			// !!! THIS LINE REALLY SLOWS DOWN THE WHOLE THING !!!
			//operands = operands.Select(item => (TreeUtils.GetSimplestForm(item.Item1), item.Item2)).ToList();

			if (operands.Count < 2)
				return this;

			var coefficientDict = ApplyRules(operands, simplificationParams);			
			
			if (coefficientDict.Keys.Count == 0)
				return new Constant(0);

			// if we have managed to simplify to a single expression
			if (coefficientDict.Keys.Count == 1)
				return new Mult(
					coefficientDict.Keys.Last(),
					coefficientDict.Values.Last()
				);

			// we build 2 trees: one for the additions and one for the subtractions (both with the + operator in them)
			// then we just subtract the subtraction tree from the addition tree
			// everything that has a negative coefficient, goes in the subtraction tree,
			// everythnig else into the addition tree

			var (additionList, subtractionList) = SplitCoeffDictToLists(coefficientDict, simplificationParams);

			TreeNode additionRoot = BuildTreeFromKeyCoeffPairs(additionList, simplificationParams);

			// build the subtraction tree

			if (subtractionList.Count == 0)
				return additionRoot;

			var subtractionRoot = BuildTreeFromKeyCoeffPairs(subtractionList, simplificationParams);

			return new Sub(additionRoot, subtractionRoot);
		}

		private (List<(TreeNode, TreeNode)> additionList, List<(TreeNode, TreeNode)> subtractionList) SplitCoeffDictToLists(Dictionary<TreeNode, TreeNode> coefficientDict, SimplificationParams simplificationParams)
		{
			var additionList = new List<(TreeNode, TreeNode)>();
			var subtractionList = new List<(TreeNode, TreeNode)>();

			foreach ((var key, var coeff) in coefficientDict)
			{
				if (key is Constant { value: 0 } || coeff is Constant { value: 0 })
					continue;

				if (coeff.Eval(simplificationParams) is Constant { value: < 0 } c)
				{
					if (key is Mult)
					{
						List<(TreeNode, bool)> multiplicants = TreeUtils.GetAssociativeOperands(key, OperatorType.Mult, OperatorType.Div);

						// simplify the *2 * a * x * 32 down before adding it to the list

						if (multiplicants.Any(item => item.Item1 is Constant { value: < 0 }))
						{
							TreeNode simplifiedKey = new Mult(key, coeff).Simplify(simplificationParams).Eval(simplificationParams);

							additionList.Add((simplifiedKey, new Constant(1)));

							continue;
						}
					}

					c.value *= -1;

					subtractionList.Add((key, c));

					continue;
				}

				if (key is Mult)
				{
					List<(TreeNode, bool)> multiplicants = TreeUtils.GetAssociativeOperands(key, OperatorType.Mult, OperatorType.Div);

					// simplify the *2 * a * x * 32 down before adding it to the list

					if (multiplicants.Any(item => item.Item1 is Constant { value: < 0 }))
					{
						TreeNode simplifiedKey = new Mult(new Mult(key, coeff), new Constant(-1)).Simplify(simplificationParams).Eval(simplificationParams);

						subtractionList.Add((simplifiedKey, new Constant(1)));

						continue;
					}
				}

				additionList.Add((key, coeff));
			}

			return (additionList, subtractionList);
		}

		private Dictionary<TreeNode, TreeNode> ApplyRules(List<(TreeNode, bool)> operands, SimplificationParams simplificationParams)
		{
			Dictionary<char, TreeNode> wildcards;

			var coefficientDict = new Dictionary<TreeNode, TreeNode>();

			foreach ((var node, bool isNodeInverse) in operands)
			{
				bool addToDict = true;

				foreach (var otherNode in coefficientDict.Keys)
				{
					if (isNodeInverse == false)
					{
						// +0 --> just skip
						if (TreeUtils.MatchPattern(
							node.Eval(simplificationParams),
							new Constant(0),
							out wildcards
						))
						{
							addToDict = false;
							break;
						}

						if (node is Constant && otherNode is Constant)
						{
							coefficientDict[otherNode] = new Add(coefficientDict[otherNode], node);
							addToDict = false;
							break;
						}

						// x + x ---> 2x
						if (TreeUtils.MatchPattern(node, otherNode, out _))
						{
							coefficientDict[otherNode] = new Add(
								  coefficientDict[otherNode],
									new Constant(1)
								);
							addToDict = false;
							break;
						}

						// a + c*a ---> (c+1)*a
						if (TreeUtils.MatchPattern(
							node,
							new Mult(
								new Wildcard('c'),
								otherNode
							),
							out wildcards
						))
						{
							coefficientDict[otherNode] = new Add(
										wildcards['c'],
										coefficientDict[otherNode]
									);
							addToDict = false;
							break;
						}

						// c*a + a ---> (c+1)*a
						if (TreeUtils.MatchPattern(
							otherNode,
							new Mult(
								new Wildcard('c'),
								node
							),
							out wildcards
						))
						{
							coefficientDict.Remove(otherNode);
							coefficientDict[node] = new Add(
										wildcards['c'],
										new Constant(1)
									);
							addToDict = false;
							break;
						}

						// c*a + d*a ---> (c+d)*a
						if (TreeUtils.MatchPattern(
							node,
							new Mult(
								new Wildcard('a'),
								new Wildcard('b')
							),
							out wildcards
						))
						{
							TreeNode a = wildcards['a'];
							TreeNode b = wildcards['b'];

							if (TreeUtils.MatchPattern(
								otherNode,
								new Mult(
									new Wildcard('c'),
									new Wildcard('d')
									),
								out wildcards)
							)
							{
								TreeNode c = wildcards['c'];
								TreeNode d = wildcards['d'];

								// eg.: a == c --> a(b+d)
								bool ac = TreeUtils.MatchPattern(a, c, out _);
								bool ad = TreeUtils.MatchPattern(a, d, out _);
								bool bc = TreeUtils.MatchPattern(b, c, out _);
								bool bd = TreeUtils.MatchPattern(b, d, out _);

								(TreeNode? key, TreeNode? value1, TreeNode? value2) = (
									ac ? (a, b, d) :
									ad ? (a, b, c) :
									bc ? (b, a, d) :
									bd ? (b, a, c) :
									(null, null, null)
								);

								if (key is not null && value1 is not null && value2 is not null)
								{
									coefficientDict.Remove(otherNode);

									if (coefficientDict.ContainsKey(key) == false)
										coefficientDict[key] = new Add(value1, value2);
									else
										coefficientDict[key] = new Add(
											coefficientDict[key],
											new Add(value1, value2)
										);

									addToDict = false;
									break;
								}
							}
						}
					}
					else
					{
						// it is an inverse

						// -0 --> just skip
						if (TreeUtils.MatchPattern(
							node.Eval(simplificationParams),
							new Constant(0),
							out wildcards
						))
						{
							addToDict = false;
							break;
						}

						if (node is Constant && otherNode is Constant)
						{
							coefficientDict[otherNode] = new Sub(coefficientDict[otherNode], node);
							addToDict = false;
							break;
						}

						// ax - x ---> (a-1)x
						if (TreeUtils.MatchPattern(node, otherNode, out _))
						{
							coefficientDict[otherNode] = new Add(
								  coefficientDict[otherNode],
									new Constant(-1)
								);
							addToDict = false;
							break;
						}

						// a - c*a ---> (-c+1)*a
						if (TreeUtils.MatchPattern(
							node,
							new Mult(
								new Wildcard('c'),
								otherNode
							),
							out wildcards
						))
						{
							coefficientDict[otherNode] = new Sub(
										coefficientDict[otherNode],
										wildcards['c']
									);
							addToDict = false;
							break;
						}

						// c*a - a ---> (c-1)*a
						if (TreeUtils.MatchPattern(
							otherNode,
							new Mult(
								new Wildcard('c'),
								node
							),
							out wildcards
						))
						{
							coefficientDict.Remove(otherNode);
							coefficientDict[node] = new Sub(
										wildcards['c'],
										new Constant(1)
									);
							addToDict = false;
							break;
						}


						// c*a - d*a ---> (c-d)*a
						if (TreeUtils.MatchPattern(
							node,
							new Mult(
								new Wildcard('a'),
								new Wildcard('b')
							),
							out wildcards
						))
						{
							TreeNode a = wildcards['a'];
							TreeNode b = wildcards['b'];

							if (TreeUtils.MatchPattern(
								otherNode,
								new Mult(
									new Wildcard('c'),
									new Wildcard('d')
									),
								out wildcards)
							)
							{
								TreeNode c = wildcards['c'];
								TreeNode d = wildcards['d'];

								// eg.: a == c --> a(b+d)
								bool ac = TreeUtils.MatchPattern(a, c, out _);
								bool ad = TreeUtils.MatchPattern(a, d, out _);
								bool bc = TreeUtils.MatchPattern(b, c, out _);
								bool bd = TreeUtils.MatchPattern(b, d, out _);

								(TreeNode? key, TreeNode? value1, TreeNode? value2) = (
									ac ? (a, b, d) :
									ad ? (a, b, c) :
									bc ? (b, a, d) :
									bd ? (b, a, c) :
									(null, null, null)
								);

								if (key is not null && value1 is not null && value2 is not null)
								{
									coefficientDict.Remove(otherNode);

									if (coefficientDict.ContainsKey(key) == false)
										coefficientDict[key] = new Sub(value1, value2);
									else
										coefficientDict[key] = new Add(
											coefficientDict[key],
											new Sub(value1, value2)
										);

									addToDict = false;
									break;
								}
							}
						}
					}
				}

				if (addToDict == false)
					continue;

				// new entry
				if (node is Constant constant)
					coefficientDict[new Constant(val: 1)] = isNodeInverse ? new Constant(-constant.value) : node;
				else
					coefficientDict[node] = new Constant(isNodeInverse ? -1 : 1);
			}

			return coefficientDict;
		}

		private TreeNode BuildTreeFromKeyCoeffPairs (List<(TreeNode, TreeNode)> list, SimplificationParams? simplificationParams)
		{
			if (list.Count == 0)
				return new Constant(0);

			if (list.Count == 1)
				return DeconstructKeyCoeffPair(list[0], simplificationParams);

			return new Add(
				DeconstructKeyCoeffPair(list[0], simplificationParams),
				BuildTreeFromKeyCoeffPairs(list.Where((_, i) => i > 0).ToList(), simplificationParams)
			);
		}

		private TreeNode DeconstructKeyCoeffPair ((TreeNode, TreeNode) pair, SimplificationParams? simplificationParams)
		{
			var (key, coeff) = pair;

			if (coeff is Constant { value: 1 })
				return key;

			if (coeff is Constant { value: 0 })
				return new Constant(0);

			if (key is Constant)
				return new Mult(key, coeff).Eval(simplificationParams);

			return new Mult(
				coeff,
				key
			);
		}

		public override string ToLatexString()
		{
			if (operand1 is Constant { value: 0 })
				return operand2.ToLatexString();
			if (operand2 is Constant { value: 0 })
				return operand1.ToLatexString();

			return $@"{{{operand1.ToLatexString()}}} + {{{operand2.ToLatexString()}}}";
		}
	}

	public sealed class Sub : Operator
	{
		public Sub(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Sub, left, right, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);
			operand2 = operand2.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant l && operand2 is Constant r)
				return new Constant(l.value - r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\left(f(x) - g(x)\right) = \frac{{d}}{{dx}}f(x) - \frac{{d}}{{dx}}g(x)",
				operand1.ToLatexString(),
				operand2.ToLatexString()
			));

			return new Sub(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
				operand2 = operand2.Simplify(simplificationParams);
			}

			Dictionary<char, TreeNode> wildcards;

			// b-(-a) = b+a
			if (TreeUtils.MatchPattern(
				operand2,
				new Mult(
					  new Constant(-1),
					  new Wildcard('a')
				),
				out wildcards
			))
			{
				return new Add(operand1, wildcards['a']).Simplify(simplificationParams, false);
			}

			// b-(0-a) = b+a
			if (TreeUtils.MatchPattern(
				operand2,
				new Sub(
					  new Constant(0),
					  new Wildcard('a')
				),
				out wildcards
			))
			{
				return new Add(operand1, wildcards['a']).Simplify(simplificationParams, false);
			}

			return new Add(
				new Constant(0),
				this
			).Simplify(simplificationParams, true);
		}

		public override string ToLatexString()
		{
			bool leaveRightParenthesisOut = operand2 is not Operator
				|| operand2 is Operator { basePriority: > 1 }
				|| operand2 is Operator { numOperands: 1 };

			bool changeSubToAdd = operand2.ToLatexString().Trim().StartsWith("-");

			string operand2LatexString = changeSubToAdd ? operand2.ToLatexString().Trim()[1..] : operand2.ToLatexString();

			if (operand1 is Constant { value: 0.0 } || operand1 is Constant { value: -0.0 })
			{
				// it is just a sign

				if (leaveRightParenthesisOut)
				{
					if (changeSubToAdd)
						return $@"+{{{operand2LatexString}}}";
					else
						return $@"-{{{operand2LatexString}}}";
				}

				if (changeSubToAdd)
					return $@"+\left({{{operand2LatexString}}}\right)";
				else
					return $@"-\left({{{operand2LatexString}}}\right)";
			}

			if (changeSubToAdd)
				if (leaveRightParenthesisOut)
					return $@"{{{operand1.ToLatexString()}}} + {{{operand2LatexString}}}";
				else
					return $@"{{{operand1.ToLatexString()}}} + \left({{{operand2LatexString}}}\right)";
			else
				if (leaveRightParenthesisOut)
					return $@"{{{operand1.ToLatexString()}}} - {{{operand2LatexString}}}";
				else
					return $@"{{{operand1.ToLatexString()}}} - \left({{{operand2LatexString}}}\right)";
		}
	}

	public sealed class Mult : Operator
	{
		public Mult(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Mult, left, right, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);
			operand2 = operand2.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant l && operand2 is Constant r)
				return new Constant(l.value * r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			if (operand1.IsConstant(varToDiff))
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}\left(c \cdot f(x)\right) = c \cdot \frac{{d}}{{dx}}f(x)",
					operand2.ToLatexString()
				));
			else if (operand2.IsConstant(varToDiff))
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}\left(c \cdot f(x)\right) = c \cdot \frac{{d}}{{dx}}f(x)",
					operand1.ToLatexString()
				));
			else
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}\left(f(x) \cdot g(x)\right) = f(x) \cdot \frac{{d}}{{dx}}g(x) + g(x) \cdot \frac{{d}}{{dx}}f(x)",
					operand1.ToLatexString(),
					operand2.ToLatexString()
				));

			if (operand1.IsConstant(varToDiff))
				return new Mult(operand1, operand2.Diff(varToDiff));

			if (operand2.IsConstant(varToDiff))
				return new Mult(operand2, operand1.Diff(varToDiff));

			return new Add(
				new Mult(operand1.Diff(varToDiff), operand2),
				new Mult(operand1, operand2.Diff(varToDiff))
			);
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
				operand2 = operand2.Simplify(simplificationParams);
			}

			var operands = TreeUtils.GetAssociativeOperands(this, type, inverseType);

			foreach ((var node, bool isNodeInverse) in operands)
			{
				if (node.Eval(simplificationParams) is Constant { value: 0.0 } || node.Eval(simplificationParams) is Constant { value: -0.0 })
				{
					if (isNodeInverse)
						throw new DivideByZeroException();
					else
						return new Constant(0);
				}
			}

			if (operands.Count < 2)
				return this;

			var (powerDict, constantPart) = ApplyRules(operands, simplificationParams);

			if (constantPart != 1.0)
				powerDict.Add(new Constant(constantPart), new Constant(1));

			if (powerDict.Keys.Count == 0)
				return new Constant(1);

			// if we have managed to simplify to a single expression
			if (powerDict.Keys.Count == 1)
			{
				(var key, var pow) = powerDict.First();

				if (pow is Constant { value: 1 })
					return key;
				else if (pow is Constant { value: -1 })
					return new Div(new Constant(1), key);
				else
					return new Pow(
						powerDict.Keys.First(),
						powerDict.Values.First()
					);
			}

			// building 2 trees
			// same trick, as with the addition / subtraction

			var multList = new List<(TreeNode, TreeNode)>();
			var divList = new List<(TreeNode, TreeNode)>();

			foreach ((var key, var power) in powerDict)
			{
				if (power.Eval(simplificationParams) is Constant { value: < 0 } pow)
				{
					pow.value *= -1;

					divList.Add((key, pow));
				}
				else
				{
					multList.Add((key, power));
				}
			}

			multList = SortKeyPowPairsByVarNames(multList, simplificationParams.varToDiff);
			divList = SortKeyPowPairsByVarNames(divList, simplificationParams.varToDiff);

			var multRoot = BuildTreeFromKeyPowPais(multList);

			if (divList.Count == 0)
				return multRoot;

			var divRoot = BuildTreeFromKeyPowPais(divList);

			return new Div(multRoot, divRoot);
		}

		private (Dictionary<TreeNode, TreeNode> powerDict, double constantPart) ApplyRules (List<(TreeNode, bool)> operands, SimplificationParams simplificationParams)
		{
			Dictionary<char, TreeNode> wildcards;

			double constantPart = 1.0;

			var powerDict = new Dictionary<TreeNode, TreeNode>();

			foreach ((var node, bool isNodeInverse) in operands)
			{
				bool addToDict = true;

				foreach (var otherNode in powerDict.Keys)
				{
					if (isNodeInverse == false)
					{
						// *1 ---> skip
						if (TreeUtils.MatchPattern(
							node.Eval(simplificationParams),
							new Constant(1),
							out wildcards
						))
						{
							addToDict = false;
							break;
						}

						// *0 = 0
						if (node.Eval(simplificationParams) is Constant { value: 0 })
						{
							return (new (), 0.0);
						}

						// othernode    node
						//     a     *   a^c   ---> a^(c+1)
						if (TreeUtils.MatchPattern(
							node,
							new Pow(
								otherNode,
								new Wildcard('c')
							),
							out wildcards
						))
						{
							powerDict[otherNode] = new Add(
										wildcards['c'],
										powerDict[otherNode]
									);
							addToDict = false;
							break;
						}

						//   node     othernode
						//     a   *     a^c   ---> a^(c+1)
						if (TreeUtils.MatchPattern(
							otherNode,
							new Pow(
								node,
								new Wildcard('c')
							),
							out wildcards
						))
						{
							powerDict.Remove(otherNode);
							powerDict[node] = new Add(
										wildcards['c'],
										new Constant(1)
									);
							addToDict = false;
							break;
						}

						// x * x ---> x^2
						if (TreeUtils.MatchPattern(node, otherNode, out _))
						{
							powerDict[otherNode] = new Constant(2);
							addToDict = false;
							break;
						}

						// a^b * a^c ---> a^(b+c)
						if (TreeUtils.MatchPattern(
							node,
							new Pow(
								new Wildcard('a'),
								new Wildcard('b')
							),
							out wildcards
						))
						{
							TreeNode a = wildcards['a'];
							TreeNode b = wildcards['b'];

							if (TreeUtils.MatchPattern(
								otherNode,
								new Pow(
									new Wildcard('c'),
									new Wildcard('d')
									),
								out wildcards)
							)
							{
								TreeNode c = wildcards['c'];
								TreeNode d = wildcards['d'];

								// eg.: a == c --> a^(b+d)
								bool ac = TreeUtils.MatchPattern(a, c, out _);

								// we only care about matching bases
								(TreeNode? key, TreeNode? value1, TreeNode? value2) = (
									ac ? (a, b, d) :
									(null, null, null)
								);

								if (key is not null && value1 is not null && value2 is not null)
								{
									powerDict.Remove(otherNode);

									if (powerDict.ContainsKey(key) == false)
										powerDict[key] = new Add(value1, value2);
									else
										powerDict[key] = new Add(
											powerDict[key],
											new Add(value1, value2)
										);

									addToDict = false;
									break;
								}
							}
						}

						// a^c * b^c ---> (a*b)^c
						if (TreeUtils.MatchPattern(
							new Mult(
								node,
								new Pow(
									otherNode,
						powerDict[otherNode]
						).Simplify(simplificationParams) // simplify, because it might have nested pows,which can be simplified down
							),
							new Mult(
								new Pow(
									new Wildcard('a'),
									new Wildcard('c')
								),
								new Pow(
									new Wildcard('b'),
									new Wildcard('c')
								)
							),
							out wildcards
						))
						{
							powerDict.Remove(otherNode);

							powerDict.Add(
								new Mult(
									wildcards['a'],
									wildcards['b']
								),
								wildcards['c']
							);

							addToDict = false;

							break;
						}
					}
					else
					{
						// node is inverse

						// /1 ---> skip
						if (TreeUtils.MatchPattern(
							node.Eval(simplificationParams),
							new Constant(1),
							out wildcards
						))
						{
							addToDict = false;
							break;

						}

						// /0 = ERR
						if (TreeUtils.MatchPattern(
							node.Eval(simplificationParams),
							new Constant(0),
							out wildcards
						))
						{
							throw new DivideByZeroException();
						}

						// othernode    node
						//     a     /   a^c   ---> a^(-c+1)
						if (TreeUtils.MatchPattern(
							node,
							new Pow(
								otherNode,
								new Wildcard('c')
							),
							out wildcards
						))
						{
							powerDict[otherNode] = new Sub(
										powerDict[otherNode],
										wildcards['c']
									);
							addToDict = false;
							break;
						}

						//  othernode     node
						//     a^c     /    a   ---> a^(c-1)
						if (TreeUtils.MatchPattern(
							otherNode,
							new Pow(
								node,
								new Wildcard('c')
							),
							out wildcards
						))
						{
							powerDict.Remove(otherNode);
							powerDict[node] = new Sub(
										wildcards['c'],
										new Constant(1)
									);
							addToDict = false;
							break;
						}

						// x / x ---> 1
						if (TreeUtils.MatchPattern(node, otherNode, out _))
						{
							powerDict.Remove(otherNode);
							addToDict = false;
							break;
						}

						// a^b / a^c ---> a^(b-c)
						if (TreeUtils.MatchPattern(
							node,
							new Pow(
								new Wildcard('a'),
								new Wildcard('b')
							),
							out wildcards
						))
						{
							TreeNode a = wildcards['a'];
							TreeNode b = wildcards['b'];

							if (TreeUtils.MatchPattern(
								otherNode,
								new Pow(
									new Wildcard('c'),
									new Wildcard('d')
									),
								out wildcards)
							)
							{
								TreeNode c = wildcards['c'];
								TreeNode d = wildcards['d'];

								// eg.: a == c --> a^(b+d)
								bool ac = TreeUtils.MatchPattern(a, c, out _);

								// we only care about matching bases
								(TreeNode? key, TreeNode? value1, TreeNode? value2) = (
									ac ? (a, b, d) :
									(null, null, null)
								);

								// IMPORTANT: value2 - value1 is the new power!!!!

								if (key is not null && value1 is not null && value2 is not null)
								{
									powerDict.Remove(otherNode);

									if (powerDict.ContainsKey(key) == false)
										powerDict[key] = new Sub(value2, value1);
									else
										powerDict[key] = new Add(
											powerDict[key],
											new Sub(value2, value1)
										);

									addToDict = false;
									break;
								}
							}
						}

						// a^c / b^c ---> (a/b)^c
						if (TreeUtils.MatchPattern(
							new Div(
								node,
								new Pow(
									otherNode,
									powerDict[otherNode]
								)
							),
							new Div(
								new Pow(
									new Wildcard('a'),
									new Wildcard('c')
								),
								new Pow(
									new Wildcard('b'),
									new Wildcard('c')
								)
							),
							out wildcards
						))
						{
							powerDict.Remove(otherNode);

							powerDict.Add(
								new Div(
									wildcards['a'],
									wildcards['b']
								),
								wildcards['c']
							);

							addToDict = false;

							break;
						}

						// sina / cosa = tana
						if (
							node is Operator { type: OperatorType.Cos } opCos
							&&
							otherNode is Operator { type: OperatorType.Sin } opSin
							&&
							TreeUtils.MatchPattern(
								opCos.operand1,
								opSin.operand1,
								out _
							)
						)
						{
							// tan(a)

							powerDict.Remove(otherNode);

							addToDict = false;

							powerDict.Add(
									new Tan(
										opSin.operand1
									),
									new Constant(1)
								);

							break;
						}
					}
				}

				if (addToDict == false)
					continue;

				// new entry
				if (node is Constant constant)
					if (isNodeInverse == false)
						constantPart *= constant.value;
					else
						constantPart /= constant.value;
				else
					powerDict[node] = new Constant(isNodeInverse ? -1 : 1);
			}

			return (powerDict, constantPart);
		}

		private TreeNode BuildTreeFromKeyPowPais (List<(TreeNode, TreeNode)> list)
		{
			if (list.Count <= 0)
				return new Constant(1);

			if (list.Count == 1)
				return DeconstructKeyPowPair(list[0]);

			return new Mult(
				DeconstructKeyPowPair(list[0]), 
				BuildTreeFromKeyPowPais(list.Where((_, i) => i > 0).ToList())
			);
		}

		private TreeNode DeconstructKeyPowPair((TreeNode, TreeNode) pair)
		{
			var (key, pow) = pair;

			if (pow is Constant { value: 1 })
				return key;

			if (pow is Constant { value: 0 })
				return new Constant(1);

			return new Pow(key, pow);
		}

		private static List<(TreeNode, TreeNode)> SortKeyPowPairsByVarNames(List<(TreeNode, TreeNode)> list, char? varToLeaveLast = null)
		{
			if (list.Count <= 1)
				return list;

			Func<TreeNode, char?> GetVarName = key =>
			{
				if (key is Variable v)
					return v.name;

				if (key is Mult { operand1: Variable } mult1)
					return (mult1.operand1 as Variable).name;

				if (key is Mult { operand2: Variable } mult2)
					return (mult2.operand2 as Variable).name;

				if (key is Pow { operand1: Variable } pow1)
					return (pow1.operand1 as Variable).name;

				return null;
			};

			(TreeNode, TreeNode) pivot = (null, null);
			char pivotName = 'a';

			foreach ((var key, var pow) in list)
			{
				char? varName = GetVarName(key);

				if (varName is not null)
				{
					pivot = (key, pow);
					pivotName = (char)varName;
					list.Remove((key, pow));
					break;
				}
			}

			if (pivot == (null, null))
				return list;

			List<(TreeNode, TreeNode)> constList = new();
			List<(TreeNode, TreeNode)> leftList = new();
			List<(TreeNode, TreeNode)> rightList = new();
			List<(TreeNode, TreeNode)> expressionList = new();

			foreach ((var key, var pow) in list)
			{
				char? varName = GetVarName(key);

				if (varName is not null)
				{
					if (varName == varToLeaveLast)
					{
						rightList.Add((key, pow));
						continue;
					}

					if (varName > pivotName)
						rightList.Add((key, pow));
					else
						leftList.Add((key, pow));

					continue;
				}

				if (key is Constant)
				{
					constList.Add((key, pow));

					continue;
				}

				expressionList.Add((key, pow));
			}

			leftList = SortKeyPowPairsByVarNames(leftList, varToLeaveLast);
			rightList = SortKeyPowPairsByVarNames(rightList, varToLeaveLast);

			if (pivotName == varToLeaveLast)
				return constList.Concat(leftList).Concat(rightList).Concat(new List<(TreeNode, TreeNode)> { pivot }).Concat(expressionList).ToList();

			return constList.Concat(leftList).Concat(new List<(TreeNode, TreeNode)> { pivot }).Concat(rightList).Concat(expressionList).ToList();
		}

		public override string ToLatexString()
		{

			bool switchOperandOrder = operand2 is Constant && operand1 is not Constant
										|| (operand1 is Operator op1 && operand2 is Operator op2 && op1.basePriority > op2.basePriority)
										|| (operand2 is Variable && operand1 is Operator)
										|| operand2 is Pow { operand2: Constant { value: 0.5 } };

			var leftOperand = switchOperandOrder ? operand2 : operand1;
			var rightOperand = switchOperandOrder ? operand1 : operand2;

			bool leaveLeftParenthesisOut = leftOperand is not Operator 
											|| leftOperand is Operator { basePriority: > 1 };
			bool leaveRightParenthesisOut = (
												rightOperand is not Operator 
												|| rightOperand is Operator { basePriority: > 1 }
											) 
											&& rightOperand is not Pow { operand1: Constant };

			bool leaveMultiplicationSignOut = (
												(leftOperand is Constant ^ rightOperand is Constant)
												|| (leftOperand is Variable && rightOperand is Variable)
												|| rightOperand is Operator { numOperands: 1 }
												|| rightOperand is Operator { basePriority: > 2 }
												|| leftOperand is Mult
												|| rightOperand is Mult
												|| leaveRightParenthesisOut == false
											)
											&& leftOperand is not DerivativeSymbol && rightOperand is not DerivativeSymbol;


			string leftPart = $"{{{(leaveLeftParenthesisOut ? "" : @"\left(")}" +
								$"{leftOperand.ToLatexString()}" +
								$"{(leaveLeftParenthesisOut ? "" : @"\right)")}}}";

			if (leftOperand is Constant { value: -1 })
			{
				leftPart = "-";
				leaveMultiplicationSignOut = true;
			}

			// after square root, there should be a dot
			if (leftOperand is Pow { operand2: Constant { value: 0.5 } })
				leaveMultiplicationSignOut = false;

			string rightPart = $"{{{(leaveRightParenthesisOut ? "" : @"\left(")}" +
								$"{rightOperand.ToLatexString()}" +
								$"{(leaveRightParenthesisOut ? "" : @"\right)")}}}";

			string multiplicationSign = leaveMultiplicationSignOut ? "" : @" \cdot ";
			return $"{leftPart}{multiplicationSign}{rightPart}";
		}
	}

	public sealed class Div : Operator
	{
		public Div(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Div, left, right, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);
			operand2 = operand2.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant l && operand2 is Constant r)
				return new Constant(l.value / r.value);
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			if (operand2.IsConstant(varToDiff))
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}\frac{{f(x)}}{{c}} = \frac{{\frac{{d}}{{dx}}f(x)}}{{c}}",
					operand1.ToLatexString()
				));
			else
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}\frac{{f(x)}}{{g(x)}} = \frac{{f(x) \cdot \frac{{d}}{{dx}}g(x) - g(x) \cdot \frac{{d}}{{dx}}f(x)}}{{{{g(x)}}^2}}",
					operand1.ToLatexString(),
					operand2.ToLatexString()
				));

			if (operand2.IsConstant(varToDiff))
				return new Div(operand1.Diff(varToDiff), operand2);

			return new Div(
				new Sub(
					new Mult(operand1.Diff(varToDiff), operand2),
					new Mult(operand1, operand2.Diff(varToDiff))
				),
				new Pow(operand2, new Constant(2))
			);
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
				operand2 = operand2.Simplify(simplificationParams);
			}

			if (operand1.Eval(simplificationParams) is Constant { value: 0 })
				return new Constant(0);

			return new Mult(
				new Constant(1),
				this
			).Simplify(simplificationParams, true);
		}

		public override string ToLatexString()
		{
			return $@"\frac{{{operand1.ToLatexString()}}}{{{operand2.ToLatexString()}}}";
		}
	}

	public sealed class Pow : Operator
	{
		public Pow(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Pow, left, right, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);
			operand2 = operand2.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant l && operand2 is Constant r)
				return new Constant(Math.Pow(l.value, r.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			if (operand1.IsConstant(varToDiff))
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}{{c^{{f(x)}}}} = ln(c) \cdot {{c^{{f(x)}}}} \cdot \frac{{d}}{{dx}}f(x)",
					operand2.ToLatexString()
				));
			else if (operand2.IsConstant(varToDiff))
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}{{f(x)}}^c = c \cdot f(x)^{{c-1}} \cdot \frac{{d}}{{dx}}f(x)",
					operand1.ToLatexString()
				));
			else
				Differentiator.AddStepDescription(new StepDescription(
					$@"\frac{{d}}{{dx}}f(x)^{{g(x)}} = \frac{{d}}{{dx}}e^{{g(x)\cdot \ln\left(f\left(x\right)\right)}} = e^{{g(x)\cdot \ln\left(f\left(x\right)\right)}}\cdot\frac{{d}}{{dx}}{{g(x)\cdot \ln\left(f\left(x\right)\right)}} = f(x)^{{g(x)}}\cdot\frac{{d}}{{dx}}{{g(x)\cdot \ln\left(f\left(x\right)\right)}}",
					operand1.ToLatexString(),
					operand1.ToLatexString()
				));

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
					new Mult(
						operand2,
						new Pow(
							operand1,
							new Sub(
								operand2,
								new Constant(1)
							)
						)
					),
					operand1.Diff(varToDiff)
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

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			Dictionary<char, TreeNode> wildcards;

			// we have to check before and after as well, because the simplification might mess up the multiplication

			// e^(lna*b) = a^b
			if (TreeUtils.MatchPattern(
				this,
				new Pow(
					Constant.E,
					new Mult(
						new Ln(new Wildcard('a')),
						new Wildcard('b')
					)
				),
				out wildcards
			))
			{
				return new Pow(
					wildcards['a'],
					wildcards['b']
				);
			}

			// 10^(loga*b) = a^b
			if (TreeUtils.MatchPattern(
				this,
				new Pow(
					new Constant(10),
					new Mult(
						new Log(new Wildcard('a')),
						new Wildcard('b')
					)
				),
				out wildcards
			))
			{
				return new Pow(
					wildcards['a'],
					wildcards['b']
				);
			}

			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
				operand2 = operand2.Simplify(simplificationParams);
			}

			if (operand1.Eval(simplificationParams) is Constant { value: 1 })
				return new Constant(1);

			if (operand1.Eval(simplificationParams) is Constant { value: 0 })
				return new Constant(0);

			if (operand2.Eval(simplificationParams) is Constant { value: 1 })
				return operand1;

			if (operand2.Eval(simplificationParams) is Constant { value: 0 })
				return new Constant(1);

			if (operand2.Eval(simplificationParams) is Constant { value: -1 })
				return new Div(new Constant(1), operand1);

			// (a^b)^c = a^(b*c)
			if (TreeUtils.MatchPattern(
				this,
				new Pow(
					new Pow(
						new Wildcard('a'),
						new Wildcard('b')
					),
					new Wildcard('c')
				),
				out wildcards
			))
			{
				return new Pow(
					wildcards['a'],
					new Mult(
						wildcards['b'],
						wildcards['c']
					)
				);
			}

			// e^(lna*b) = a^b
			if (TreeUtils.MatchPattern(
				this,
				new Pow(
					Constant.E,
					new Mult(
						new Ln(
							new Wildcard('a')
						),
						new Wildcard('b')
					)
				),
				out wildcards
			))
			{
				return new Pow(
					wildcards['a'],
					wildcards['b']
				);
			}

			// 10^(loga*b) = a^b
			if (TreeUtils.MatchPattern(
				this,
				new Pow(
					new Constant(10),
					new Mult(
						new Log(
							 new Wildcard('a')
							),
						new Wildcard('b')
					)
				),
				out wildcards
			))
			{
				return new Pow(
					wildcards['a'],
					wildcards['b']
				);
			}


			return this;
		}

		public override string ToLatexString()
		{
			if (operand2 is Constant { value: 0.5 })
				return $@"\sqrt{{{operand1.ToLatexString()}}}";

			bool leaveBaseParenthesisOut = operand1 is not Operator
										|| operand1 is Operator { numOperands: 1 };
			bool leavePowerParenthesisOut = operand2 is not Pow;

			string operand1String = operand1.ToLatexString();

			if (operand1String.StartsWith("-"))
				operand1String = $@"\left({{{operand1.ToLatexString()}}}\right)";

			return $"{(leaveBaseParenthesisOut ? "" : @"\left(")}" +
				$"{{{operand1String}}}" +
				$"{(leaveBaseParenthesisOut ? "" : @"\right)")}" +
				$"^" +
				$"{(leavePowerParenthesisOut ? "" : @"\left(")}" +
				$"{{{operand2.ToLatexString()}}}" +
				$"{(leavePowerParenthesisOut ? "" : @"\right)")}";
		}
	}

	public sealed class Sin : Operator
	{
		public Sin(TreeNode? operand = null, int? priority = null) : base(OperatorType.Sin, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Sin(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\sin(x) = \cos(x)",
				operand1.ToLatexString()
			));

			return new Mult(
				new Cos(operand1),
				operand1.Diff(varToDiff)
			);
		}

		public override string ToLatexString()
		{
			return $@"\sin\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Cos : Operator
	{
		public Cos(TreeNode? operand = null, int? priority = null) : base(OperatorType.Cos, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Cos(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\cos(x) = -\sin(x)",
				operand1.ToLatexString()
			));

			return new Mult(
				new Mult(
					new Constant(-1),
					new Sin(operand1)
				),
				operand1.Diff(varToDiff)
			);
		}

		public override string ToLatexString()
		{
			return $@"\cos\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Tan : Operator
	{
		public Tan(TreeNode? operand = null, int? priority = null) : base(OperatorType.Tan, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Tan(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\tan(x) = \frac{{1}}{{{{\cos\left(x\right)}}^2}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Pow(
					new Cos(operand1),
					new Constant(2)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\tan\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Ln : Operator
	{
		public Ln(TreeNode? operand = null, int? priority = null) : base(OperatorType.Ln, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
			{
				//if (c.value <= 0)
				//	throw new NotFiniteNumberException("Ln only takes in positive!");

				if (c.value <= 0)
					return this;

				return new Constant(Math.Log(c.value));
			}
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\ln(x) = \frac{{1}}{{x}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				operand1
			);
		}

		public override string ToLatexString()
		{
			return $@"\ln\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			if (operand1 is Constant { value: Math.E })
				return new Constant(1);

			Dictionary<char, TreeNode> wildcards;

			if (TreeUtils.MatchPattern(
				operand1,
				new Pow(
					Constant.E,
					new Wildcard('a')
				),
				out wildcards
			))
				return wildcards['a'];

			return this;
		}
	}

	public sealed class Log : Operator
	{
		public Log(TreeNode? operand = null, int? priority = null) : base(OperatorType.Log, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
			{
				//if (c.value <= 0)
				//	throw new NotFiniteNumberException("Log only takes in positive!");

				if (c.value <= 0)
					return this;

				return new Constant(Math.Log10(c.value));
			}
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\log_{{10}}(x) = \frac{{1}}{{\ln\left(10\right)\cdotx}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Mult(
					new Ln(new Constant(10)),
					operand1
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\log_{{10}}\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Cot : Operator
	{
		public Cot(TreeNode? operand = null, int? priority = null) : base(OperatorType.Cot, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Tan(1.0d / c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\cot(x) = \frac{{-1}}{{{{\sin\left(x\right)}}^2}}",
				operand1.ToLatexString()
			));

			return new Div(
				new Mult(
					new Constant(-1),
					operand1.Diff(varToDiff)
				),
				new Pow(
					new Sin(operand1),
					new Constant(2)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\cot\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arcsin : Operator
	{
		public Arcsin(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arcsin, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Asin(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\arcsin(x) = \frac{{1}}{{\sqrt{{1-x^2}}}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Pow(
					new Sub(
						new Constant(1),
						new Pow(
							operand1,
							new Constant(2)
						)
					),
					new Constant(0.5)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\arcsin\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arccos : Operator
	{
		public Arccos(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arccos, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Acos(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\arccos(x) = \frac{{1}}{{\sqrt{{x^2-1}}}}",
				operand1.ToLatexString()
			));

			return new Div(
				new Mult(
					new Constant(-1),
					operand1.Diff(varToDiff)
				),
				new Pow(
					new Sub(
						new Pow(
							operand1,
							new Constant(2)
						),
						new Constant(1)
					),
					new Constant(0.5)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\arccos\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arctan : Operator
	{
		public Arctan(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arctan, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Atan(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\arctan(x) = \frac{{1}}{{x^2+1}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Add(
					new Pow(
						operand1,
						new Constant(2)
					),
					new Constant(1)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\arctan\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arccot : Operator
	{
		public Arccot(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arccot, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Atan(1.0d / c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\arccot(x) = \frac{{-1}}{{\sqrt{{1-x^2}}}}",
				operand1.ToLatexString()
			));

			return new Div(
				new Mult(
					new Constant(-1),
					operand1.Diff(varToDiff)
				),
				new Add(
					new Pow(
						operand1,
						new Constant(2)
					),
					new Constant(1)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\arccot\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Sinh : Operator
	{
		public Sinh(TreeNode? operand = null, int? priority = null) : base(OperatorType.Sinh, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Sinh(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\sinh(x) = \cosh(x)",
				operand1.ToLatexString()
			));

			return new Mult(
				new Cosh(operand1),
				operand1.Diff(varToDiff)
			);
		}

		public override string ToLatexString()
		{
			return $@"\sinh\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Cosh : Operator
	{
		public Cosh(TreeNode? operand = null, int? priority = null) : base(OperatorType.Cosh, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Cosh(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\cosh(x) = \sinh(x)",
				operand1.ToLatexString()
			));

			return new Mult(
				new Sinh(operand1),
				operand1.Diff(varToDiff)
			);
		}

		public override string ToLatexString()
		{
			return $@"\cosh\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Tanh : Operator
	{
		public Tanh(TreeNode? operand = null, int? priority = null) : base(OperatorType.Tanh, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Tanh(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\tanh(x) = \frac{{1}}{{{{\cosh\left(x\right)}}^2}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Pow(
					new Cosh(operand1),
					new Constant(2)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\tanh\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Coth : Operator
	{
		public Coth(TreeNode? operand = null, int? priority = null) : base(OperatorType.Coth, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Tanh(1.0d / c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\coth(x) = \frac{{-1}}{{{{\cosh\left(x\right)}}^2}}",
				operand1.ToLatexString()
			));

			return new Div(
				new Mult(
					new Constant(-1),
					operand1.Diff(varToDiff)
				),
				new Pow(
					new Sinh(operand1),
					new Constant(2)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"\coth\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arsinh : Operator
	{
		public Arsinh(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arsinh, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Asinh(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}arsinh(x) = \frac{{1}}{{\sqrt{{x^2+1}}}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Pow(
					new Add(
						new Pow(
							operand1,
							new Constant(2)
						),
						new Constant(1)
					),
					new Constant(0.5)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"arsinh\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arcosh : Operator
	{
		public Arcosh(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arcosh, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Acosh(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}arcosh(x) = \frac{{1}}{{\sqrt{{x^2-1}}}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Pow(
					new Sub(
						new Pow(
							operand1,
							new Constant(2)
						),
						new Constant(1)
					),
					new Constant(0.5)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"arcosh\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Artanh : Operator
	{
		public Artanh(TreeNode? operand = null, int? priority = null) : base(OperatorType.Artanh, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Atanh(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}artanh(x) = \frac{{1}}{{1-x^2}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Sub(
					new Constant(1),
					new Pow(
						operand1,
						new Constant(2)
					)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"artanh\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Arcoth : Operator
	{
		public Arcoth(TreeNode? operand = null, int? priority = null) : base(OperatorType.Arcoth, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Atanh(1.0d / c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}arcoth(x) = \frac{{1}}{{1-x^2}}",
				operand1.ToLatexString()
			));

			return new Div(
				operand1.Diff(varToDiff),
				new Sub(
					new Constant(1),
					new Pow(
						operand1,
						new Constant(2)
					)
				)
			);
		}

		public override string ToLatexString()
		{
			return $@"arcoth\left({{{operand1.ToLatexString()}}}\right)";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			return this;
		}
	}

	public sealed class Abs : Operator
	{
		public Abs(TreeNode? operand = null, int? priority = null) : base(OperatorType.Abs, operand, null, priority) { }

		public override TreeNode Eval(SimplificationParams simplificationParams = null)
		{
			operand1 = operand1.Eval(simplificationParams);

			if (this.ShouldEval(simplificationParams) == false)
				return this;

			if (operand1 is Constant c)
				return new Constant(Math.Abs(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			Differentiator.AddStepDescription(new StepDescription(
				$@"\frac{{d}}{{dx}}\lvert x \rvert = \frac{{\lvert x \rvert}}{{x}}",
				operand1.ToLatexString()
			));

			return new Mult(
				new Div(
					new Abs(operand1),
					operand1
				),
				operand1.Diff(varToDiff)
			);
		}

		public override string ToLatexString()
		{
			return $@"\lvert {{{operand1.ToLatexString()}}} \rvert";
		}

		public override TreeNode Simplify(SimplificationParams simplificationParams, bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify(simplificationParams);
			}

			if (operand1 is Abs)
				return operand1;

			if (operand1 is Pow { operand1: Constant { value: > 0 } }) // c^x where c > 0
				return operand1;

			if (
				operand1 is Pow { operand2: Constant exponent } 
				&& Math.Floor(exponent.value) == exponent.value // int
				&& (int)exponent.value % 2 == 0 // even
			) // x^n where n is even
				return operand1;

			if (operand1 is Pow { operand2: Constant { value: < 1 } }) // sqrt, etc.
				return operand1;

			if (operand1 is Cosh)
				return operand1;

			if (operand1 is Arccot)
				return operand1;

			if (operand1 is Mult mult)
			{
				if (mult.operand1 is Constant { value: < 0 } c1)
					return new Abs(
						new Mult(
							new Constant(-c1.value),
							mult.operand2
						)
					);

				if (mult.operand2 is Constant { value: < 0 } c2)
					return new Abs(
						new Mult(
							mult.operand1,
							new Constant(-c2.value)
						)
					);
			}

			return this;
		}
	}
}