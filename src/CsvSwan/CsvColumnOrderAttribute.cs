namespace CsvSwan
{
    using System;

    /// <summary>
    /// Indicates which column to use to map a  property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CsvColumnOrderAttribute : Attribute
    {
        /// <summary>
        /// The 0-indexed column index for the column to use to map this property.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Create a new <see cref="CsvColumnOrderAttribute"/>.
        /// </summary>
        public CsvColumnOrderAttribute(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }
    }
}
