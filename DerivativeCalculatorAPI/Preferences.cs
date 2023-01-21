
using DerivativeCalculator;

namespace DerivativeCalculatorAPI
{
	public sealed class Preferences
	{
		public SimplificationPreferences simplificationPreferences { get; set; }

		public static readonly Preferences Default = new Preferences() { 
			simplificationPreferences = SimplificationPreferences.Default
		};
	}

	public sealed class SimplificationPreferences
	{
		public List<string> opsNotToEvaluate { get; set; }

		public List<OperatorType> GetOpsNotToEval ()
		{
			return opsNotToEvaluate
				.Select(opString => Operator.ParseFromString(opString))
				.Where(type => type is not null)
				.Cast<OperatorType>()
				.ToList();
		}

		public static readonly SimplificationPreferences Default = new SimplificationPreferences() { 
			opsNotToEvaluate = new List<string> { "ln", "log" }
		};
	}
}
