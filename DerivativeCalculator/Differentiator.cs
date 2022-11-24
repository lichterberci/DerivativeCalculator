using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeCalculator
{
	public static class Differentiator {
		public static List<string> steps { get; private set; }

		private static int numStapsTaken = 0;
		private static int maxSteps = 0;

		static Differentiator()
		{
			steps = new List<string>();
		}

		public static TreeNode Differentiate (TreeNode root, char varToDiff)
		{
			return root.Diff(varToDiff);
		}

		public static TreeNode DifferentiateWithStepsRecorded (TreeNode root, char varToDiff)
		{
			TreeNode diffTree = root;
			TreeNode prevTree = null;
			TreeNode prettyTree = null;

			maxSteps = 0;

			// initial step
			steps.Add(
				TreeUtils.CollapseTreeToString(
					TreeUtils.SimplifyWithPatterns(TreeUtils.Calculate(TreeUtils.SimplifyWithPatterns(new DerivativeSymbol(root, varToDiff))))
				)
			);

			while (TreeUtils.AreTreesEqual(prevTree, diffTree) == false)
			{
				numStapsTaken = 0;
				maxSteps++;

				prevTree = TreeUtils.CopyTree(diffTree);

				diffTree = root.Diff(varToDiff);

				prettyTree = TreeUtils.SimplifyWithPatterns(diffTree);
				prettyTree = TreeUtils.Calculate(prettyTree);
				prettyTree = TreeUtils.SimplifyWithPatterns(prettyTree);
				steps.Add(TreeUtils.CollapseTreeToString(prettyTree));
			}

			// last 2 steps are the same
			if (steps.Count >= 2)
				steps.RemoveAt(steps.Count - 1);

			return diffTree;
		}
	}
}
