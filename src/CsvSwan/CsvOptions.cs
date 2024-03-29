﻿namespace CsvSwan
{
    using System.Text;

    /// <summary>
    /// Options defining how to open/parse a <see cref="Csv"/>.
    /// </summary>
    public class CsvOptions
    {
        /// <summary>
        /// A tab-separated file with no header row.
        /// </summary>
        public static CsvOptions TabSeparated { get; } = new CsvOptions
        {
            Separator = '\t'
        };

        /// <summary>
        /// A comma-separated file with no header row.
        /// </summary>
        public static CsvOptions CommaSeparated { get; } = new CsvOptions
        {
            Separator = ','
        };

        /// <summary>
        /// The separator character between fields.
        /// Default comma (',').
        /// </summary>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// The character used to quote text fields if <see cref="AreTextFieldsQuoted"/>
        /// is <see langword="true"/>.
        /// Default double quote ('"').
        /// </summary>
        public char QuotationCharacter { get; set; } = '\"';

        /// <summary>
        /// Whether text fields are surrounded with quotes.
        /// Default <see langword="true"/>.
        /// </summary>
        public bool AreTextFieldsQuoted { get; set; } = true;

        /// <summary>
        /// Whether the file starts with a row defining the column names.
        /// Default <see langword="false"/>.
        /// </summary>
        public bool HasHeaderRow { get; set; }

        /// <summary>
        /// The encoding to interpret the CSV with.
        /// </summary>
        public Encoding Encoding { get; set; } = null;

        /// <summary>
        /// The backslash character can be used to escape quote marks inside fields.
        /// Default <see langword="false"/>.
        /// </summary>
        public bool BackslashEscapesQuotes { get; set; } = false;

        /// <summary>
        /// For non-nullable types like <see langword="int" />s, <see langword="double" />s, <see langword="DateTime" />s
        /// use a default value when <see langword="null" /> is encountered.
        /// </summary>
        public bool DefaultNullValues { get; set; } = true;

        /// <summary>
        /// Create a new <see cref="CsvOptions"/> with the specified separator.
        /// </summary>
        public static CsvOptions WithSeparator(char separator, bool hasHeaderRow = false)
        {
            return new CsvOptions
            {
                Separator = separator,
                AreTextFieldsQuoted = true,
                HasHeaderRow = hasHeaderRow,
                DefaultNullValues = true
            };
        }
    }
}
