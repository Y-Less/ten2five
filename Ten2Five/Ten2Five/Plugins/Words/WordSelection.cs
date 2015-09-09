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
	// To do this, we assume that words can be allocated by random chance - that
	// is, every word has an equal chance of being right or wrong (this is
	// clearly preposterous in reality).  Given that, what are the chances of a
	// result being given.
	// 
	// The basic algorithm is:
	// 
	//   TOTAL = Sum all word accuracies.
	//   Generate a random number between 0.0 and TOTAL.
	//   Loop through words.
	//   Add current accuracy, until the total > random.
	//   Return that word.
	// 
	// Highly accurate words (those with a very low percentage of wrong answers)
	public class WordSelection
	{

	}
}

