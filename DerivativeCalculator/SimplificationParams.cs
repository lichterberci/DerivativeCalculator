

namespace DerivativeCalculator
{
	public sealed record SimplificationParams(char? varToDiff = null, List<OperatorType>? opsNotToSimplify = null);
}
