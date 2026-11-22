using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using Epi.Core;
using StatisticsRepository;
using EpiInfo.Plugin;
using System.Collections.Generic;
using Epi;
using Epi.Data;
using EpiDashboard;
using EpiDashboard.Controls;
using Epi.Fields;
using System.Data;
using System.Xml;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using EpiDashboard.NutStat;
using EpiDashboard.Gadgets.Reporting;

namespace Epi.UnitTests
{
    [TestClass]
    public class AdvancedAnalysisTests : StatisticsTests
    {
        private static double ParseRegressionExpected(string value, int row, int column)
        {
            double result;
            Assert.IsTrue(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result),
                $"Invalid regression result at row {row + 1}, column {column + 1}.");
            return result;
        }

        private struct VariableRow
        {
            public string variableName;
            public double oddsRatio;
            public double ninetyFivePercent;
            public double ci;
            public double coefficient;
            public double se;
            public double Z;
            public double P;
            public double Ftest;
            public double stdError;
            public double coefficientLower;
            public double coefficientUpper;
        }

        private struct RegressionResults
        {
            public List<VariableRow> variables;
            public string convergence;
            public int iterations;
            public double finalLikelihood;
            public int casesIncluded;
            public double scoreStatistic;
            public double scoreDF;
            public double scoreP;
            public double LRStatistic;
            public double LRDF;
            public double LRP;
            public string errorMessage;
            public StatisticsRepository.LogisticRegression.LogisticRegressionResults regressionResults;
        }

