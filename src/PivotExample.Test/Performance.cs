namespace PivotExample.Test {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Performance {
        [TestMethod]
        public void ExecutePivot_10() {
            PerformanceTest(10, 100);
            PerformanceTest(10, 1000);
            PerformanceTest(10, 10000);
            PerformanceTest(10, 100000);
        }

        [TestMethod]
        public void ExecutePivot_50() {
            PerformanceTest(50, 100);
            PerformanceTest(50, 1000);
            PerformanceTest(50, 10000);
            PerformanceTest(50, 100000);
        }

        [TestMethod]
        public void ExecutePivot_100() {
            PerformanceTest(100, 100);
            PerformanceTest(100, 1000);
            PerformanceTest(100, 10000);
            PerformanceTest(100, 100000);
        }

        [TestMethod]
        public void Optimization() {
            PerformanceTest(100, 10000);
        }

        /// <summary>
        /// Executes a new performance test using the specified parameters.
        /// </summary>
        /// <param name="valueCount">Specifies how much different values are stored for each part.</param>
        /// <param name="partCount">Specifies how much different parts are generated (each containing 'valueCount' values)</param>
        private void PerformanceTest(int valueCount, int partCount) {
            var pivot = new Pivot();
            var configuration = GenerateConfiguraton(valueCount).ToList();

            var watch = new Stopwatch();
            //Definies how much runs are done to get a better average result.
            const int testRuns = 10;

            for (int i = 0; i < testRuns; i++) {
                //Querying data from a MemoryDatReader so we would get more precise results and won't need a SQL database.
                using (var datareader = new MemoryDataReader(GenerateTestData(configuration, partCount))) {
                    watch.Start();
                    pivot.CreatePivot(datareader, configuration);
                    watch.Stop();
                }
            }
            Console.WriteLine("{0}*{1} = {2}", valueCount, partCount, watch.ElapsedMilliseconds / testRuns);
        }

        private IEnumerable<ColumnConfiguration> GenerateConfiguraton(int items) {
            for (int i = 0; i < items; i++) {
                var name = "DATA" + i;
                yield return new ColumnConfiguration(name, name, i+1, typeof(string));
            }
        }

        private IEnumerable<DataRow> GenerateTestData(IList<ColumnConfiguration> configuration, int testCount) {
            DataTable dt = new DataTable();
            dt.Columns.Add("PARTID", typeof(string));
            dt.Columns.Add("VALUEID", typeof(string));
            dt.Columns.Add("VALUE", typeof(string));

            long dummyValue = 0;

            for (int i = 0; i < testCount; i++) {
                foreach (var c in configuration) {
                    var row = dt.NewRow();
                    row["PARTID"] = i.ToString();
                    row["VALUEID"] = c.Name;
                    row["VALUE"] = dummyValue++;

                    yield return row;
                }
            }
        }

    }
}
