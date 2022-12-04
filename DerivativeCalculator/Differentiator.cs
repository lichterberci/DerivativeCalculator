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

		public static int numStapsTaken = 0;
		public static int maxSteps = int.MaxValue;

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

			steps = new List<string>();

			// initial step
			steps.Add(
				diffTree.ToLatexString()
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
			}

			maxSteps = int.MaxValue;

			return diffTree;
		}
	}
}