        [TestMethod]
        public void LinearRegressionTest()
        {
            int iterations = 100;

            var resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LinearRegressionResults.csv");
            string[] resultRows = File.ReadAllLines(resultPath);
            Assert.AreEqual(iterations, resultRows.Length, "Unexpected number of linear regression reference rows.");
            int linesinfile = iterations;
            double[] constants = new double[linesinfile];
            double[] constantlcls = new double[linesinfile];
            double[] constantucls = new double[linesinfile];
            double[] constantstes = new double[linesinfile];
            double[] i1s = new double[linesinfile];
            double[] i1lcls = new double[linesinfile];
            double[] i1ucls = new double[linesinfile];
            double[] i1stes = new double[linesinfile];
            double[] i2s = new double[linesinfile];
            double[] i2lcls = new double[linesinfile];
            double[] i2ucls = new double[linesinfile];
            double[] i2stes = new double[linesinfile];
            double[] i3s = new double[linesinfile];
            double[] i3lcls = new double[linesinfile];
            double[] i3ucls = new double[linesinfile];
            double[] i3stes = new double[linesinfile];
            double[] fconstants = new double[linesinfile];
            double[] pfconstants = new double[linesinfile];
            double[] f1s = new double[linesinfile];
            double[] pf1s = new double[linesinfile];
            double[] f2s = new double[linesinfile];
            double[] pf2s = new double[linesinfile];
            double[] f3s = new double[linesinfile];
            double[] pf3s = new double[linesinfile];
            double[] regdfs = new double[linesinfile];
            double[] regsss = new double[linesinfile];
            double[] regmss = new double[linesinfile];
            double[] fstatistics = new double[linesinfile];
            double[] pvalues = new double[linesinfile];
            double[] resdfs = new double[linesinfile];
            double[] ressss = new double[linesinfile];
            double[] resmss = new double[linesinfile];
            double[] totdfs = new double[linesinfile];
            double[] totsss = new double[linesinfile];
            double[] rsquares = new double[linesinfile];
            for (int i = 0; i < resultRows.Length; i++)
            {
                var values = resultRows[i].TrimEnd(',').Split(',');
                Assert.AreEqual(36, values.Length, $"Unexpected column count in linear regression reference row {i + 1}.");
                Assert.AreEqual(i, int.Parse(values[0], CultureInfo.InvariantCulture), $"Unexpected iteration in linear regression reference row {i + 1}.");
                constants[i] = ParseRegressionExpected(values[1], i, 1);
                constantlcls[i] = ParseRegressionExpected(values[2], i, 2);
                constantucls[i] = ParseRegressionExpected(values[3], i, 3);
                constantstes[i] = ParseRegressionExpected(values[4], i, 4);
                i1s[i] = ParseRegressionExpected(values[5], i, 5);
                i1lcls[i] = ParseRegressionExpected(values[6], i, 6);
                i1ucls[i] = ParseRegressionExpected(values[7], i, 7);
                i1stes[i] = ParseRegressionExpected(values[8], i, 8);
                i2s[i] = ParseRegressionExpected(values[9], i, 9);
                i2lcls[i] = ParseRegressionExpected(values[10], i, 10);
                i2ucls[i] = ParseRegressionExpected(values[11], i, 11);
                i2stes[i] = ParseRegressionExpected(values[12], i, 12);
                i3s[i] = ParseRegressionExpected(values[13], i, 13);
                i3lcls[i] = ParseRegressionExpected(values[14], i, 14);
                i3ucls[i] = ParseRegressionExpected(values[15], i, 15);
                i3stes[i] = ParseRegressionExpected(values[16], i, 16);
                fconstants[i] = ParseRegressionExpected(values[17], i, 17);
                pfconstants[i] = ParseRegressionExpected(values[18], i, 18);
                f1s[i] = ParseRegressionExpected(values[19], i, 19);
                pf1s[i] = ParseRegressionExpected(values[20], i, 20);
                f2s[i] = ParseRegressionExpected(values[21], i, 21);
                pf2s[i] = ParseRegressionExpected(values[22], i, 22);
                f3s[i] = ParseRegressionExpected(values[23], i, 23);
                pf3s[i] = ParseRegressionExpected(values[24], i, 24);
                regdfs[i] = ParseRegressionExpected(values[25], i, 25);
                regsss[i] = ParseRegressionExpected(values[26], i, 26);
                regmss[i] = ParseRegressionExpected(values[27], i, 27);
                fstatistics[i] = ParseRegressionExpected(values[28], i, 28);
                pvalues[i] = ParseRegressionExpected(values[29], i, 29);
                resdfs[i] = ParseRegressionExpected(values[30], i, 30);
                ressss[i] = ParseRegressionExpected(values[31], i, 31);
                resmss[i] = ParseRegressionExpected(values[32], i, 32);
                totdfs[i] = ParseRegressionExpected(values[33], i, 33);
                totsss[i] = ParseRegressionExpected(values[34], i, 34);
                rsquares[i] = ParseRegressionExpected(values[35], i, 35);
            }

            var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LinearRegressionTestData.csv");
            var inputTablesByIteration = new Dictionary<int, DataTable>();
            bool isHeader = true;
            foreach (var line in File.ReadLines(inputPath))
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                var values = line.Split(',');
                Assert.AreEqual(6, values.Length, "Unexpected column count in LinearRegressionTestData.csv.");
                int iteration = int.Parse(values[0], CultureInfo.InvariantCulture);
                DataTable dataTable;
                if (!inputTablesByIteration.TryGetValue(iteration, out dataTable))
                {
                    dataTable = new DataTable("iteration" + iteration);
                    dataTable.CaseSensitive = true;
                    dataTable.Columns.Add("depvar", typeof(double));
                    dataTable.Columns.Add("indepvar1", typeof(double));
                    dataTable.Columns.Add("indepvar2", typeof(double));
                    dataTable.Columns.Add("indepvar3", typeof(double));
                    inputTablesByIteration.Add(iteration, dataTable);
                }

                DataRow dataRow = dataTable.NewRow();
                dataRow["depvar"] = double.Parse(values[2], CultureInfo.InvariantCulture);
                dataRow["indepvar1"] = double.Parse(values[3], CultureInfo.InvariantCulture);
                dataRow["indepvar2"] = double.Parse(values[4], CultureInfo.InvariantCulture);
                dataRow["indepvar3"] = double.Parse(values[5], CultureInfo.InvariantCulture);
                dataTable.Rows.Add(dataRow);
            }

