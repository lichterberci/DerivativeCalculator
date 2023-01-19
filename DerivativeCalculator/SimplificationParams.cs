

namespace DerivativeCalculator
{
	public partial record SimplificationParams(char? varToDiff = null, List<OperatorType>? opsNotToEval = null)
	{
		public static readonly SimplificationParams Default = new(null, new () { OperatorType.Ln });
	}
}
