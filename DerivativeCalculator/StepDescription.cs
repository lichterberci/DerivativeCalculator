using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public sealed class StepDescription
	{
		public string ruleNameAsLatex { get; } // eg.: power rule f(x)^g(x) = e^(g(x)*ln(f(x)) --> d/dx(f(x)^g(x)) = f(x)^g(x)*d/dx(g(x)*ln(f(x))
		public string fxAsLatex { get; }
		public string? gxAsLatex { get; }

		public StepDescription(string ruleNameAsLatex, string fxAsLatex, string? gxAsLatex = null)
		{
			this.ruleNameAsLatex = ruleNameAsLatex;
			this.fxAsLatex = fxAsLatex;
			this.gxAsLatex = gxAsLatex;
		}
	}
}
