

namespace DerivativeCalculator
{
	public partial record SimplificationParams(char? varToDiff = null, List<OperatorType>? opsNotToEval = null)
	{
		public static readonly SimplificationParams Default = new(null, new () { OperatorType.Ln });

		public static SimplificationParams UpdateDefault(SimplificationParams simplificationParams)
		{
			return new SimplificationParams(
				simplificationParams.varToDiff ?? Default.varToDiff,	
				simplificationParams.opsNotToEval ?? Default.opsNotToEval
			);
		}
	}
}
