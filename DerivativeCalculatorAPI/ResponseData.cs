using DerivativeCalculator;

namespace DerivativeCalculatorAPI
{
	public class ResponseData
	{
		public string inputAsLatex { get; set; }
		public string simplifiedInputAsLatex { get; set; }
		public string outputAsLatex { get; set; }
		public List<string> stepsAsLatex { get; set; }
		public List<StepDescription?> stepDescriptions { get; set; }
		public char varToDiff { get; set; }

		public string? errorType { get; set; }
		public string? errorMessage { get; set; }

		public ResponseData() { }
		public ResponseData (string? errorType, string? errorMessage)
		{
			this.errorType = errorType;
			this.errorMessage = errorMessage;
		}
		public ResponseData(string inputAsLatex, string simplifiedInputAsLatex, string outputAsLatex, List<string> stepsAsLatex, List<StepDescription> stepDescriptions, char varToDiff)
		{
			this.inputAsLatex = inputAsLatex;
			this.simplifiedInputAsLatex = simplifiedInputAsLatex;
			this.outputAsLatex = outputAsLatex;
			this.stepsAsLatex = stepsAsLatex;
			this.stepDescriptions = stepDescriptions;
			this.varToDiff = varToDiff;
		}
	}
}
