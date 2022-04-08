using System;

namespace CsvSwan
{
    /// <summary>
    /// Indicates which column to use to map a  property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CsvColumnNameAttribute : Attribute
    {
        /// <summary>
        /// The case-insensitive column name to use to map this property.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Create a new <see cref="CsvColumnNameAttribute"/>.
        /// </summary>
        public CsvColumnNameAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}
