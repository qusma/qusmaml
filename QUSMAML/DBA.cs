// -----------------------------------------------------------------------
// <copyright file="DBA.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------
//
//Usage: simply call Average() with two or more series of equal length.
//
//This is an implementation of the algorithm presented in:
//A global averaging method for dynamic time warping, with applications to clustering, Petitjean et. al.
//(http://dpt-info.u-strasbg.fr/~fpetitjean/Research/Petitjean2011-PR.pdf)

using System;
using System.Collections.Generic;
using System.Linq;
using NDtw;

namespace QUSMAML
{
    /// <summary>
    /// DTW Barycenter Averaging
    /// </summary>
    public class DBA
    {
        /// <summary>
        /// Generate average of supplied series.
        /// </summary>
        public static double[] Average(List<double[]> series, int maxIterations = 100)
        {
            if (series == null || series.Count == 0) throw new Exception("series null or empty");
            if (series.Count == 1) return series[0];
            if (series.Select(x => x.Length).Distinct().Count() != 1) throw new Exception("Series must be of equal length");

            int length = series[0].Length;

            //initializing the average to a simple mean of every point, without DTW
            //double[] average = new double[length];
            //for (int i = 0; i < series[0].Length; i++)
            //{
            //    average[i] = series.Average(x => x[i]);
            //}

            //initialize to a random series from the input
            //var r = new Random();
            //double[] average = series[r.Next(0, series.Count - 1)];

            //initialize to series closest to median min/max after detrending
            List<double[]> tempSeries = series.Select(Detrend).ToList();
            List<int> maxIndexes = tempSeries.Select(x => x.IndexOfMax()).ToList();
            List<int> minIndexes = tempSeries.Select(x => x.IndexOfMin()).ToList();
            double medianMaxIndex = maxIndexes.Median();
            double medianMinIndex = minIndexes.Median();
            var distances = maxIndexes.Select((x, i) => Math.Pow(x - medianMaxIndex, 2) + Math.Pow(minIndexes[i] - medianMinIndex, 2)).ToList();
            int selectedSeries = distances.IndexOfMin();
            double[] average = series[selectedSeries];

            //this list will hold the values of each aligned point, 
            //later used to construct the aligned average
            List<double>[] points = new List<double>[length];
            for (int i = 0; i < length; i++)
            {
                points[i] = new List<double>();
            }

            double prevTotalDist = -1;
            double totalDist = -2;

            //sometimes the process gets "stuck" in a loop between two different states
            //so we have to set a hard limit to end the loop
            int count = 0; 

            //get the path between each series and the average
            while (totalDist != prevTotalDist && count < maxIterations)
            {
                prevTotalDist = totalDist;

                //clear the points from the last calculation
                foreach (var list in points)
                {
                    list.Clear();
                }

                //here we do the alignment for every series
                foreach (double[] ts in series)
                {
                    var dtw = new Dtw(new[] { new SeriesVariable(ts, average) });
                    Tuple<int, int>[] path = dtw.GetPath();

                    //use the path to distribute the points according to the warping
                    Array.ForEach(path, x => points[x.Item2].Add(ts[x.Item1]));
                }

                //Then simply construct the new average series by taking the mean of every List in points.
                average = points.Select(x => x.Average()).ToArray();

                //calculate Euclidean distance to stop the loop if no further improvement can be made
                double[] average1 = average;
                totalDist = series.Sum(x => x.Select((y, i) => Math.Pow(y - average1[i], 2)).Sum()); //we get convergence even though there's still work to be done
                count++;
            }

            return average;
        }

        private static double[] Detrend(double[] input)
        {
            int len = input.Length;
            double step = (input[len - 1] - input[0]) / len;
            var output = new double[len];
            for (int i = 0; i < len; i++)
            {
                output[i] = input[i] - step * i;
            }
            return output;
        }
    }
}