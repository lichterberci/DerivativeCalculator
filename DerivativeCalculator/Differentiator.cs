using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public static class Differentiator {
		public static List<string> steps { get; private set; }
		public static List<StepDescription?> stepDescriptions { get; private set; }

		public static int numStapsTaken = 0;
		public static int maxSteps = int.MaxValue;

		static Differentiator()
		{
			steps = new List<string>();
			stepDescriptions = new List<StepDescription?>();
		}

		public static TreeNode Differentiate (TreeNode root, char varToDiff)
		{
			return root.Diff(varToDiff);
		}

		public static void AddStepDescription (StepDescription? stepDesc)
		{
			stepDescriptions.Add(stepDesc);
		}

		public static TreeNode DifferentiateWithStepsRecorded (TreeNode root, char varToDiff)
		{
			TreeNode diffTree = root;

			maxSteps = 0;

			steps = new List<string>();
			stepDescriptions = new List<StepDescription?>();

			// initial step
			steps.Add(
				new DerivativeSymbol(diffTree, varToDiff).ToLatexString()
			);

			string prevStepString = "";

			while (true)
			{
				numStapsTaken = 0;
				maxSteps++;

				diffTree = TreeUtils.CopyTree(root).Diff(varToDiff);

				diffTree = TreeUtils.GetSimplestForm(diffTree);

				if (prevStepString == diffTree.ToLatexString())
					break;

				prevStepString = diffTree.ToLatexString();

				steps.Add(prevStepString);
				stepDescriptions = new List<StepDescription?>() { null };
			}

			maxSteps = int.MaxValue;

			return diffTree;
		}
	}
}
