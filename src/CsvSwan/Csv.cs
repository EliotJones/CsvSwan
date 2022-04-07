using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CsvSwan.Tests")]

namespace CsvSwan
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <inheritdoc />
    /// <summary>
    /// Provides access to the data in a CSV file. Use one of the static methods to open a CSV file, for example
    /// <see cref="M:CsvSwan.Csv.Open(System.String,System.Char)" />
    /// </summary>
    public class Csv : IDisposable
    {
        #region Static Factories

        /// <summary>
        /// Open a <see cref="Csv"/> from a file at the given path.
        /// </summary>
        public static Csv Open(string filename, char separator = ',', bool hasHeaderRow = false) => new Csv(File.OpenRead(filename), CsvOptions.WithSeparator(separator, hasHeaderRow), true);
        /// <summary>
        /// Open a <see cref="Csv"/> from a file at the given path.
        /// </summary>
        public static Csv Open(string filename, CsvOptions options) => new Csv(File.OpenRead(filename), options, true);
        /// <summary>
        /// Open a <see cref="Csv"/> from the provided stream.
        /// </summary>
        public static Csv Open(Stream fileStream, char separator = ',', bool hasHeaderRow = false) => new Csv(fileStream, CsvOptions.WithSeparator(separator, hasHeaderRow), false);
        /// <summary>
        /// Open a <see cref="Csv"/> from the provided bytes.
        /// </summary>
        public static Csv Open(byte[] fileBytes, char separator = ',', bool hasHeaderRow = false) => new Csv(new MemoryStream(fileBytes), CsvOptions.WithSeparator(separator, hasHeaderRow), true);
        /// <summary>
        /// Open a <see cref="Csv"/> from the provided stream.
        /// </summary>
        public static Csv Open(Stream fileStream, CsvOptions options) => new Csv(fileStream, options, false);
        /// <summary>
        /// Open a <see cref="Csv"/> from the input string.
        /// </summary>
        public static Csv FromString(string value, char separator = ',', bool hasHeaderRow = false) => FromString(value, new CsvOptions
        {
            Separator = separator,
            Encoding = Encoding.Unicode,
            HasHeaderRow = hasHeaderRow
        });

        /// <summary>
        /// Open a <see cref="Csv"/> from the input string.
        /// </summary>
        public static Csv FromString(string value, CsvOptions options)
        {
            var encoding = options.Encoding ?? Encoding.UTF8;
            return new Csv(new MemoryStream(encoding.GetBytes(value)), options, true);
        }

        /// <summary>
        /// Create a <see cref="CsvBuilder"/> for generating a new CSV.
        /// </summary>
        internal static CsvBuilder Create(CsvBuilder.Options options = null)
        {
            return new CsvBuilder(options);
        }

        #endregion

        private readonly CsvReader reader;
        private readonly Stream stream;
        private readonly bool canDisposeStream;

        private IReadOnlyList<string> currentValues;
        private int rowIndex = -1;
        private readonly RowAccessor accessor;
        private readonly CsvOptions options;

        /// <summary>
        /// Get the header row values for this file. If there is no header row (<see cref="CsvOptions.HasHeaderRow"/> is
        /// <see langword="false"/>) then this is an empty list.
        /// </summary>
        public IReadOnlyList<string> HeaderRow { get; }

        /// <summary>
        /// Step through the rows in this CSV. <see cref="RowAccessor"/> should not be stored.
        /// </summary>
        public IEnumerable<RowAccessor> Rows
        {
            get
            {
                reader.SeekStart(true);
                rowIndex = -1;

                while (reader.ReadRow(out currentValues))
                {
                    rowIndex++;
                    yield return accessor;
                }
            }
        }

        private Csv(Stream stream, CsvOptions options, bool canDisposeStream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException($"Cannot read from a stream of type: {stream.GetType().FullName}.", nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException($"Cannot seek in a stream of type: {stream.GetType().FullName}.", nameof(stream));
            }

            this.stream = stream;
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.canDisposeStream = canDisposeStream;

            reader = new CsvReader(this.stream, options);
            accessor = new RowAccessor(this);

            if (options.HasHeaderRow)
            {
                HeaderRow = reader.GetHeaderRow();
            }
            else
            {
                HeaderRow = new string[0];
            }
        }

        /// <summary>
        /// Gets the set of values for all rows in the file.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> GetAllRowValues()
        {
            var result = new List<IReadOnlyList<string>>();

            foreach (var row in Rows)
            {
                result.Add(row.GetValues());
            }

            return result;
        }

        /// <summary>
        /// Map each row in the CSV file to an object of type T.
        /// Columns can be mapped to properties using the <see cref="CsvColumnOrderAttribute"/>
        /// to specify a column index for a property on the input type. Alternatively if the CSV
        /// contains a header row the properties will be matched to the columns using a case insensitive
        /// lookup. <see cref="CsvColumnOrderAttribute"/> takes precedence over header column names, if present.
        /// Finally for a type without attributes in a CSV without a header row, the property order is used, this
        /// is generally the declaration order for properties but this is not (always) deterministic.
        /// </summary>
        /// <typeparam name="T">The type of object to map to.</typeparam>
        /// <returns>An enumerable of instances of type T.</returns>
        public IEnumerable<T> Map<T>() where T : class
        {
            foreach (var row in Rows)
            {
                yield return row.Map<T>();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            reader.Dispose();

            if (canDisposeStream)
            {
                stream?.Dispose();
            }
        }

        /// <summary>
        /// Provides access to values in the currently loaded row.
        /// References to this should not be stored.
        /// </summary>
        public class RowAccessor
        {
            private readonly Csv csv;

            private readonly object mutex = new object();
            private readonly Dictionary<Type, TypeMapFactory> typeFactories = new Dictionary<Type, TypeMapFactory>();

            internal RowAccessor(Csv csv)
            {
                this.csv = csv ?? throw new ArgumentNullException(nameof(csv));
            }

            /// <summary>
            /// Get all values in the row as <see langword="string"/>s.
            /// </summary>
            /// <returns>A snapshot of the values for this row.</returns>
            public IReadOnlyList<string> GetValues()
            {
                return new List<string>(csv.currentValues);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="short"/>.
            /// </summary>
            public short GetShort(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return short.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as an <see langword="int"/>.
            /// </summary>
            public int GetInt(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return int.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as an <see langword="int?"/>.
            /// </summary>
            public int? GetNullableInt(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);

                var value = csv.currentValues[index];

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                return int.Parse(value, NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="long"/>.
            /// </summary>
            public long GetLong(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return long.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="long?"/>.
            /// </summary>
            public long? GetNullableLong(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);

                var value = csv.currentValues[index];

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                return long.Parse(value, NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="decimal"/>.
            /// </summary>
            public decimal GetDecimal(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return decimal.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="decimal?"/>.
            /// </summary>
            public decimal? GetNullableDecimal(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);

                var value = csv.currentValues[index];

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                return decimal.Parse(value, NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="double"/>.
            /// </summary>
            public double GetDouble(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return double.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="double?"/>.
            /// </summary>
            public double? GetNullableDouble(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);

                var value = csv.currentValues[index];

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                return double.Parse(value, NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="float"/>.
            /// </summary>
            public float GetFloat(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return float.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the value from the column at the given index as a <see langword="float?"/>.
            /// </summary>
            public float? GetNullableFloat(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);

                var value = csv.currentValues[index];

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                return float.Parse(value, NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the <see langword="string" /> value from the column at the given index.
            /// </summary>
            public string GetString(int index)
            {
                GuardIndex(index);
                return csv.currentValues[index];
            }

            private void GuardIndex(int index)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index cannot be negative, got: {index}. For row {csv.rowIndex}.");
                }

                if (index >= csv.currentValues.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index was out of range, the maximum index value is {csv.currentValues.Count - 1}. For row {csv.rowIndex}.");
                }
            }

            /// <summary>
            /// Map the row to a type T using either <see cref="CsvColumnOrderAttribute"/>s or the header row.
            /// </summary>
            public T Map<T>(IFormatProvider formatProvider = null) where T : class
            {
                lock (mutex)
                {
                    if (!typeFactories.TryGetValue(typeof(T), out var factory))
                    {
                        factory = TypeMapFactory.Create<T>(csv.options.HasHeaderRow ? csv.HeaderRow : null);
                        typeFactories[typeof(T)] = factory;
                    }

                    return (T)factory.Map(this, formatProvider);
                }
            }
        }
    }
}
