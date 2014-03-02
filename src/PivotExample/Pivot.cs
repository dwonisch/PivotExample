namespace PivotExample {
    using System;
    using System.Collections;
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
            var returnTable = new DataTable();            

            var columnDefinitions = new Dictionary<Tuple<string,string>,ColumnConfiguration>();
            var configurationDictionary = configuration.ToDictionary(c => c.Name);
            FetchData(reader, configurationDictionary, columnDefinitions, returnTable);
            Console.WriteLine("FetchData: {0}", watch.ElapsedMilliseconds);

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
        /// <param name="returnTable">The DataTable where generated columns are added.</param>
        /// <returns>A List of DataObject that contains each row's data.</returns>
        private static void FetchData(IDataReader reader, IDictionary<string, ColumnConfiguration> configuration, IDictionary<Tuple<string, string>, ColumnConfiguration> columns, DataTable returnTable) {
            IDictionary<string, DataColumn> columnCache = new Dictionary<string, DataColumn>();
            IDictionary<string, DataRow> rowCache = new Dictionary<string, DataRow>();
            
            //Add first data column by which all values are grouped by
            var primaryKeyColumn = new ColumnConfiguration("PARTID", "Part", 0, typeof(string));
            columns.Add(Tuple.Create("PARTID", "PARTID"), primaryKeyColumn);
            CreateTableDefinition(returnTable, primaryKeyColumn, columnCache);

            while (reader.Read()) {
                var dataObject = new DataObject(reader);
                var columnDefintion = GetColumns(columns, configuration, dataObject);
                if (columnDefintion != null)
                    CreateTableDefinition(returnTable, columnDefintion, columnCache);
                CalculatePivot(returnTable, dataObject, rowCache, columnCache);
            }
        }

        /// <summary>
        /// Create a column definition for each data row combined with configuration data.
        /// </summary>
        /// <param name="columns">The collection of the currently available columns. New definitions are added here.</param>
        /// <param name="configuration">The definition in which order and what columns to return.</param>
        /// <param name="dataEntry">The entry that is used to create additional columns.</param>
        /// <returns>A List of column definitions.</returns>
        private static ColumnConfiguration GetColumns(IDictionary<Tuple<string, string>, ColumnConfiguration> columns, IDictionary<string, ColumnConfiguration> configuration, DataObject dataEntry) {
            if (configuration.ContainsKey(dataEntry.ValueId)) {
                var configurationEntry = configuration[dataEntry.ValueId];
                string displayName = configurationEntry.DisplayName;
                var key = Tuple.Create(dataEntry.ValueId, displayName);
                if (!columns.ContainsKey(key)) {
                    columns.Add(key, configurationEntry);
                    return columns[key];
                }
            }
            return null;
        }

        /// <summary>
        /// Converts a List of ColumnDefinition into a DataTable
        /// </summary>
        /// <param name="dataTable">The DataTable where the generated column is added.</param>
        /// <param name="columnDefinition">The information that is used to create the columnDefinition.</param>
        /// <param name="columnCache"></param>
        private static void CreateTableDefinition(DataTable dataTable, ColumnConfiguration columnDefinition, IDictionary<string, DataColumn> columnCache) {
            var column = dataTable.Columns.Add(columnDefinition.Name, columnDefinition.DataType);
            column.Caption = columnDefinition.DisplayName;
            columnCache.Add(columnDefinition.DisplayName, column);
        }

        /// <summary>
        /// Pivoting queried data.
        /// </summary>
        /// <param name="returnTable">The DataTable that should be filled with data.</param>
        /// <param name="dataEntry">The data that is used to fill the DataTable.</param>
        /// <param name="configuration">The column configuration that was used to create the datatable.</param>
        /// <param name="dataRowCache">A dictionary that is used to track primary key information.</param>
        /// <param name="columnCache"></param>
        private static void CalculatePivot(DataTable returnTable, DataObject dataEntry, IDictionary<string, DataRow> dataRowCache, IDictionary<string, DataColumn> columnCache) {
            DataRow row;
            if (!dataRowCache.ContainsKey(dataEntry.PartId)) {
                row = returnTable.NewRow();
                row["PARTID"] = dataEntry.PartId;
                returnTable.Rows.Add(row);
                dataRowCache.Add(dataEntry.PartId, row);
            } else {
                row = dataRowCache[dataEntry.PartId];
            }

            string columnName = dataEntry.ValueId;

            if (columnCache.ContainsKey(columnName))
                row[columnName] = dataEntry.Value;
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