            Assert.AreEqual(iterations, inputTablesByIteration.Count, "Unexpected number of linear regression input datasets.");
            for (int j = 0; j < iterations; j++)
            {
                DataTable regressionData;
                Assert.IsTrue(inputTablesByIteration.TryGetValue(j, out regressionData), $"Missing linear regression input for iteration {j}.");
                Assert.AreEqual(100, regressionData.Rows.Count, $"Unexpected input row count for linear regression iteration {j}.");

                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("depvar", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("P", "0.95");
                inputVariableList.Add("indepvar1", "unsorted");
                inputVariableList.Add("indepvar2", "unsorted");
                inputVariableList.Add("indepvar3", "unsorted");

                LinearRegression.LinearRegressionResults results = new LinearRegression.LinearRegressionResults();

                StatisticsRepository.LinearRegression linearRegression = new StatisticsRepository.LinearRegression();
                results = linearRegression.LinearRegression(inputVariableList, regressionData);

                double tScore = 1.9;
                while (Epi.Statistics.SharedResources.PFromT(tScore, results.residualsDf) > 0.025)
                {
                    tScore += 0.000001;
                }
                List<VariableRow> moreStats = new List<VariableRow>();
                foreach (StatisticsRepository.LinearRegression.VariableRow vrow in results.variables)
                {
                    VariableRow nrow = new VariableRow();
                    nrow.coefficient = vrow.coefficient;
                    nrow.Ftest = vrow.Ftest;
                    nrow.P = vrow.P;
                    nrow.stdError = vrow.stdError;
                    nrow.variableName = vrow.variableName;
                    nrow.coefficientLower = nrow.coefficient - tScore * nrow.stdError;
                    nrow.coefficientUpper = nrow.coefficient + tScore * nrow.stdError;
                    moreStats.Add(nrow);
                }
                double regressionFp = Epi.Statistics.SharedResources.PFromF(results.regressionF, results.regressionDf, results.residualsDf);

                Assert.AreEqual(results.variables[3].coefficient, constants[j], 0.0001);
                Assert.AreEqual(moreStats[3].coefficientLower, constantlcls[j], 0.0001);
                Assert.AreEqual(moreStats[3].coefficientUpper, constantucls[j], 0.0001);
                Assert.AreEqual(moreStats[3].stdError, constantstes[j], 0.0001);
                Assert.AreEqual(results.variables[0].coefficient, i1s[j], 0.0001);
                Assert.AreEqual(moreStats[0].coefficientLower, i1lcls[j], 0.0001);
                Assert.AreEqual(moreStats[0].coefficientUpper, i1ucls[j], 0.0001);
                Assert.AreEqual(moreStats[0].stdError, i1stes[j], 0.0001);
                Assert.AreEqual(results.variables[1].coefficient, i2s[j], 0.0001);
                Assert.AreEqual(moreStats[1].coefficientLower, i2lcls[j], 0.0001);
                Assert.AreEqual(moreStats[1].coefficientUpper, i2ucls[j], 0.0001);
                Assert.AreEqual(moreStats[1].stdError, i2stes[j], 0.0001);
                Assert.AreEqual(results.variables[2].coefficient, i3s[j], 0.0001);
                Assert.AreEqual(moreStats[2].coefficientLower, i3lcls[j], 0.0001);
                Assert.AreEqual(moreStats[2].coefficientUpper, i3ucls[j], 0.0001);
                Assert.AreEqual(moreStats[2].stdError, i3stes[j], 0.0001);
                Assert.AreEqual(results.variables[3].Ftest, fconstants[j], 0.0001);
                Assert.AreEqual(results.variables[3].P, pfconstants[j], 0.0001);
                Assert.AreEqual(results.variables[0].Ftest, f1s[j], 0.0001);
                Assert.AreEqual(results.variables[0].P, pf1s[j], 0.0001);
                Assert.AreEqual(results.variables[1].Ftest, f2s[j], 0.0001);
                Assert.AreEqual(results.variables[1].P, pf2s[j], 0.0001);
                Assert.AreEqual(results.variables[2].Ftest, f3s[j], 0.0001);
                Assert.AreEqual(results.variables[2].P, pf3s[j], 0.0001);
                Assert.AreEqual(results.regressionDf, regdfs[j], 0.0001);
                Assert.AreEqual(results.regressionSumOfSquares, regsss[j], 0.0001);
                Assert.AreEqual(results.regressionMeanSquare, regmss[j], 0.0001);
                Assert.AreEqual(results.regressionF, fstatistics[j], 0.0001);
                if (pvalues[j] < 0.001)
                    Assert.AreEqual(regressionFp, pvalues[j], 0.0001);
                else if (pvalues[j] < 0.01)
                    Assert.AreEqual(regressionFp, pvalues[j], 0.001);
                else if (pvalues[j] < 0.1)
                    Assert.AreEqual(regressionFp, pvalues[j], 0.01);
                else
                    Assert.AreEqual(regressionFp, pvalues[j], 0.1);
                Assert.AreEqual(results.residualsDf, resdfs[j], 0.0001);
                Assert.AreEqual(results.residualsSumOfSquares, ressss[j], 0.0001);
                Assert.AreEqual(results.residualsMeanSquare, resmss[j], 0.0001);
                Assert.AreEqual(results.totalDf, totdfs[j], 0.0001);
                Assert.AreEqual(results.totalSumOfSquares, totsss[j], 0.0001);
                Assert.AreEqual(results.correlationCoefficient, rsquares[j], 0.0001);
            }
        }

