using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public class ParsingError : Exception
	{
		public ParsingError() { }
		public ParsingError(string message) : base(message) { }
		public ParsingError(string message, Exception inner) : base(message, inner) { }
	}

	public class DifferentiationException : Exception
	{
		public DifferentiationException() { }
		public DifferentiationException(string message) : base(message) { }
		public DifferentiationException(string message, Exception inner) : base(message, inner) { }
	}

	public class SimplificationException : Exception
	{
		public SimplificationException() { }
		public SimplificationException(string message) : base(message) { }
		public SimplificationException(string message, Exception inner) : base(message, inner) { }
	}

	public class ExerciseCouldNotBeGeneratedException : Exception
	{
		public ExerciseCouldNotBeGeneratedException() { }
		public ExerciseCouldNotBeGeneratedException(string message) : base(message) { }
		public ExerciseCouldNotBeGeneratedException(string message, Exception inner) : base(message, inner) { }
	}
}
