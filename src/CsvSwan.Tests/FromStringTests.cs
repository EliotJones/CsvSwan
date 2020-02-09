namespace CsvSwan.Tests
{
    using System.Linq;
    using Xunit;

    public class FromStringTests
    {
        private const string SimplestInput = @"a string, another one,1.433,simple
we have,four columns,42.564,that's all 2 rows";

        [Fact]
        public void RowsHaveCorrectCount()
        {
            using (var csv = Csv.FromString(SimplestInput))
            {
                Assert.Equal(2, csv.Rows.Count());
            }
        }

        [Fact]
        public void RowsHaveCorrectValues()
        {
            using (var csv = Csv.FromString(SimplestInput))
            {
                var rows = csv.Rows.Select(x => x.GetValues().ToList()).ToList();

                Assert.Equal(new[]{ "a string", "another one", "1.433", "simple" }, rows[0]);
                Assert.Equal(new[]{ "we have", "four columns", "42.564", "that's all 2 rows" }, rows[1]);
            }
        }
    }
}
