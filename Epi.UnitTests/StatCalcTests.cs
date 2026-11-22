using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using EpiDashboard;
using Epi.Core;
using StatisticsRepository;

namespace Epi.UnitTests
{
    [TestClass]
    public class StatCalcTests : StatisticsTests
    {
        [TestMethod]
        public void BinomialTest()
        {
            var fixturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BinomialTests.csv");
            var calculator = new StatisticsRepository.Strat2x2();
            int row = 0;

            foreach (var line in File.ReadLines(fixturePath))
            {
                var values = line.TrimEnd(',').Split(',');
                Assert.AreEqual(11, values.Length, $"Unexpected column count in binomial fixture row {row + 1}.");

                int x = int.Parse(values[0], CultureInfo.InvariantCulture);
                int n = int.Parse(values[1], CultureInfo.InvariantCulture);
                double p = double.Parse(values[2], CultureInfo.InvariantCulture);
                double expectedLtp = double.Parse(values[3], CultureInfo.InvariantCulture);
                double expectedLep = double.Parse(values[4], CultureInfo.InvariantCulture);
                double expectedEqp = double.Parse(values[5], CultureInfo.InvariantCulture);
                double expectedGep = double.Parse(values[6], CultureInfo.InvariantCulture);
                double expectedGtp = double.Parse(values[7], CultureInfo.InvariantCulture);
                double expectedPtt = double.Parse(values[8], CultureInfo.InvariantCulture);
                int expectedLcl = int.Parse(values[9], CultureInfo.InvariantCulture);
                int expectedUcl = int.Parse(values[10], CultureInfo.InvariantCulture);

                double ltp = 0;
                double lep = 0;
                double eqp = 0;
                double gep = 0;
                double gtp = 0;
                double ptt = 0;
                int lcl = 0;
                int ucl = 0;
                calculator.binpdf(n, x, p, ref ltp, ref lep, ref eqp, ref gep, ref gtp, ref ptt, ref lcl, ref ucl);

                string context = $"Binomial fixture row {row + 1}.";
                Assert.AreEqual(expectedLtp, ltp, 0.000001, context + " Lower-tail probability.");
                Assert.AreEqual(expectedLep, lep, 0.000001, context + " Lower inclusive probability.");
                Assert.AreEqual(expectedEqp, eqp, 0.000001, context + " Exact probability.");
                Assert.AreEqual(expectedGep, gep, 0.000001, context + " Upper inclusive probability.");
                Assert.AreEqual(expectedGtp, gtp, 0.000001, context + " Upper-tail probability.");
                Assert.AreEqual(expectedPtt, ptt, 0.000001, context + " Two-tailed probability.");
                Assert.AreEqual(expectedLcl, lcl, context + " Lower confidence limit.");
                Assert.IsTrue(Math.Abs(expectedUcl - ucl) <= 1, context + " Upper confidence limit.");
                row++;
            }

            Assert.AreEqual(5000, row, "Unexpected number of binomial fixture rows.");
        }
    }
}
