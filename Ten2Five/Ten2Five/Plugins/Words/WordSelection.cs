using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ten2Five.Plugins
{
	// From a list of words, each with "show" counts and "right/wrong" counts,
	// decide which words are more likely to be shown next, leaning towards the
	// words got wrong more frequently, and those not shown very much.
	// 
	// This is done by getting their accuracy, flipping it so that the least
	// accurate words have the highest values, and then iterating through all
	// the words to select one based on this adjusted likelihood.  Because we
	// adjust twice, the shift to >= 0 actually cancel out and we don't need to
	// record the lowest value at all!
	// 
	//   x: accuracy
	//   l: lowest x
	//   u: highest x
	//   
	//   s = x - l (shifted to >= 0)
	//   u' = u - l
	//   x' = x - l
	//   f = u' - x' + 1 (likelihood >= 1, lower x = higher f)
	//     = (u - l) - (x - l) + 1
	//     = u - l - x + l + 1
	//     = u - x + 1
	//     = u + 1 - x
	//   
	//   t: total
	//   
	//   t = sum_{i=0}^n(f_i)
	//     = sum_{i=0}^n(u + 1 - x_i)
	//     = n(u + 1) - sum_{i=0}^n(x_i)
	// 
	public static class WordSelection
	{
		private static Random rand_ = new Random();

		private static int GetAccuracy(WordMap w)
		{
			// "accuracy = right - wrong", "right = total - wrong".  An accuracy
			// > 0 means more right than wrong, < 0 means more wrong than right,
			// == 0 means exactly even.
			return w.Shown - w.Wrong - w.Wrong;
		}

		private static int GetTotal(IEnumerable<WordMap> words, out int upper)
		{
			// initialise the values to be as extreme as possible.
			upper = int.MinValue;
			// Loop through all the words, and get the total for all the
			// accuracies.  These are shifted so that the lowest accuracy is
			// always 1.  This adds on 1, so that even the most accurate numbers
			// will appear occasionally (and is the reason why the lowest
			// accuracy is 1 not 0).  Later on, the values are inverted so that
			// the lowest ones become more likely.
			int total = 0;
			int count = 0;
			foreach (WordMap w in words)
			{
				int a = GetAccuracy(w);
				if (a > upper)
					upper = a;
				total += a;
				++count;
			}
			// Invert all the values.
			return count * (upper + 1) - total;
		}

		private static int GetLikelihood(WordMap w, int upper)
		{
			return upper - GetAccuracy(w) + 1;
		}

		public static WordMap Select(IEnumerable<WordMap> words)
		{
			int upper;
			int total = GetTotal(words, out upper);
			int running = 0;
			int target = rand_.Next(total);
			foreach (WordMap w in words)
			{
				running += GetLikelihood(w, upper);
				if (running > target)
					return w;
			}
			return null;
		}
	}
}

