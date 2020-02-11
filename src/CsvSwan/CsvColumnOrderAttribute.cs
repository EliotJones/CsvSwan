namespace CsvSwan
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CsvColumnOrderAttribute : Attribute
    {
        public int ColumnIndex { get; set; }

        public CsvColumnOrderAttribute(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }
    }
}
