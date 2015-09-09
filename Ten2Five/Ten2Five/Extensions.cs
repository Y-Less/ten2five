/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Ten2Five
{
	public static class Extensions
	{
		public class ToComparer<T> : IComparer<T> where T : IComparable
		{
			public int Compare(T x, T y)
			{
				return x.CompareTo(y);
			}
		}

		private static T ID<T>(T x)
		{
			return x;
		}

		public static int GetInsertionIndex<T>(this Collection<T> c, T elem) where T : IComparable
		{
			return GetInsertionIndex(c, elem, ID, new ToComparer<T>());
		}

		public static int GetInsertionIndex<T>(this Collection<T> c, T elem, IComparer<T> comparison)
		{
			return GetInsertionIndex(c, elem, ID, comparison);
		}

		public static int GetInsertionIndex<T, U>(this Collection<T> c, T elem, Func<T, U> accessor) where U : IComparable
		{
			return GetInsertionIndex(c, elem, accessor, new ToComparer<U>());
		}

		public static int GetInsertionIndex<T, U>(this Collection<T> c, T elem, Func<T, U> accessor, IComparer<U> comparison)
		{
			// Use a binary serach to find where in this 
			U value = accessor(elem);
			int lower = 0;
			int upper = c.Count;
			// Perform a binary search to find a point BETWEEN two elements.
			while (lower != upper)
			{
				int idx = (upper - lower) / 2 + lower;
				int diff = comparison.Compare(value, accessor(c[idx]));
				if (diff > 0)
					lower = idx + 1;
				else if (diff < 0)
					upper = idx;
				else
					return -1;
			}
			return lower;
		}

		public static int InsertSorted<T>(this Collection<T> c, T elem) where T : IComparable
		{
			int idx = GetInsertionIndex(c, elem);
			if (idx == -1)
				return -1;
			c.Insert(idx, elem);
			return idx;
		}

		public static int InsertSorted<T>(this Collection<T> c, T elem, IComparer<T> comparison)
		{
			int idx = GetInsertionIndex(c, elem, comparison);
			if (idx == -1)
				return -1;
			c.Insert(idx, elem);
			return idx;
		}

		public static int InsertSorted<T, U>(this Collection<T> c, T elem, Func<T, U> accessor) where U : IComparable
		{
			int idx = GetInsertionIndex(c, elem, accessor);
			if (idx == -1)
				return -1;
			c.Insert(idx, elem);
			return idx;
		}

		public static int InsertSorted<T, U>(this Collection<T> c, T elem, Func<T, U> accessor, IComparer<U> comparison)
		{
			int idx = GetInsertionIndex(c, elem, accessor, comparison);
			if (idx == -1)
				return -1;
			c.Insert(idx, elem);
			return idx;
		}

		public static decimal NextDecimal(this Random rnd, decimal from, decimal to)
		{
			byte fromScale = new System.Data.SqlTypes.SqlDecimal(from).Scale;
			byte toScale = new System.Data.SqlTypes.SqlDecimal(to).Scale;

			byte scale = (byte)(fromScale + toScale);
			if (scale > 28)
				scale = 28;

			decimal r = new decimal(rnd.Next(), rnd.Next(), rnd.Next(), false, scale);
			if (Math.Sign(from) == Math.Sign(to) || from == 0 || to == 0)
				return decimal.Remainder(r, to - from) + from;

			bool getFromNegativeRange = (double)from + rnd.NextDouble() * ((double)to - (double)from) < 0;
			return getFromNegativeRange ? decimal.Remainder(r, -from) + from : decimal.Remainder(r, to);
		}

		public static float NextFloat(this Random rnd, float from, float to)
		{
			return (float)(rnd.NextDouble() * ((double)to - (double)from) + (double)from);
		}

		public static double NextDouble(this Random rnd, double from, double to)
		{
			return rnd.NextDouble() * (to - from) + from;
		}

		public static IEnumerable<T> Yield<T>(this T item)
		{
			yield return item;
		}

		public static IList<T> ToIList<T>(this System.Collections.IList iList)
		{
			IList<T> result = new List<T>();
			if (iList != null)
			{
				foreach (T value in iList)
				{
					result.Add(value);
				}
			}
			return result;
		}
	}
}