        [TestMethod]
        public void LogisticRegressionTest()
        {
            int iterations = 100;
            var coefficientPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LogisticCoefficientResults.csv");
            var oddsRatioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LogisticOddsRatioResults.csv");
            var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LogisticTestData.csv");
            string[] coefficientRows = File.ReadAllLines(coefficientPath);
            string[] oddsRatioRows = File.ReadAllLines(oddsRatioPath);
            Assert.AreEqual(iterations * 3, coefficientRows.Length, "Unexpected number of logistic coefficient rows.");
            Assert.AreEqual(iterations * 2, oddsRatioRows.Length, "Unexpected number of logistic odds-ratio rows.");

            double[] coefficients = new double[coefficientRows.Length];
            double[] stderrs = new double[coefficientRows.Length];
            double[] chisqs = new double[coefficientRows.Length];
            double[] ps = new double[coefficientRows.Length];
            string[] coefficientNames = { "Intercept", "indepvar1", "indepvar2" };
            for (int i = 0; i < coefficientRows.Length; i++)
            {
                var values = coefficientRows[i].Split(',');
                Assert.AreEqual(6, values.Length, $"Unexpected column count in logistic coefficient row {i + 1}.");
                Assert.AreEqual(i / 3, int.Parse(values[0], CultureInfo.InvariantCulture), $"Unexpected iteration in logistic coefficient row {i + 1}.");
                Assert.AreEqual(coefficientNames[i % 3], values[1].Trim('"'), $"Unexpected variable in logistic coefficient row {i + 1}.");
                coefficients[i] = ParseRegressionExpected(values[2], i, 2);
                stderrs[i] = ParseRegressionExpected(values[3], i, 3);
                chisqs[i] = ParseRegressionExpected(values[4], i, 4);
                ps[i] = ParseRegressionExpected(values[5], i, 5);
            }

            double[] oddsratios = new double[oddsRatioRows.Length];
            double[] lowercls = new double[oddsRatioRows.Length];
            double[] uppercls = new double[oddsRatioRows.Length];
            string[] oddsRatioNames = { "indepvar1 1 vs 0", "indepvar2" };
            for (int i = 0; i < oddsRatioRows.Length; i++)
            {
                var values = oddsRatioRows[i].Split(',');
                Assert.AreEqual(5, values.Length, $"Unexpected column count in logistic odds-ratio row {i + 1}.");
                Assert.AreEqual(i / 2, int.Parse(values[0], CultureInfo.InvariantCulture), $"Unexpected iteration in logistic odds-ratio row {i + 1}.");
                Assert.AreEqual(oddsRatioNames[i % 2], values[1].Trim('"'), $"Unexpected variable in logistic odds-ratio row {i + 1}.");
                oddsratios[i] = ParseRegressionExpected(values[2], i, 2);
                lowercls[i] = ParseRegressionExpected(values[3], i, 3);
                uppercls[i] = ParseRegressionExpected(values[4], i, 4);
            }

            var inputTablesByIteration = new Dictionary<int, DataTable>();
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
                DataTable dataTable;
                if (!inputTablesByIteration.TryGetValue(iteration, out dataTable))
                {
                    dataTable = new DataTable("iteration" + iteration);
                    dataTable.CaseSensitive = true;
                    dataTable.Columns.Add("depvar", typeof(double));
                    dataTable.Columns.Add("indepvar1", typeof(double));
                    dataTable.Columns.Add("indepvar2", typeof(double));
                    inputTablesByIteration.Add(iteration, dataTable);
                }

                DataRow dataRow = dataTable.NewRow();
                dataRow["depvar"] = double.Parse(values[2], CultureInfo.InvariantCulture);
                dataRow["indepvar1"] = double.Parse(values[3], CultureInfo.InvariantCulture);
                dataRow["indepvar2"] = double.Parse(values[4], CultureInfo.InvariantCulture);
                dataTable.Rows.Add(dataRow);
            }

