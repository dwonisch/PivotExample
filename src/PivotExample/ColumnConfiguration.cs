namespace PivotExample {
    using System;

    public class ColumnConfiguration {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public int SortOrder { get; private set; }
        public Type DataType { get; private set; }

        public ColumnConfiguration(string name, string displayName, int sortOrder, Type dataType) {
            Name = name;
            DisplayName = displayName;
            SortOrder = sortOrder;
            DataType = dataType;
        }
    }
}