namespace CsvSwan
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class Csv : IDisposable
    {
        public static Csv Open(string filename, char separator = ',') => new Csv(File.OpenRead(filename), CsvOptions.WithSeparator(separator), true);
        public static Csv Open(Stream fileStream, char separator = ',') => new Csv(fileStream, CsvOptions.WithSeparator(separator), false);
        public static Csv Open(byte[] fileBytes, char separator = ',') => new Csv(new MemoryStream(fileBytes), CsvOptions.WithSeparator(separator), true);
        public static Csv Open(Stream fileStream, CsvOptions options) => new Csv(fileStream, options, false);
        public static Csv FromString(string value, char separator = ',') => FromString(value, CsvOptions.WithSeparator(separator));
        public static Csv FromString(string value, CsvOptions options) => new Csv(new MemoryStream(Encoding.UTF8.GetBytes(value)), options, true);

        private readonly CsvReader reader;
        private readonly bool canDisposeStream;

        private IReadOnlyList<string> currentValues;
        private readonly RowAccessor accessor;

        public IEnumerable<RowAccessor> Rows
        {
            get
            {
                reader.SeekStart();
                while (reader.ReadRow(out currentValues))
                {
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
                throw new InvalidOperationException($"Cannot read from a stream of type: {stream.GetType()?.FullName}.");
            }

            reader = new CsvReader(stream, options);
            accessor = new RowAccessor(this);
            this.canDisposeStream = canDisposeStream;
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

        public void Dispose()
        {
            if (canDisposeStream)
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Provides access to values in the currently loaded row.
        /// References to this should not be stored.
        /// </summary>
        public class RowAccessor
        {
            private readonly Csv csv;

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
        }
    }
}