            Assert.AreEqual(iterations, inputTablesByIteration.Count, "Unexpected number of logistic input datasets.");
            for (int j = 0; j < iterations; j++)
            {
                DataTable regressionData;
                Assert.IsTrue(inputTablesByIteration.TryGetValue(j, out regressionData), $"Missing logistic input for iteration {j}.");
                Assert.AreEqual(100, regressionData.Rows.Count, $"Unexpected input row count for logistic iteration {j}.");

                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("depvar", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("P", "0.95");
                inputVariableList.Add("indepvar1", "unsorted");
                inputVariableList.Add("indepvar2", "unsorted");

                RegressionResults results = new RegressionResults();

                StatisticsRepository.LogisticRegression logisticRegression = new StatisticsRepository.LogisticRegression();
                results.regressionResults = logisticRegression.LogisticRegression(inputVariableList, regressionData);
                Assert.IsTrue(string.IsNullOrEmpty(results.regressionResults.errorMessage), $"Logistic regression failed for iteration {j}: {results.regressionResults.errorMessage}");
                Assert.AreEqual(3, results.regressionResults.variables.Count, $"Unexpected result-variable count for logistic iteration {j}.");
                Assert.AreEqual(coefficients[j * 3], results.regressionResults.variables[2].coefficient, 0.01);
                Assert.AreEqual(stderrs[j * 3], results.regressionResults.variables[2].se, 0.01);
                Assert.AreEqual(ps[j * 3], results.regressionResults.variables[2].P, 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].oddsRatio, oddsratios[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ninetyFivePercent, lowercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ci, uppercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].coefficient, coefficients[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].se, stderrs[j * 3 + 1], 0.01);
//                Assert.AreEqual(results.regressionResults.variables[0].Z, chisqs[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].P, ps[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].oddsRatio, oddsratios[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ninetyFivePercent, lowercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ci, uppercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].coefficient, coefficients[j * 3 + 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].se, stderrs[j * 3 + 2], 0.01);
//                Assert.AreEqual(results.regressionResults.variables[1].Z, chisqs[j * 3 + 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].P, ps[j * 3 + 2], 0.01);
            }
        }

        [TestMethod]
        public void MatchedLogisticRegressionTest()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\MatchedLogisticCoefficientResults.csv"));
            int linesinfile = 2 * iterations;
            double[] coefficients = new double[linesinfile];
            double[] stderrs = new double[linesinfile];
            double[] chisqs = new double[linesinfile];
            double[] ps = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out coefficients[i]);
                rv = Double.TryParse(values[3], out stderrs[i]);
                rv = Double.TryParse(values[4], out chisqs[i]);
                rv = Double.TryParse(values[5], out ps[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            reader = new StreamReader(File.OpenRead(@"..\..\Data\MatchedLogisticOddsRatioResults.csv"));
            linesinfile = 2 * iterations;
            double[] oddsratios = new double[linesinfile];
            double[] lowercls = new double[linesinfile];
            double[] uppercls = new double[linesinfile];
            i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out oddsratios[i]);
                rv = Double.TryParse(values[3], out lowercls[i]);
                rv = Double.TryParse(values[4], out uppercls[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            reader = new StreamReader(File.OpenRead(@"..\..\Data\MatchedLogisticGlobalTestResults.csv"));
            linesinfile = 2 * iterations;
            double[] globalchisqs = new double[linesinfile];
            double[] dfs = new double[linesinfile];
            double[] probchisqs = new double[linesinfile];
            i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out globalchisqs[i]);
                rv = Double.TryParse(values[3], out dfs[i]);
                rv = Double.TryParse(values[4], out probchisqs[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            reader = new StreamReader(File.OpenRead(@"..\..\Data\MatchedLogisticFitStatisticResults.csv"));
            linesinfile = iterations;
            double[] withcovariates = new double[linesinfile];
            i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[1], out withcovariates[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            for (int j = 0; j < iterations; j++)
            {
                getData("MatchedLogisticTestData", "iteration = " + j);

                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("case", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("GROUPID", "matchvar");
                inputVariableList.Add("P", "0.95");
                inputVariableList.Add("indepvar1", "unsorted");
                inputVariableList.Add("indepvar2", "unsorted");

                DataTable regressTable = new DataTable();

                RegressionResults results = new RegressionResults();

                StatisticsRepository.LogisticRegression logisticRegression = new StatisticsRepository.LogisticRegression();
                results.regressionResults = logisticRegression.LogisticRegression(inputVariableList, mainTable);
                if (false)
                {
                    System.Diagnostics.Trace.Write(results.regressionResults.variables[0].oddsRatio + "\t" +
                        results.regressionResults.variables[0].ninetyFivePercent + "\t" +
                        results.regressionResults.variables[0].ci + "\t" +
                        results.regressionResults.variables[0].coefficient + "\t" +
                        results.regressionResults.variables[0].se + "\t" +
                        results.regressionResults.variables[0].Z + "\t" +
                        results.regressionResults.variables[0].P + "\n");
                    System.Diagnostics.Trace.Write(results.regressionResults.variables[1].oddsRatio + "\t" +
                        results.regressionResults.variables[1].ninetyFivePercent + "\t" +
                        results.regressionResults.variables[1].ci + "\t" +
                        results.regressionResults.variables[1].coefficient + "\t" +
                        results.regressionResults.variables[1].se + "\t" +
                        results.regressionResults.variables[1].Z + "\t" +
                        results.regressionResults.variables[1].P + "\n");
                }
                Assert.AreEqual(results.regressionResults.variables[0].oddsRatio, oddsratios[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ninetyFivePercent, lowercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ci, uppercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].coefficient, coefficients[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].se, stderrs[j * 2], 0.01);
                //                Assert.AreEqual(results.regressionResults.variables[0].Z, chisqs[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].P, ps[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].oddsRatio, oddsratios[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ninetyFivePercent, lowercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ci, uppercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].coefficient, coefficients[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].se, stderrs[j * 2 + 1], 0.01);
                //                Assert.AreEqual(results.regressionResults.variables[1].Z, chisqs[j * 3 + 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].P, ps[j * 2 + 1], 0.01);

                Assert.AreEqual(results.regressionResults.finalLikelihood, withcovariates[j], 0.0001);
                Assert.AreEqual(results.regressionResults.LRStatistic, globalchisqs[j * 2], 0.0001);
                Assert.AreEqual(results.regressionResults.scoreStatistic, globalchisqs[j * 2 + 1], 0.0001);
                Assert.AreEqual(results.regressionResults.LRDF, dfs[j * 2], 0.0001);
                Assert.AreEqual(results.regressionResults.scoreDF, dfs[j * 2 + 1], 0.0001);
                Assert.AreEqual(results.regressionResults.LRP, probchisqs[j * 2], 0.0001);
                Assert.AreEqual(results.regressionResults.scoreP, probchisqs[j * 2 + 1], 0.0001);
            }
        }
    }
}
