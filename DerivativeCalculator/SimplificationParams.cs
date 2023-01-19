

namespace DerivativeCalculator
{
	public partial record SimplificationParams(char? varToDiff = null, List<OperatorType>? opsNotToSimplify = null)
	{
		public static readonly SimplificationParams Default = new(null, new () { OperatorType.Ln });
	}
}
