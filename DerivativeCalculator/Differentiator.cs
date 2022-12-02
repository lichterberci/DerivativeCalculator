﻿using System;
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

			maxSteps = 0;

			// initial step
			steps.Add(
				diffTree.ToLatexString()
			);

			string prevStepString = "";

			do
			{
				numStapsTaken = 0;
				maxSteps++;

				diffTree = TreeUtils.CopyTree(root).Diff(varToDiff);

				diffTree = TreeUtils.GetSimplestForm(diffTree);

				prevStepString = diffTree.ToLatexString();

				steps.Add(prevStepString);
			}
			while (prevStepString != diffTree.ToLatexString());

			// last 2 steps are the same
			if (steps.Count >= 2)
				steps.RemoveAt(steps.Count - 1);

			return diffTree;
		}
	}
}
