// -----------------------------------------------------------------------
// <copyright file="MdsTest.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using ILNumerics;
using NUnit.Framework;
using QUSMAML;

namespace QUSMAMLTest
{
    [TestFixture]
    public class MdsTest
    {
        [Test]
        public void MdsReturnsCorrectResult()
        {
            ILArray<double> dist = ILMath.zeros(3, 3);
            dist[1, 0] = 4.94760097137838;
            dist[2, 0] = 3.48822665076897;

            dist[0, 1] = 4.94760097137838;
            dist[2, 1] = 5.07828347170446;

            dist[0, 2] = 3.48822665076897;
            dist[1, 2] = 5.07828347170446;

            MultidimensionalScaling.Scale(dist);
            Assert.IsTrue(false);
        }
    }
}
