namespace CsvSwan.Tests
{
    using System.Linq;
    using Xunit;

    public class SqliteTests
    {
        [Fact]
        public void PipeSeparatedWithQuotes()
        {
            using (var csv = Csv.Open(TestHelpers.GetDocumentPath("sqlite-pipe-quotes.csv"), '|', true))
            {
                var values = csv.Map<AdjustmentRecord>().ToList();

                Assert.Equal(6, values.Count);

                Assert.Equal(1, values[0].Id);
                Assert.Equal(-5.67m, values[0].Adjustment);
                Assert.Equal("NQS", values[0].Type);

                Assert.Equal(2, values[1].Id);
                Assert.Equal(3.257m, values[1].Adjustment);
                Assert.Equal("EDG \"Any\"", values[1].Type);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class AdjustmentRecord
        {
            public int Id { get; set; }

            public decimal Adjustment { get; set; }

            public string Type { get; set; }
        }
    }
}