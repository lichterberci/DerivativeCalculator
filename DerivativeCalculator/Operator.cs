using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DerivativeCalculator
{
	public enum OperatorType
	{
		Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log, Ln
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
		public static Operator GetClassInstanceFromType (OperatorType _type, TreeNode? operand1 = null, TreeNode? operand2 = null, int? priority = null)
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
		public static bool IsOperatorTypeCommutative (OperatorType type)
		{
			switch (type)
			{
				case OperatorType.Add:
					return true;
				case OperatorType.Sub:
					return false;
				case OperatorType.Mult:
					return true;
				case OperatorType.Div:
					return false;
				case OperatorType.Pow:
					return false;
				case OperatorType.Sin:
					return false;
				case OperatorType.Cos:
					return false;
				case OperatorType.Tan:
					return false;
				case OperatorType.Log:
					return false;
				case OperatorType.Ln:
					return false;
				default:
					return false;
			}
		}		
		public bool isCommutative { 
			get
			{
				return IsOperatorTypeCommutative(this.type);
			} 
		}
		public static OperatorType? GetInverse (OperatorType type)
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
	}

	public sealed class Add : Operator
	{
		public Add(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Add, left, right, priority) { }

		public override TreeNode Eval ()
		{
			operand1 = operand1.Eval();
			operand2 = operand2.Eval();

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

			return new Add(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}

		public override TreeNode Simplify(bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify();
				operand2 = operand2.Simplify();
			}

			// associative things

			var operands = TreeUtils.GetAssociativeOperands(this, type, inverseType);

			Dictionary<char, TreeNode> wildcards;

			if (operands.Count >= 2)
			{
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
								node.Eval(),
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
								node.Eval(),
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
						coefficientDict[new Constant(1)] = isNodeInverse ? new Constant(-constant.value) : node;
					else
						coefficientDict[node] = new Constant(isNodeInverse ? -1 : 1);
				}

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

				var additionList = new List<(TreeNode, TreeNode)>();
				var subtractionList = new List<(TreeNode, TreeNode)>();

				foreach ((var key, var coeff) in coefficientDict)
				{
					if (coeff.Eval() is Constant { value: < 0 } c)
					{
						c.value *= -1;

						subtractionList.Add((key, c));
					}
					else if (coeff.Eval() is not Constant { value: 0 })
					{
						additionList.Add((key, coeff));
					}
				}

				Operator head = new Add(null, null);

				TreeNode additionRoot = head;
								
				if (additionList.Count == 1)
				{
					// a - (b + c + d + ...)

					(var key, var coeff) = additionList.First();

					if (coeff is Constant { value: 1 })
					{
						additionRoot = key;
					}
					else
					{
						additionRoot = new Mult(
							coeff,
							key
						);
					}
				}
				else if (additionList.Count == 0)
				{
					// -(a+b+c+...)
					additionRoot = new Constant(0);
				}
				else
				{
					while (additionList.Count >= 2)
					{
						(var key, var coeff) = additionList.Last();

						if (coeff is Constant { value: 1 })
						{
							head.operand1 = key;
						}
						else
						{
							head.operand1 = new Mult(
									coeff,
									key
								);
						}

						additionList.Remove((key, coeff));

						if (additionList.Count == 1)
						{
							(var key2, var coeff2) = additionList.Last();

							if (coeff2 is Constant { value: 1 })
							{
								head.operand2 = key2;
							}
							else
							{
								head.operand2 = new Mult(
										coeff2,
										key2
									);
							}

							additionList.Remove((key2, coeff2));
						}
						else
						{
							head.operand2 = new Add(null, null);
							head = head.operand2 as Operator;
						}
					}

				}

				// build the subtraction tree

				head = new Add(null, null);
				TreeNode subtractionRoot = head;

				if (subtractionList.Count == 1)
				{
					// (b + c + d + ...) - a

					(var key, var coeff) = subtractionList.First();

					if (coeff is Constant { value: 1})
					{
						subtractionRoot = key;
					}
					else
					{
						subtractionRoot = new Mult(
							coeff,
							key
						);
					}
				}
				else if (subtractionList.Count == 0)
				{
					// (b + c + d + ...)
					return additionRoot;
				}
				else
				{
					while (subtractionList.Count >= 2)
					{
						(var key, var coeff) = subtractionList.Last();

						if (coeff is not Constant)
						{
							throw new Exception("coeff should be a constant!!!");
						}

						if (coeff is Constant { value: 1 })
						{
							head.operand1 = key;
						}
						else if (key is Constant)
						{
							head.operand1 = coeff;
						}
						else
						{
							head.operand1 = new Mult(
									coeff,
									key
								);
						}

						subtractionList.Remove((key, coeff));

						if (subtractionList.Count == 1)
						{
							(var key2, var coeff2) = subtractionList.Last();

							if (coeff2 is not Constant)
							{
								throw new Exception("coeff2 should be a constant!!!");
							}

							if (coeff2 is Constant { value: 1 })
							{
								head.operand2 = key2;
							}
							else if (key is Constant)
							{
								head.operand1 = coeff;
							}
							else
							{
								head.operand2 = new Mult(
										coeff2,
										key2
									);
							}

							subtractionList.Remove((key2, coeff2));
						}
						else
						{
							head.operand2 = new Add(null, null);
							head = head.operand2 as Operator;
						}
					}
				}

				return new Sub(additionRoot, subtractionRoot);
			}

			return this;
		}

		public override string ToLatexString()
		{
			return $@"{{{operand1.ToLatexString()}}} + {{{operand2.ToLatexString()}}}";
		}
	}

	public sealed class Sub : Operator
	{
		public Sub(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Sub, left, right, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();
			operand2 = operand2.Eval();

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

			return new Sub(operand1.Diff(varToDiff), operand2.Diff(varToDiff));
		}

		public override TreeNode Simplify(bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify();
				operand2 = operand2.Simplify();
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
				return new Add(operand1, wildcards['a']).Simplify(false);
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
				return new Add(operand1, wildcards['a']).Simplify(false);
			}

			return new Add(
				new Constant(0),
				this
			).Simplify(true);
		}

		public override string ToLatexString()
		{
			bool leaveRightParenthesisOut = operand2 is not Operator 
				|| operand2 is Operator { basePriority: > 1 } 
				|| operand2 is Operator { numOperands: 1};

			if (operand1 is Constant { value: 0 })
			{
				// it is just a sign

				if (leaveRightParenthesisOut)
				{
					return $@"-{{{operand2.ToLatexString()}}}";
				}

				return $@"-\left({{{operand2.ToLatexString()}}}\right)";
			}

			if (leaveRightParenthesisOut)
				return $@"{{{operand1.ToLatexString()}}} - {{{operand2.ToLatexString()}}}";
			else
				return $@"{{{operand1.ToLatexString()}}} - \left({{{operand2.ToLatexString()}}}\right)";
		}
	}

	public sealed class Mult : Operator
	{
		public Mult(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Mult, left, right, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();
			operand2 = operand2.Eval();

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
				return new Mult(operand1, operand2.Diff(varToDiff));

			if (operand2.IsConstant(varToDiff))
				return new Mult(operand2, operand1.Diff(varToDiff));

			return new Add(
				new Mult(operand1.Diff(varToDiff), operand2),
				new Mult(operand1, operand2.Diff(varToDiff))
			);
		}

		public override TreeNode Simplify(bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify();
				operand2 = operand2.Simplify();
			}

			// associative things

			Dictionary<char, TreeNode> wildcards;

			//if (TreeUtils.MatchPattern(this, new Mult(new Wildcard('a'), new Constant(1)), out wildcards))
			//	return wildcards['a'];
			//return this;

			var operands = TreeUtils.GetAssociativeOperands(this, type, inverseType);

			foreach ((var node, bool isNodeInverse) in operands)
			{
				if (node.Eval() is Constant { value: 0 })
				{
					if (isNodeInverse)
						throw new DivideByZeroException();
					else
						return new Constant(0);
				}
			}

			double constantPart = 1.0;

			if (operands.Count >= 2)
			{
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
								node.Eval(),
								new Constant(1),
								out wildcards
							))
							{
								addToDict = false;
								break;
							}

							// *0 = 0
							if (node.Eval() is Constant { value: 0 })
							{
								return new Constant(0);
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
						}
						else
						{
							// node is inverse

							// /1 ---> skip
							if (TreeUtils.MatchPattern(
								node.Eval(),
								new Constant(1),
								out wildcards
							))
							{
								addToDict = false;
								break;

							}

							// /0 = ERR
							if (TreeUtils.MatchPattern(
								node.Eval(),
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
					if (power.Eval() is Constant { value: < 0 } pow)
					{
						pow.value *= -1;

						divList.Add((key, pow));
					}
					else
					{
						multList.Add((key, power));
					}
				}

				Operator head = new Mult(null, null);

				TreeNode multRoot = head;

				if (multList.Count == 1)
				{
					(var key, var pow) = multList.First();

					if (pow is Constant { value: 1 })
					{
						multRoot = key;
					}
					else
					{
						multRoot = new Pow(
							key,
							pow
						);
					}
				}
				else if (multList.Count == 0)
				{
					multRoot = new Constant(1);
				}
				else
				{
					while (multList.Count >= 2)
					{
						(var key, var pow) = multList.Last();

						if (pow is Constant { value: 1 })
						{
							head.operand1 = key;
						}
						else
						{
							head.operand1 = new Pow(
									key,
									pow
								);
						}

						multList.Remove((key, pow));

						if (multList.Count == 1)
						{
							(var key2, var pow2) = multList.Last();

							if (pow2 is Constant { value: 1 })
							{
								head.operand2 = key2;
							}
							else
							{
								head.operand2 = new Pow(
										key2,
										pow2
									);
							}

							multList.Remove((key2, pow2));
						}
						else
						{
							head.operand2 = new Mult(null, null);
							head = head.operand2 as Operator;
						}
					}
				}

				head = new Mult(null, null);

				TreeNode divRoot = head;


				if (divList.Count == 1)
				{
					(var key, var pow) = divList.First();

					if (pow is Constant { value: 1 })
					{
						divRoot = key;
					}
					else
					{
						divRoot = new Pow(
							key,
							pow
						);
					}
				}
				else if (divList.Count == 0)
				{
					return multRoot;
				}
				else
				{
					while (divList.Count >= 2)
					{
						(var key, var pow) = divList.Last();

						if (pow is Constant { value: 1 })
						{
							head.operand1 = key;
						}
						else
						{
							head.operand1 = new Pow(
									key,
									pow
								);
						}

						divList.Remove((key, pow));

						if (divList.Count == 1)
						{
							(var key2, var pow2) = divList.Last();

							if (pow2 is Constant { value: 1 })
							{
								head.operand2 = key2;
							}
							else
							{
								head.operand2 = new Pow(
										key2,
										pow2
									);
							}

							divList.Remove((key2, pow2));
						}
						else
						{
							head.operand2 = new Mult(null, null);
							head = head.operand2 as Operator;
						}
					}
				}

				return new Div(multRoot, divRoot);
			}

			return this;
		}

		public override string ToLatexString()
		{

			bool switchOperandOrder = operand2 is Constant && operand1 is not Constant
										|| (operand1 is Operator op1 && operand2 is Operator op2 && op1.basePriority > op2.basePriority)
										|| (operand2 is Variable && operand1 is Operator);

			var leftOperand = switchOperandOrder ? operand2 : operand1;
			var rightOperand = switchOperandOrder ? operand1 : operand2;

			bool leaveLeftParenthesisOut = leftOperand is not Operator || leftOperand is Operator { basePriority: > 1 };
			bool leaveRightParenthesisOut = rightOperand is not Operator || rightOperand is Operator { basePriority: > 1 };

			bool leaveMultiplicationSignOut = (leftOperand is Constant ^ rightOperand is Constant)
											|| (leftOperand is Variable && rightOperand is Variable)
											|| rightOperand is Operator { numOperands: 1 }
											|| rightOperand is Operator { basePriority: > 2 }
											|| leftOperand is Mult 
											|| rightOperand is Mult
											|| leaveRightParenthesisOut == false;


			string leftPart = $"{{{(leaveLeftParenthesisOut ? "" : @"\left(")}" +
								$"{leftOperand.ToLatexString()}" +
								$"{(leaveLeftParenthesisOut ? "" : @"\right)")}}}";

			if (leftOperand is Constant { value: -1 })
			{
				leftPart = "-";
				leaveLeftParenthesisOut = true;
			}

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
		
		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();
			operand2 = operand2.Eval();

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
				return new Div(operand1.Diff(varToDiff), operand2);

			return new Div(
				new Sub(
					new Mult(operand1.Diff(varToDiff), operand2),
					new Mult(operand1, operand2.Diff(varToDiff))
				),
				new Pow (operand2, new Constant(2))
			);
		}

		public override TreeNode Simplify(bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify();
				operand2 = operand2.Simplify();
			}

			if (operand1.Eval() is Constant { value: 0 })
				return new Constant(0);

			return new Mult(
				new Constant(1),
				this
			).Simplify(true);
		}

		public override string ToLatexString()
		{
			return $@"\frac{{{operand1.ToLatexString()}}}{{{operand2.ToLatexString()}}}";
		}
	}

	public sealed class Pow : Operator
	{
		public Pow(TreeNode? left = null, TreeNode? right = null, int? priority = null) : base(OperatorType.Pow, left, right, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();
			operand2 = operand2.Eval();

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

		public override TreeNode Simplify(bool skipSimplificationOfChildren = false)
		{
			if (skipSimplificationOfChildren == false)
			{
				operand1 = operand1.Simplify();
				operand2 = operand2.Simplify();
			}

			if (operand2.Eval() is Constant { value: 1 })
				return operand1;

			if (operand2.Eval() is Constant { value: 0 })
				return new Constant(1);

			if (operand1.Eval() is Constant { value: 1 })
				return new Constant(1);

			if (operand1.Eval() is Constant { value: 0 })
				return new Constant(0);

			Dictionary<char, TreeNode> wildcards;


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


			return this;
		}

		public override string ToLatexString()
		{
			bool leaveBaseParenthesisOut = operand1 is not Operator
										|| operand1 is Operator { numOperands: 1 };
			bool leavePowerParenthesisOut = operand2 is not Pow;

			return $"{(leaveBaseParenthesisOut ? "" : @"\left(")}" +
				$"{{{operand1.ToLatexString()}}}" +
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

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();

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

			return new Mult(
				new Cos(operand1),
				operand1.Diff(varToDiff)
			);
		}

		public override string ToLatexString()
		{
			return $@"\sin\left({{{operand1.ToLatexString()}}}\right)";
		}
	}
	
	public sealed class Cos : Operator
	{
		public Cos(TreeNode? operand = null, int? priority = null) : base(OperatorType.Cos, operand, null, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();

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
	}

	public sealed class Tan : Operator
	{
		public Tan(TreeNode? operand = null, int? priority = null) : base(OperatorType.Tan, operand, null, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();

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
	}

	public sealed class Ln : Operator
	{
		public Ln(TreeNode? operand = null, int? priority = null) : base(OperatorType.Ln, operand, null, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();

			if (operand1 is Constant c)
				return new Constant(Math.Log(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

			return new Div(
				operand1.Diff(varToDiff),
				operand1
			);
		}

		public override string ToLatexString()
		{
			return $@"\ln\left({{{operand1.ToLatexString()}}}\right)";
		}
	}

	public sealed class Log : Operator
	{
		public Log(TreeNode? operand = null, int? priority = null) : base(OperatorType.Log, operand, null, priority) { }

		public override TreeNode Eval()
		{
			operand1 = operand1.Eval();

			if (operand1 is Constant c)
				return new Constant(Math.Log10(c.value));
			else
				return this;
		}

		public override TreeNode Diff(char varToDiff)
		{
			if (Differentiator.numStapsTaken++ >= Differentiator.maxSteps)
			{
				return new DerivativeSymbol(this, varToDiff);
			}

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
			return $@"\log\left({{{operand1.ToLatexString()}}}\right)";
		}
	}
}