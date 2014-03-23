// -----------------------------------------------------------------------
// <copyright file="DBATest.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using NUnit.Framework;
using QUSMAML;

namespace QUSMAMLTest
{
    [TestFixture]
    public class DBATest
    {
        [Test]
        public void Blah()
        {
            List<double[]> series = new List<double[]>();
            
            DBA.Average(series);
        }
    }
}
