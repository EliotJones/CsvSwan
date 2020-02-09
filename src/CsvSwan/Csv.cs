namespace CsvSwan
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
                var offset = reader.Position;
                while (reader.ReadRow(out currentValues))
                {
                    yield return accessor;

                    offset = reader.Position;
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

        public void Dispose()
        {
            if (canDisposeStream)
            {
                reader.Dispose();
            }
        }

        public class RowAccessor
        {
            private readonly Csv csv;

            internal RowAccessor(Csv csv)
            {
                this.csv = csv ?? throw new ArgumentNullException(nameof(csv));
            }

            public IReadOnlyList<string> GetValues()
            {
                return new List<string>(csv.currentValues);
            }
        }
    }
}
