using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace CsvSwan
{
    /// <summary>
    /// Used to build a new CSV file.
    /// </summary>
    public class CsvBuilder
    {
        private Options options;

        private IReadOnlyList<string> headers;

        private readonly List<IReadOnlyList<object>> rows = new List<IReadOnlyList<object>>();

        /// <summary>
        /// Create a new <see cref="CsvBuilder"/> with the specified or default <see cref="Options"/>.
        /// </summary>
        public CsvBuilder(Options options = null)
        {
            this.options = options ?? new Options();
        }

        /// <summary>
        /// Change the options configured on this builder.
        /// </summary>
        public void ChangeOptions(Options options)
        {
            this.options = options ?? this.options;
        }

        /// <summary>
        /// Add headers to the output, a single header row will be written with the headers in the order provided.
        /// Replaces any previously set headers.
        /// </summary>
        public CsvBuilder WithHeaders(string header1, string header2)
        {
            WithHeaders((IReadOnlyList<string>)new[] { header1, header2 });

            return this;
        }

        /// <summary>
        /// Add headers to the output, a single header row will be written with the headers in the order provided.
        /// Replaces any previously set headers.
        /// </summary>
        public CsvBuilder WithHeaders(params string[] headers)
        {
            this.headers = headers;

            return this;
        }

        /// <summary>
        /// Add headers to the output, a single header row will be written with the headers in the order provided.
        /// Replaces any previously set headers.
        /// </summary>
        public CsvBuilder WithHeaders(IReadOnlyList<string> headers)
        {
            this.headers = headers;

            return this;
        }

        /// <summary>
        /// Clear any headers, no header row will be written to the output.
        /// </summary>
        public CsvBuilder RemoveHeaders()
        {
            headers = null;

            return this;
        }

        /// <summary>
        /// Add a new row to the collection in this CSV containing the strings in the order provided.
        /// </summary>
        public CsvBuilder AddRow(IReadOnlyList<string> values)
        {
            if (values == null)
            {
                return this;
            }

            rows.Add(values);

            return this;
        }

        /// <summary>
        /// Add a new row to the collection in this CSV containing the values in the order provided.
        /// </summary>
        public CsvBuilder AddRow(IReadOnlyList<object> values)
        {
            if (values == null)
            {
                return this;
            }

            rows.Add(values);

            return this;
        }

        /// <summary>
        /// Add new rows to the collection in this CSV containing the strings in the order provided.
        /// </summary>
        public CsvBuilder AddRows(IEnumerable<IReadOnlyList<string>> values)
        {
            if (values == null)
            {
                return this;
            }

            foreach (var value in values)
            {
                if (value == null)
                {
                    continue;
                }

                rows.Add(value);
            }

            return this;
        }

        /// <summary>
        /// Add new rows to the collection in this CSV containing the values in the order provided.
        /// </summary>
        public CsvBuilder AddRows(IEnumerable<IReadOnlyList<object>> values)
        {
            if (values == null)
            {
                return this;
            }

            foreach (var value in values)
            {
                if (value == null)
                {
                    continue;
                }

                rows.Add(value);
            }

            return this;
        }

        /// <summary>
        /// Write the CSV to a byte array using the specified encoding. If encoding is not provided it defaults to <see cref="Encoding.Unicode"/>.
        /// </summary>
        public byte[] ToBytes(Encoding encoding = null)
        {
            var str = ToString();
            return (encoding ?? Encoding.Unicode).GetBytes(str);
        }

        /// <summary>
        /// Copy/write the CSV as bytes to the provided stream using the specified encoding. If encoding is not provided it defaults to <see cref="Encoding.Unicode"/>.
        /// </summary>
        public void CopyToStream(Stream stream, Encoding encoding = null)
        {
            var bytes = ToBytes(encoding);
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            void Newline()
            {
                sb.Append(options.NewLine);
            }

            if (headers != null && headers.Count > 0)
            {
                for (var i = 0; i < headers.Count; i++)
                {
                    var val = headers[i];
                    var header = SafeEscape(val);

                    if (header != val || options.QuoteAllFields)
                    {
                        sb.Append(options.QuoteCharacter).Append(header).Append(options.QuoteCharacter);
                    }
                    else
                    {
                        sb.Append(header);
                    }

                    if (i < headers.Count - 1)
                    {
                        sb.Append(options.Separator);
                    }
                }

                if (options.EndWithNewline || rows.Count > 0)
                {
                    Newline();
                }
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                for (var j = 0; j < row.Count; j++)
                {
                    var field = row[j];

                    PrintField(field, sb);

                    if (j < row.Count - 1)
                    {
                        sb.Append(options.Separator);
                    }
                }

                if (i < rows.Count - 1 || options.EndWithNewline)
                {
                    Newline();
                }
            }

            return sb.ToString();
        }

        private void PrintField(object field, StringBuilder stringBuilder)
        {
            if (field == null)
            {
                return;
            }

            if (field is string str)
            {
                stringBuilder.Append(options.QuoteCharacter).Append(SafeEscape(str)).Append(options.QuoteCharacter);
            }

            //string valueString;
            //switch (field)
            //{
            //    case bool boo:
            //        valueString = boo.ToString(options.Culture);
            //        break;
            //    case byte b:
            //        valueString = b.ToString(options.Culture);
            //        break;
            //    case ushort us:
            //        valueString = us.ToString("");
            //        break;
            //    case short s:
            //        break;
            //    case uint ui:
            //        break;
            //    case int i:
            //        break;
            //    case ulong ul:
            //        break;
            //    case long l:
            //        break;
            //    case float fl:
            //        break;
            //    case double db:
            //        break;
            //    case decimal d:
            //        break;
            //    case DateTime dt:
            //        break;
            //}

            var strVal = Convert.ToString(field, options.Culture);

            var safeStrVal = SafeEscape(strVal);

            if (strVal != safeStrVal || options.QuoteAllFields)
            {
                stringBuilder.Append(options.QuoteCharacter).Append(safeStrVal).Append(options.QuoteCharacter);
            }
            else
            {
                stringBuilder.Append(safeStrVal);
            }
        }

        private string SafeEscape(string input)
        {
            var escape = options.UseBackslashEscape ? $"\\{options.QuoteCharacter}" : new string(options.QuoteCharacter, 2);

            return input.Replace(options.QuoteCharacter.ToString(), escape);
        }

        /// <summary>
        /// Options controlling the formatting of the output CSV.
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Whether all fields in the output should be surrounded by quotes. Defaults to <see langword="true"/>.
            /// </summary>
            public bool QuoteAllFields { get; set; } = true;

            /// <summary>
            /// The culture used to format the output, e.g. for numbers/dates. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
            /// </summary>
            public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

            /// <summary>
            /// The character used to separate fields. Defaults to ','.
            /// </summary>
            public char Separator { get; set; } = ',';

            /// <summary>
            /// Whether to escape quote characters in field values with a backslash (when set to <see langword="true"/>)
            /// or by doubling the character.
            /// If true '"' becomes '\"'. If false '"' becomes '""'.
            /// Defaults to <see langword="true"/>.
            /// </summary>
            public bool UseBackslashEscape { get; set; } = true;

            /// <summary>
            /// The character used to quote text fields, or all fields if <see cref="QuoteAllFields"/> is <see langword="true"/>.
            /// Defaults to '"'.
            /// </summary>
            public char QuoteCharacter { get; set; } = '"';

            /// <summary>
            /// The character or characters used for newlines, defaults to '\r\n'.
            /// </summary>
            public string NewLine { get; set; } = "\r\n";

            /// <summary>
            /// Whether the last line of the file should end with the <see cref="NewLine"/>.
            /// Defaults to <see langword="true"/>.
            /// </summary>
            public bool EndWithNewline { get; set; } = true;
        }
    }
}
