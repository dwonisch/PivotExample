namespace PivotExample {
    using System;

    public class ColumnDefinition {
        public string DisplayName { get; private set; }
        public int SortOrder { get; private set; }
        public Type DataType { get; private set; }

        public ColumnDefinition(string displayName, int sortOrder, Type dataType) {
            DisplayName = displayName;
            SortOrder = sortOrder;
            DataType = dataType;
        }
    }
}