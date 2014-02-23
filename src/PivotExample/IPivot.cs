namespace PivotExample {
    using System.Collections.Generic;
    using System.Data;

    public interface IPivot {
        DataTable CreatePivot(IDataReader reader, IList<ColumnConfiguration> configuration);
    }
}