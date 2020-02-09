namespace CsvSwan
{
    using System.Text;

    public class CsvOptions
    {
        public static CsvOptions TabSeparated { get; } = new CsvOptions
        {
            Separator = '\t'
        };

        public static CsvOptions CommaSeparated { get; } = new CsvOptions
        {
            Separator = ','
        };

        /// <summary>
        /// The separator character between fields, defaults to comma ','.
        /// </summary>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// The character used to quote text fields if <see cref="AreTextFieldsQuoted"/>
        /// is <see langword="true"/>. Defaults to double quote '"'.
        /// </summary>
        public char QuotationCharacter { get; set; } = '\"';

        /// <summary>
        /// Whether text fields are surrounded with quotes.
        /// </summary>
        public bool AreTextFieldsQuoted { get; set; }

        /// <summary>
        /// Whether the file starts with a row defining the column names.
        /// </summary>
        public bool IncludesHeaderRow { get; set; }

        /// <summary>
        /// The encoding to interpret the CSV with.
        /// </summary>
        public Encoding Encoding { get; set; } = null;

        /// <summary>
        /// Create a new <see cref="CsvOptions"/> with the specified separator.
        /// </summary>
        public static CsvOptions WithSeparator(char separator)
        {
            return new CsvOptions
            {
                Separator = separator
            };
        }
    }
}