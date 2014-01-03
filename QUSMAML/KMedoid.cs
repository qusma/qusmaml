// Example usage:
// Our data consists of points in 2D space:
//
//  var points = new List<Point>
//  {
//      new Point(5, 2),
//      new Point(6, 3),
//      new Point(5, 2),
//      new Point(8, 2),
//      new Point(6, 2),
//      new Point(7, 4),
//      new Point(25, 30),
//      new Point(26, 33),
//      new Point(24, 28),
//      new Point(30, 29),
//      new Point(32, 32),
//      new Point(5, 20),
//      new Point(20, 5),
//  };
//
//
// Create a distance function:
//
//  Func<Point, Point, double> euclidean =
//      (a, b) => Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
//
//
// Create the KMedoid object and call Compute(), using two clusters. The generic type is the type of inputs:
//
//  var km = new KMedoid<Point>(euclidean);
//  km.Compute(2, points);
//
//
// The resulting cluster labels can be found in the Clusters property:
//
//	[0]	0
//  [1]	0
//  [2]	0
//  [3]	0
//  [4]	0
//  [5]	0
//  [6]	1
//  [7]	1
//  [8]	1
//  [9]	1
//  [10]1
//  [11]0
//  [12]0

using System;
using System.Collections.Generic;
using System.Linq;

namespace QUSMAML
{
    public class KMedoid<T>
    {
        private readonly Func<T, T, double> _distanceFunc;
        private double[,] _distances;
        private int[] _medoids;

        /// <summary>
        /// After calling Compute(), this List contains an integer for each input element, representing which cluster it belongs to.
        /// </summary>
        public List<int> Clusters { get; private set; }

        public KMedoid(Func<T, T, double> distanceFunc)
        {
            _distanceFunc = distanceFunc;
        }

        /// <summary>
        /// Assign cluster labels to the provided inputs.
        /// </summary>
        /// <param name="k">The number of clusters.</param>
        /// <param name="inputs">The inputs.</param>
        public void Compute(int k, List<T> inputs)
        {
            Clusters = new List<int>();

            //calculate all the distances
            _distances = new double[inputs.Count, inputs.Count];

            for (int i = 0; i < inputs.Count - 1; i++)
            {
                for (int j = i + 1; j < inputs.Count; j++)
                {
                    _distances[i, j] = _distanceFunc(inputs[i], inputs[j]);
                    _distances[j, i] = _distances[i, j];
                }
            }

            _medoids = Enumerable.Range(1, k).Select(x => -1).ToArray();

            //initialization
            InitializeMedoids(inputs, k);

            //loop until no further improvement is made
            bool changed = true;
            double lastTotalCost = TotalCost(_medoids, ref inputs);
            while (changed)
            {
                //for each medoid, we try replacing it with a nonmedoid
                //if the total score improves, we keep the change

                _medoids = GetNewMedoids(k, ref inputs, ref lastTotalCost, out changed);
            }

            //finally get the clusters from the current medoids
            UpdateClusters(inputs);
        }

        /// <summary>
        /// A deterministic approach to initialization.
        /// Follows Park et al (http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.90.7981&rep=rep1&type=pdf) for the first medoid.
        /// Then picks medoids on the basis of maximum differene from the already existing ones.
        /// </summary>
        private void InitializeMedoids(List<T> inputs, int k)
        {
            double[] distanceSums = new double[inputs.Count];
            double[,] p = new double[inputs.Count, inputs.Count];
            double[] pSum = new double[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
            {
                for (int j = 0; j < inputs.Count; j++)
                {
                    distanceSums[i] += _distances[i, j];
                }
            }

            for (int i = 0; i < inputs.Count; i++)
            {
                for (int j = 0; j < inputs.Count; j++)
                {
                    p[i, j] += _distances[i, j] / distanceSums[i];
                }
            }

            for (int i = 0; i < inputs.Count; i++)
            {
                for (int j = 0; j < inputs.Count; j++)
                {
                    pSum[i] += p[j, i];
                }
            }

            _medoids[0] = Array.IndexOf(pSum, pSum.Min());

            //we have the first medoid, now simply select new ones on the basis of maximum distance from the existing ones
            for (int i = 1; i < k; i++)
            {
                double[] medoidDistanceSums = new double[inputs.Count];
                for (int j = 0; j < i; j++)
                {
                    for (int l = 0; l < inputs.Count; l++)
                    {
                        medoidDistanceSums[l] += _distances[l, _medoids[j]];
                    }
                }
                _medoids[i] = Array.IndexOf(medoidDistanceSums, medoidDistanceSums.Max());
            }
        }

        /// <summary>
        /// Sets the cluster label for each input, depending on which medoid it is closest to.
        /// </summary>
        private void UpdateClusters(List<T> inputs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var medoidDistances = _medoids.Select(x => _distances[x, i]).ToList();
                int minIndex = 0;
                double minDistance = double.MaxValue;
                for (int j = 0; j < medoidDistances.Count; j++)
                {
                    if (medoidDistances[j] < minDistance)
                    {
                        minDistance = medoidDistances[j];
                        minIndex = j;
                    }
                }
                Clusters.Add(minIndex);
            }
        }

        /// <summary>
        /// Creates a new set of medoids by trying to replace current medoids with non-medoid points.
        /// </summary>
        private int[] GetNewMedoids(int k, ref List<T> inputs, ref double lastTotalCost, out bool changed)
        {
            var nonMedoids = Enumerable.Range(0, inputs.Count).Where(x => !_medoids.Contains(x)).ToList();
            var newMedoids = new int[k];
            for (int i = 0; i < _medoids.Length; i++)
            {
                newMedoids[i] = _medoids[i];
            }

            for (int i = 0; i < k; i++)
            {
                foreach (int t in nonMedoids)
                {
                    newMedoids[i] = t;
                    var cost = TotalCost(newMedoids, ref inputs);
                    if (cost < lastTotalCost)
                    {
                        changed = true;
                        lastTotalCost = cost;
                        return newMedoids;
                    }
                }
                newMedoids[i] = _medoids[i];
            }

            changed = false;
            return _medoids;
        }

        /// <summary>
        /// Calculates the total cost given a set of points and medoids.
        /// Total cost is the sum of the minimum distances between each point and a medoid.
        /// </summary>
        private double TotalCost(int[] medoids, ref List<T> inputs)
        {
            double totalCost = 0;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (!medoids.Contains(i))
                {
                    totalCost += medoids.Select(x => _distances[x, i]).Min();
                }
            }

            return totalCost;
        }
    }
}