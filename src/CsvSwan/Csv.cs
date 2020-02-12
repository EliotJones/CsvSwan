﻿namespace CsvSwan
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

        public static Csv Open(string filename, char separator = ',') => new Csv(File.OpenRead(filename), CsvOptions.WithSeparator(separator), true);
        public static Csv Open(string filename, CsvOptions options) => new Csv(File.OpenRead(filename), options, true);
        public static Csv Open(Stream fileStream, char separator = ',') => new Csv(fileStream, CsvOptions.WithSeparator(separator), false);
        public static Csv Open(byte[] fileBytes, char separator = ',') => new Csv(new MemoryStream(fileBytes), CsvOptions.WithSeparator(separator), true);
        public static Csv Open(Stream fileStream, CsvOptions options) => new Csv(fileStream, options, false);

        public static Csv FromString(string value, char separator = ',', bool hasHeaderRow = false) => FromString(value, new CsvOptions
        {
            Separator = separator,
            Encoding = Encoding.Unicode,
            HasHeaderRow = hasHeaderRow
        });

        public static Csv FromString(string value, CsvOptions options)
        {
            var encoding = options.Encoding ?? Encoding.UTF8;
            return new Csv(new MemoryStream(encoding.GetBytes(value)), options, true);
        }

        #endregion

        private readonly CsvReader reader;
        private readonly Stream stream;
        private readonly bool canDisposeStream;

        private IReadOnlyList<string> currentValues;
        private int rowIndex = -1;
        private readonly RowAccessor accessor;

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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

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
            this.canDisposeStream = canDisposeStream;

            reader = new CsvReader(this.stream, options);
            accessor = new RowAccessor(this);

            if (options.HasHeaderRow)
            {
                HeaderRow = reader.GetHeaderRow();
            }
            else
            {
                HeaderRow = Array.Empty<string>();
            }
        }

        public IReadOnlyList<IReadOnlyList<string>> GetAllRowValues()
        {
            var result = new List<IReadOnlyList<string>>();

            foreach (var row in Rows)
            {
                result.Add(row.GetValues());
            }

            return result;
        }

        public IEnumerable<T> MapRows<T>()
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

            public short GetShort(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return short.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            public int GetInt(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return int.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            public long GetLong(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return long.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            public decimal GetDecimal(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return decimal.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

            public double GetDouble(int index, IFormatProvider formatProvider = null)
            {
                GuardIndex(index);
                return double.Parse(csv.currentValues[index], NumberStyles.Number, formatProvider ?? CultureInfo.InvariantCulture);
            }

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

            public T Map<T>(IFormatProvider formatProvider = null)
            {
                lock (mutex)
                {
                    if (!typeFactories.TryGetValue(typeof(T), out var factory))
                    {
                        factory = TypeMapFactory.Create<T>();
                        typeFactories[typeof(T)] = factory;
                    }

                    return (T)factory.Map(this, formatProvider);
                }
            }
        }
    }
}
