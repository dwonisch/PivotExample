namespace PivotExample {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    public class Pivot : IPivot {

        /// <summary>
        /// Queries data via IDataReader and creates a pivot table with the columns configured in configuration
        /// </summary>
        /// <param name="reader">The IDataReader that is used to query the data.</param>
        /// <param name="configuration">This parameter defines which data should be used, in which order it should be displayed and the columns display names.</param>
        /// <returns>A Pivot Table of the queried data.</returns>
        public DataTable CreatePivot(IDataReader reader, IList<ColumnConfiguration> configuration) {
            var watch = Stopwatch.StartNew();
            var columnDefinitions = new List<ColumnDefinition>();
            var data = FetchData(reader, configuration, columnDefinitions);
            Console.WriteLine("FetchData: {0}", watch.ElapsedMilliseconds);
            watch.Reset();
            watch.Start();
            var returnTable = CreateTableDefinition(columnDefinitions);
            Console.WriteLine("CreateTableDefinition: {0}", watch.ElapsedMilliseconds);
            watch.Reset();
            watch.Start();

            CalculatePivot(returnTable, data, configuration);
            Console.WriteLine("CalculatePivot: {0}", watch.ElapsedMilliseconds);
            watch.Reset();
            watch.Start();
            CheckForNullValues(returnTable);
            Console.WriteLine("CheckForNullValues: {0}", watch.ElapsedMilliseconds);

            return returnTable;
        }

        /// <summary>
        /// Queries data from IDataReader and returns all data converted into a List of DataObject
        /// </summary>
        /// <param name="reader">The IDataReader from which the data is read.</param>
        /// <param name="configuration">The configuration of all columns that should be contained in Pivot Table.</param>
        /// <param name="columns">A collection of columns that is filled by this method.</param>
        /// <returns>A List of DataObject that contains each row's data.</returns>
        private static IList<DataObject> FetchData(IDataReader reader, IList<ColumnConfiguration> configuration, List<ColumnDefinition> columns) {
            //Add first data column by which all values are grouped by
            columns.Add(new ColumnDefinition("PARTID", 0, typeof(string)));

            var dataObjects = new List<DataObject>();

            while (reader.Read()) {
                var dataObject = new DataObject(reader);
                dataObjects.Add(dataObject);
                GetColumns(columns, configuration, dataObject);
            }

            return dataObjects;
        }

        /// <summary>
        /// Create a column definition for each data row combined with configuration data.
        /// </summary>
        /// <param name="data">All rows that were returned from database.</param>
        /// <param name="configuration">The definition in which order and what columns to return.</param>
        /// <param name="dataEntry">The entry that is used to create additional columns.</param>
        /// <returns>A List of column definitions.</returns>
        private static void GetColumns(IList<ColumnDefinition> columns, IList<ColumnConfiguration> configuration, DataObject dataEntry) {
            foreach (var configurationEntry in configuration) {
                if (!columns.Any(c => c.DisplayName == configurationEntry.DisplayName) && configurationEntry.Name == dataEntry.ValueId) {
                    columns.Add(new ColumnDefinition(configurationEntry.DisplayName, configurationEntry.SortOrder, configurationEntry.DataType));
                }
            }
        }

        /// <summary>
        /// Converts a List of ColumnDefinition into a DataTable
        /// </summary>
        /// <param name="columnDefinitions">For each column definition a DataColumn is added to the returned DataTable.</param>
        /// <returns>A DataTable based on the committed column definitions.</returns>
        private DataTable CreateTableDefinition(List<ColumnDefinition> columnDefinitions) {
            var sortedDefinitions = columnDefinitions.OrderBy(d => d.SortOrder).ToList();

            var dataTable = new DataTable();

            foreach (var columnDefinition in sortedDefinitions) {
                dataTable.Columns.Add(columnDefinition.DisplayName, columnDefinition.DataType);
            }

            return dataTable;
        }

        /// <summary>
        /// Pivoting queried data.
        /// </summary>
        /// <param name="returnTable">The DataTable that should be filled with data.</param>
        /// <param name="data">The data that is used to fill the DataTable.</param>
        /// <param name="configuration">The column configuration that was used to create the datatable.</param>
        private void CalculatePivot(DataTable returnTable, IList<DataObject> data, IList<ColumnConfiguration> configuration) {
            returnTable.PrimaryKey = new[] { returnTable.Columns["PARTID"] };

            foreach (var dataEntry in data) {
                var row = returnTable.Select(string.Format("PARTID = '{0}'", dataEntry.PartId)).FirstOrDefault();

                if (row == null) {
                    row = returnTable.NewRow();
                    row["PARTID"] = dataEntry.PartId;
                    returnTable.Rows.Add(row);
                }

                string columnName = dataEntry.ValueId;
                foreach (var configurationEntry in configuration.Where(c => columnName.Equals(c.Name))) {
                    columnName = configurationEntry.DisplayName;
                    break;
                }

                foreach (DataColumn column in returnTable.Columns) {
                    if (column.ColumnName == columnName) {
                        row[columnName] = dataEntry.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Checks each DataCell for null entries and replaces it with a default value.
        /// </summary>
        /// <param name="returnTable">The DataTable that should be checked for null values.</param>
        private void CheckForNullValues(DataTable returnTable) {
            foreach (DataRow row in returnTable.Rows) {
                foreach (DataColumn column in returnTable.Columns) {
                    if (row.IsNull(column.ColumnName) && column.DataType == typeof(string))
                        row[column.ColumnName] = "<No data available>";
                }
            }
        }
    }
}
