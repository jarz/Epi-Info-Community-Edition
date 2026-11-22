using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using EpiDashboard;
using Epi.Core;
using StatisticsRepository;
using System.Collections.Generic;
using System.Data;

namespace Epi.UnitTests
{
    [TestClass]
    public class AnalysisTest : StatisticsTests
    {
        [TestMethod]
        public void MeansTestTwoValues()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\MeansTwoValueResults.csv"));
            int linesinfile = iterations;
            double[] obs0s = new double[linesinfile];
            double[] obs1s = new double[linesinfile];
            double[] total0s = new double[linesinfile];
            double[] total1s = new double[linesinfile];
            double[] mean0s = new double[linesinfile];
            double[] mean1s = new double[linesinfile];
            double[] var0s = new double[linesinfile];
            double[] var1s = new double[linesinfile];
            double[] sd0s = new double[linesinfile];
            double[] sd1s = new double[linesinfile];
            double[] min0s = new double[linesinfile];
            double[] min1s = new double[linesinfile];
            double[] q10s = new double[linesinfile];
            double[] q11s = new double[linesinfile];
            double[] median0s = new double[linesinfile];
            double[] median1s = new double[linesinfile];
            double[] q30s = new double[linesinfile];
            double[] q31s = new double[linesinfile];
            double[] max0s = new double[linesinfile];
            double[] max1s = new double[linesinfile];
            double[] mode0s = new double[linesinfile];
            double[] mode1s = new double[linesinfile];
            double[] poolmeans = new double[linesinfile];
            double[] poollcls = new double[linesinfile];
            double[] poolucls = new double[linesinfile];
            double[] sattermeans = new double[linesinfile];
            double[] satterlcls = new double[linesinfile];
            double[] satterucls = new double[linesinfile];
            double[] sds = new double[linesinfile];
            double[] equaldfs = new double[linesinfile];
            double[] equalts = new double[linesinfile];
            double[] equaltps = new double[linesinfile];
            double[] unequaldfs = new double[linesinfile];
            double[] unequalts = new double[linesinfile];
            double[] unequaltps = new double[linesinfile];
            double[] betweensss = new double[linesinfile];
            double[] withinsss = new double[linesinfile];
            double[] totalsss = new double[linesinfile];
            double[] betweendfs = new double[linesinfile];
            double[] withindfs = new double[linesinfile];
            double[] totaldfs = new double[linesinfile];
            double[] betweenmss = new double[linesinfile];
            double[] withinmss = new double[linesinfile];
            double[] fanovas = new double[linesinfile];
            double[] pfanovas = new double[linesinfile];
            double[] dfbarts = new double[linesinfile];
            double[] chibarts = new double[linesinfile];
            double[] chipbarts = new double[linesinfile];
            double[] chikwts = new double[linesinfile];
            double[] dfkwts = new double[linesinfile];
            double[] pchikwts = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[1], out obs0s[i]);
                rv = Double.TryParse(values[2], out obs1s[i]);
                rv = Double.TryParse(values[3], out total0s[i]);
                rv = Double.TryParse(values[4], out total1s[i]);
                rv = Double.TryParse(values[5], out mean0s[i]);
                rv = Double.TryParse(values[6], out mean1s[i]);
                rv = Double.TryParse(values[7], out var0s[i]);
                rv = Double.TryParse(values[8], out var1s[i]);
                rv = Double.TryParse(values[9], out sd0s[i]);
                rv = Double.TryParse(values[10], out sd1s[i]);
                rv = Double.TryParse(values[11], out min0s[i]);
                rv = Double.TryParse(values[12], out min1s[i]);
                rv = Double.TryParse(values[13], out q10s[i]);
                rv = Double.TryParse(values[14], out q11s[i]);
                rv = Double.TryParse(values[15], out median0s[i]);
                rv = Double.TryParse(values[16], out median1s[i]);
                rv = Double.TryParse(values[17], out q30s[i]);
                rv = Double.TryParse(values[18], out q31s[i]);
                rv = Double.TryParse(values[19], out max0s[i]);
                rv = Double.TryParse(values[20], out max1s[i]);
                rv = Double.TryParse(values[21], out mode0s[i]);
                rv = Double.TryParse(values[22], out mode1s[i]);
                rv = Double.TryParse(values[23], out poolmeans[i]);
                rv = Double.TryParse(values[24], out poollcls[i]);
                rv = Double.TryParse(values[25], out poolucls[i]);
                rv = Double.TryParse(values[26], out sattermeans[i]);
                rv = Double.TryParse(values[27], out satterlcls[i]);
                rv = Double.TryParse(values[28], out satterucls[i]);
                rv = Double.TryParse(values[29], out sds[i]);
                rv = Double.TryParse(values[30], out equaldfs[i]);
                rv = Double.TryParse(values[31], out equalts[i]);
                rv = Double.TryParse(values[32], out equaltps[i]);
                rv = Double.TryParse(values[33], out unequaldfs[i]);
                rv = Double.TryParse(values[34], out unequalts[i]);
                rv = Double.TryParse(values[35], out unequaltps[i]);
                rv = Double.TryParse(values[36], out betweensss[i]);
                rv = Double.TryParse(values[37], out withinsss[i]);
                rv = Double.TryParse(values[38], out totalsss[i]);
                rv = Double.TryParse(values[39], out betweendfs[i]);
                rv = Double.TryParse(values[40], out withindfs[i]);
                rv = Double.TryParse(values[41], out totaldfs[i]);
                rv = Double.TryParse(values[42], out betweenmss[i]);
                rv = Double.TryParse(values[43], out withinmss[i]);
                rv = Double.TryParse(values[44], out fanovas[i]);
                rv = Double.TryParse(values[45], out pfanovas[i]);
                rv = Double.TryParse(values[46], out dfbarts[i]);
                rv = Double.TryParse(values[47], out chibarts[i]);
                rv = Double.TryParse(values[48], out chipbarts[i]);
                rv = Double.TryParse(values[49], out chikwts[i]);
                rv = Double.TryParse(values[50], out dfkwts[i]);
                rv = Double.TryParse(values[51], out pchikwts[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            EpiDashboard.MeansParameters Parameters = new MeansParameters();
            Parameters.CrosstabVariableName = "crosstabvar2";
            Parameters.ColumnNames.Add("meanvar");
            Parameters.ColumnNames.Add("crosstabvar2");
            Parameters.IncludeFullSummaryStatistics = true;

            for (int j = 0; j < iterations; j++)
            {
                getData("MeansTestData", "iteration = " + j);
                Parameters.CustomFilter = "iteration = " + j;
                Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(Parameters);
                foreach (object key in stratifiedFrequencyTables.Keys)
                {
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].observations, obs0s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].observations, obs1s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].sum.Value, total0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].sum.Value, total1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].mean.Value, mean0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].mean.Value, mean1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].variance.Value, var0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].variance.Value, var1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].stdDev.Value, sd0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].stdDev.Value, sd1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].min.Value, min0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].min.Value, min1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].q1.Value, q10s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].q1.Value, q11s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].median.Value, median0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].median.Value, median1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].q3.Value, q30s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].q3.Value, q31s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].max.Value, max0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].max.Value, max1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].mode.Value, mode0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].mode.Value, mode1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].meansDiff, poolmeans[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].equalLCLMean, poollcls[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].equalUCLMean, poolucls[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].meansDiff, sattermeans[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].unequalLCLMean, satterlcls[j], 0.001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].unequalUCLMean, satterucls[j], 0.001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].stdDevDiff, sds[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].df, equaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].tStatistic, equalts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].pEqual, equaltps[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].SatterthwaiteDF, unequaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].tStatisticUnequal, unequalts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].pUneqal, unequaltps[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssBetween.Value, betweensss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssWithin.Value, withinsss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssBetween.Value + stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssWithin.Value, totalsss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, betweendfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfWithin.Value, withindfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value + stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfWithin.Value, totaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].msBetween.Value, betweenmss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].msWithin.Value, withinmss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].fStatistic.Value, fanovas[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].anovaPValue.Value, pfanovas[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, dfbarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].chiSquare.Value, chibarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].bartlettPValue.Value, chipbarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].kruskalWallisH.Value, chikwts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, dfkwts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].kruskalPValue.Value, pchikwts[j], 0.0001);
                }
            }
        }

        [TestMethod]
        public void MeansTestFourValues()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\MeansFourValueResults.csv"));
            int linesinfile = iterations;
            double[] obs0s = new double[linesinfile];
            double[] obs1s = new double[linesinfile];
            double[] obs2s = new double[linesinfile];
            double[] obs3s = new double[linesinfile];
            double[] total0s = new double[linesinfile];
            double[] total1s = new double[linesinfile];
            double[] total2s = new double[linesinfile];
            double[] total3s = new double[linesinfile];
            double[] mean0s = new double[linesinfile];
            double[] mean1s = new double[linesinfile];
            double[] mean2s = new double[linesinfile];
            double[] mean3s = new double[linesinfile];
            double[] var0s = new double[linesinfile];
            double[] var1s = new double[linesinfile];
            double[] var2s = new double[linesinfile];
            double[] var3s = new double[linesinfile];
            double[] sd0s = new double[linesinfile];
            double[] sd1s = new double[linesinfile];
            double[] sd2s = new double[linesinfile];
            double[] sd3s = new double[linesinfile];
            double[] min0s = new double[linesinfile];
            double[] min1s = new double[linesinfile];
            double[] min2s = new double[linesinfile];
            double[] min3s = new double[linesinfile];
            double[] q10s = new double[linesinfile];
            double[] q11s = new double[linesinfile];
            double[] q12s = new double[linesinfile];
            double[] q13s = new double[linesinfile];
            double[] median0s = new double[linesinfile];
            double[] median1s = new double[linesinfile];
            double[] median2s = new double[linesinfile];
            double[] median3s = new double[linesinfile];
            double[] q30s = new double[linesinfile];
            double[] q31s = new double[linesinfile];
            double[] q32s = new double[linesinfile];
            double[] q33s = new double[linesinfile];
            double[] max0s = new double[linesinfile];
            double[] max1s = new double[linesinfile];
            double[] max2s = new double[linesinfile];
            double[] max3s = new double[linesinfile];
            double[] mode0s = new double[linesinfile];
            double[] mode1s = new double[linesinfile];
            double[] mode2s = new double[linesinfile];
            double[] mode3s = new double[linesinfile];
            double[] betweensss = new double[linesinfile];
            double[] withinsss = new double[linesinfile];
            double[] totalsss = new double[linesinfile];
            double[] betweendfs = new double[linesinfile];
            double[] withindfs = new double[linesinfile];
            double[] totaldfs = new double[linesinfile];
            double[] betweenmss = new double[linesinfile];
            double[] withinmss = new double[linesinfile];
            double[] fanovas = new double[linesinfile];
            double[] pfanovas = new double[linesinfile];
            double[] dfbarts = new double[linesinfile];
            double[] chibarts = new double[linesinfile];
            double[] chipbarts = new double[linesinfile];
            double[] chikwts = new double[linesinfile];
            double[] dfkwts = new double[linesinfile];
            double[] pchikwts = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[1], out obs0s[i]);
                rv = Double.TryParse(values[2], out obs1s[i]);
                rv = Double.TryParse(values[3], out obs2s[i]);
                rv = Double.TryParse(values[4], out obs3s[i]);
                rv = Double.TryParse(values[5], out total0s[i]);
                rv = Double.TryParse(values[6], out total1s[i]);
                rv = Double.TryParse(values[7], out total2s[i]);
                rv = Double.TryParse(values[8], out total3s[i]);
                rv = Double.TryParse(values[9], out mean0s[i]);
                rv = Double.TryParse(values[10], out mean1s[i]);
                rv = Double.TryParse(values[11], out mean2s[i]);
                rv = Double.TryParse(values[12], out mean3s[i]);
                rv = Double.TryParse(values[13], out var0s[i]);
                rv = Double.TryParse(values[14], out var1s[i]);
                rv = Double.TryParse(values[15], out var2s[i]);
                rv = Double.TryParse(values[16], out var3s[i]);
                rv = Double.TryParse(values[17], out sd0s[i]);
                rv = Double.TryParse(values[18], out sd1s[i]);
                rv = Double.TryParse(values[19], out sd2s[i]);
                rv = Double.TryParse(values[20], out sd3s[i]);
                rv = Double.TryParse(values[21], out min0s[i]);
                rv = Double.TryParse(values[22], out min1s[i]);
                rv = Double.TryParse(values[23], out min2s[i]);
                rv = Double.TryParse(values[24], out min3s[i]);
                rv = Double.TryParse(values[25], out q10s[i]);
                rv = Double.TryParse(values[26], out q11s[i]);
                rv = Double.TryParse(values[27], out q12s[i]);
                rv = Double.TryParse(values[28], out q13s[i]);
                rv = Double.TryParse(values[29], out median0s[i]);
                rv = Double.TryParse(values[30], out median1s[i]);
                rv = Double.TryParse(values[31], out median2s[i]);
                rv = Double.TryParse(values[32], out median3s[i]);
                rv = Double.TryParse(values[33], out q30s[i]);
                rv = Double.TryParse(values[34], out q31s[i]);
                rv = Double.TryParse(values[35], out q32s[i]);
                rv = Double.TryParse(values[36], out q33s[i]);
                rv = Double.TryParse(values[37], out max0s[i]);
                rv = Double.TryParse(values[38], out max1s[i]);
                rv = Double.TryParse(values[39], out max2s[i]);
                rv = Double.TryParse(values[40], out max3s[i]);
                rv = Double.TryParse(values[41], out mode0s[i]);
                rv = Double.TryParse(values[42], out mode1s[i]);
                rv = Double.TryParse(values[43], out mode2s[i]);
                rv = Double.TryParse(values[44], out mode3s[i]);
                rv = Double.TryParse(values[45], out betweensss[i]);
                rv = Double.TryParse(values[46], out withinsss[i]);
                rv = Double.TryParse(values[47], out totalsss[i]);
                rv = Double.TryParse(values[48], out betweendfs[i]);
                rv = Double.TryParse(values[49], out withindfs[i]);
                rv = Double.TryParse(values[50], out totaldfs[i]);
                rv = Double.TryParse(values[51], out betweenmss[i]);
                rv = Double.TryParse(values[52], out withinmss[i]);
                rv = Double.TryParse(values[53], out fanovas[i]);
                rv = Double.TryParse(values[54], out pfanovas[i]);
                rv = Double.TryParse(values[55], out dfbarts[i]);
                rv = Double.TryParse(values[56], out chibarts[i]);
                rv = Double.TryParse(values[57], out chipbarts[i]);
                rv = Double.TryParse(values[58], out chikwts[i]);
                rv = Double.TryParse(values[59], out dfkwts[i]);
                rv = Double.TryParse(values[60], out pchikwts[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            EpiDashboard.MeansParameters Parameters = new MeansParameters();
            Parameters.CrosstabVariableName = "crosstabvar4";
            Parameters.ColumnNames.Add("meanvar");
            Parameters.ColumnNames.Add("crosstabvar4");
            Parameters.IncludeFullSummaryStatistics = true;

            for (int j = 0; j < iterations; j++)
            {
                getData("MeansTestData", "iteration = " + j);
                Parameters.CustomFilter = "iteration = " + j;
                Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(Parameters);
                foreach (object key in stratifiedFrequencyTables.Keys)
                {
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].observations, obs0s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].observations, obs1s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].observations, obs2s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].observations, obs3s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].sum.Value, total0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].sum.Value, total1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].sum.Value, total2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].sum.Value, total3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].mean.Value, mean0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].mean.Value, mean1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].mean.Value, mean2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].mean.Value, mean3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].variance.Value, var0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].variance.Value, var1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].variance.Value, var2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].variance.Value, var3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].stdDev.Value, sd0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].stdDev.Value, sd1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].stdDev.Value, sd2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].stdDev.Value, sd3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].min.Value, min0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].min.Value, min1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].min.Value, min2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].min.Value, min3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].q1.Value, q10s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].q1.Value, q11s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].q1.Value, q12s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].q1.Value, q13s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].median.Value, median0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].median.Value, median1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].median.Value, median2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].median.Value, median3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].q3.Value, q30s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].q3.Value, q31s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].q3.Value, q32s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].q3.Value, q33s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].max.Value, max0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].max.Value, max1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].max.Value, max2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].max.Value, max3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].mode.Value, mode0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].mode.Value, mode1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][2].mode.Value, mode2s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][3].mode.Value, mode3s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssBetween.Value, betweensss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssWithin.Value, withinsss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssBetween.Value + stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssWithin.Value, totalsss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, betweendfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfWithin.Value, withindfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value + stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfWithin.Value, totaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].msBetween.Value, betweenmss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].msWithin.Value, withinmss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].fStatistic.Value, fanovas[j], 0.0001);
                    if (pfanovas[j] < 0.001)
                        Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].anovaPValue.Value, pfanovas[j], 0.0001);
                    else if (pfanovas[j] < 0.01)
                        Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].anovaPValue.Value, pfanovas[j], 0.001);
                    else if (pfanovas[j] < 0.1)
                        Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].anovaPValue.Value, pfanovas[j], 0.01);
                    else
                        Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].anovaPValue.Value, pfanovas[j], 0.1);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, dfbarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].chiSquare.Value, chibarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].bartlettPValue.Value, chipbarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].kruskalWallisH.Value, chikwts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, dfkwts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].kruskalPValue.Value, pchikwts[j], 0.0001);
                }
            }
        }

        [TestMethod]
        public void TwoByTwoTest()
        {
            StatisticsRepository.cTable cTable = new StatisticsRepository.cTable();
            var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LogisticTestData.csv");
            var expectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TwoBy2Stats.csv");
            var cellCountsByIteration = new Dictionary<int, int[]>();
            bool isHeader = true;

            foreach (var line in File.ReadLines(inputPath))
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                var values = line.Split(',');
                Assert.AreEqual(5, values.Length, "Unexpected column count in LogisticTestData.csv.");
                int iteration = int.Parse(values[0], CultureInfo.InvariantCulture);
                int depvar = int.Parse(values[2], CultureInfo.InvariantCulture);
                int indepvar = int.Parse(values[3], CultureInfo.InvariantCulture);
                Assert.IsTrue(depvar == 0 || depvar == 1, $"Unexpected outcome in input iteration {iteration}.");
                Assert.IsTrue(indepvar == 0 || indepvar == 1, $"Unexpected exposure in input iteration {iteration}.");

                int[] cellCounts;
                if (!cellCountsByIteration.TryGetValue(iteration, out cellCounts))
                {
                    cellCounts = new int[4];
                    cellCountsByIteration.Add(iteration, cellCounts);
                }

                int cell = depvar == 1 ? (indepvar == 1 ? 0 : 1) : (indepvar == 1 ? 2 : 3);
                cellCounts[cell]++;
            }

            var expectedRows = File.ReadAllLines(expectedPath);
            Assert.AreEqual(100, expectedRows.Length, "Unexpected number of 2x2 reference rows.");
            Assert.AreEqual(expectedRows.Length, cellCountsByIteration.Count, "Input and expected fixtures have different iteration counts.");

            for (int j = 0; j < expectedRows.Length; j++)
            {
                var values = expectedRows[j].TrimEnd(',').Split(',');
                Assert.AreEqual(17, values.Length, $"Unexpected column count in 2x2 reference row {j + 1}.");
                int iteration = int.Parse(values[0], CultureInfo.InvariantCulture);
                Assert.AreEqual(j, iteration, $"Unexpected iteration in 2x2 reference row {j + 1}.");

                int[] cellCounts;
                Assert.IsTrue(cellCountsByIteration.TryGetValue(iteration, out cellCounts), $"Missing input for 2x2 iteration {iteration}.");
                var singleTableResults = cTable.SigTable(cellCounts[0], cellCounts[1], cellCounts[2], cellCounts[3], 0.95);
                string context = $"2x2 fixture iteration {iteration}.";
                Assert.AreEqual(double.Parse(values[1], CultureInfo.InvariantCulture), singleTableResults.ChiSquareUncorrectedVal, 0.0001, context + " Uncorrected chi-square.");
                Assert.AreEqual(double.Parse(values[2], CultureInfo.InvariantCulture), singleTableResults.ChiSquareUncorrected2P, 0.0001, context + " Uncorrected chi-square p-value.");
                Assert.AreEqual(double.Parse(values[3], CultureInfo.InvariantCulture), singleTableResults.ChiSquareMantelVal, 0.0001, context + " Mantel chi-square.");
                Assert.AreEqual(double.Parse(values[4], CultureInfo.InvariantCulture), singleTableResults.ChiSquareMantel2P, 0.0001, context + " Mantel chi-square p-value.");
                Assert.AreEqual(double.Parse(values[5], CultureInfo.InvariantCulture), singleTableResults.ChiSquareYatesVal, 0.0001, context + " Yates chi-square.");
                Assert.AreEqual(double.Parse(values[6], CultureInfo.InvariantCulture), singleTableResults.ChiSquareYates2P, 0.0001, context + " Yates chi-square p-value.");
                Assert.AreEqual(double.Parse(values[7], CultureInfo.InvariantCulture), singleTableResults.OddsRatioEstimate.Value, 0.0001, context + " Odds ratio.");
                Assert.AreEqual(double.Parse(values[8], CultureInfo.InvariantCulture), singleTableResults.OddsRatioLower.Value, 0.0001, context + " Odds-ratio lower limit.");
                Assert.AreEqual(double.Parse(values[9], CultureInfo.InvariantCulture), singleTableResults.OddsRatioUpper.Value, 0.001, context + " Odds-ratio upper limit.");
                Assert.AreEqual(double.Parse(values[10], CultureInfo.InvariantCulture), singleTableResults.RiskRatioEstimate.Value, 0.0001, context + " Risk ratio.");
                Assert.AreEqual(double.Parse(values[11], CultureInfo.InvariantCulture), singleTableResults.RiskRatioLower.Value, 0.0001, context + " Risk-ratio lower limit.");
                Assert.AreEqual(double.Parse(values[12], CultureInfo.InvariantCulture), singleTableResults.RiskRatioUpper.Value, 0.0001, context + " Risk-ratio upper limit.");
                Assert.AreEqual(double.Parse(values[13], CultureInfo.InvariantCulture), singleTableResults.OddsRatioMLEFisherLower, 0.0001, context + " Fisher odds-ratio lower limit.");
                Assert.AreEqual(double.Parse(values[14], CultureInfo.InvariantCulture), singleTableResults.OddsRatioMLEFisherUpper, 0.0001, context + " Fisher odds-ratio upper limit.");
                Assert.AreEqual(double.Parse(values[15], CultureInfo.InvariantCulture), singleTableResults.FisherExactP, 0.0001, context + " One-sided Fisher exact p-value.");
                Assert.AreEqual(double.Parse(values[16], CultureInfo.InvariantCulture), singleTableResults.FisherExact2P, 0.0001, context + " Two-sided Fisher exact p-value.");
            }
        }
    }
}
