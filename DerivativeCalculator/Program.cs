public partial class Program
{
	public static void Main ()
	{
		Console.WriteLine("Hello, World!");
	}
}

public abstract class Node
{

}

public sealed class Constant : Node
{
	double value;
}

public sealed class Variable : Node
{
	char name;
}

enum OperatorType
{
	Add, Sub, Mult, Div, Pow, Sin, Cos, Tan, Log
}

public sealed class Operator : Node
{
	OperatorType type;
}