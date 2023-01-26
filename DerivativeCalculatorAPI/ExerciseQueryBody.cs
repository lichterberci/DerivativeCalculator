using DerivativeCalculator;

namespace DerivativeCalculatorAPI
{
	public sealed class ExerciseQueryBody
	{
		public string? level { get; set; }
		public DifficultyMetrics? difficultyMetrics { get; set; }
		public Preferences? preferences { get; set; }
	}
}
