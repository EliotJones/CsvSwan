namespace CsvSwan.Tests
{
    using Xunit;

    public class LibreOfficeTests
    {
        [Fact]
        public void CommaSeparatedWithQuotes()
        {
            var filePath = TestHelpers.GetDocumentPath("libre-office-comma-quotes.csv");
            using (var csv = Csv.Open(filePath))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(11, rows.Count);

                TestHelpers.RowMatch(rows[0], "10/02/2020", "A573", "Beverage", "Yes", "1");
                TestHelpers.RowMatch(rows[1], "11/02/2020", "A584", "Beverage, other", "Yes", "1");
            }
        }
    }
}
