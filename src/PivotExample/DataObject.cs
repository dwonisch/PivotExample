namespace PivotExample {
    using System.Data;

    public class DataObject {
        public string PartId { get; private set; }
        public string ValueId { get; private set; }
        public string Value { get; private set; }

        public DataObject(IDataReader reader) {
            PartId = (string)reader["PARTID"];
            ValueId = (string)reader["VALUEID"];
            Value = (string)reader["VALUE"];
        }
    }
}