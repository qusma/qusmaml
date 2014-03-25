// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace QUSMAML
{
    public static class Extensions
    {
        public static int IndexOfMax(this IList<double> input)
        {
            if (input == null) throw new ArgumentNullException("input");

            int maxIndex = -1;
            double maxValue = input[0];

            for (int i = 0; i < input.Count; i++)
            {
                if (input[i] >= maxValue)
                {
                    maxIndex = i;
                    maxValue = input[i];
                }
            }

            return maxIndex;
        }

        public static int IndexOfMin(this IList<double> input)
        {
            if (input == null) throw new ArgumentNullException("input");

            int minIndex = -1;
            double minValue = input[0];

            for (int i = 0; i < input.Count; i++)
            {
                if (input[i] <= minValue)
                {
                    minIndex = i;
                    minValue = input[i];
                }
            }

            return minIndex;
        }

        public static double Median(this IEnumerable<int> input)
        {
            if (input == null) throw new ArgumentNullException("input");

            int midIndex = input.Count() / 2;
            var sorted = input.OrderBy(x => x).ToList();

            double median = 
                input.Count() % 2 == 0
                    ? (sorted[midIndex] + sorted[midIndex - 1]) / 2
                    : sorted[midIndex];

            return median;
        }
    }
}
