// -----------------------------------------------------------------------
// <copyright file="DBA.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
//DTW Barycenter Averaging
using NDtw;

namespace QUSMAML
{
    public class DBA
    {
        private readonly Func<double[], double[], double> _distanceFunc;

        public DBA(Func<double[], double[], double> distanceFunc)
        {
            _distanceFunc = distanceFunc;
        }

        public double[] Average(double[] average, List<double[]> series)
        {

            

            //get the path between each series and the average
            for (int i = 0; i < series.Count; i++)
            {
                var dtw = new Dtw(new[] { new SeriesVariable(average, series[i]) });
                var path = dtw.GetPath();
            }

            return average;
        }


    }
}
