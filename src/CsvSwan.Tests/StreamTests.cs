namespace CsvSwan.Tests
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class StreamTests
    {
        private const string SimplestInput = @"a string, another one,1.433,simple
we have,four columns,42.564,that's all 2 rows";

        [Fact]
        public void KeepsStreamOpen()
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(SimplestInput)))
            {
                using (var csv = Csv.Open(memoryStream))
                {
                    var rows = csv.Rows.Select(x => x.GetValues()).ToList();

                    Assert.Equal(2, rows.Count);

                    TestHelpers.RowMatch(rows[0], "a string", "another one", "1.433", "simple");
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                Assert.Equal(0, memoryStream.Position);
            }
        }
    }
}