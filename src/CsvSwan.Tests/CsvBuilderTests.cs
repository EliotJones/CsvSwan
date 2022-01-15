using Xunit;

namespace CsvSwan.Tests
{
    public class CsvBuilderTests
    {
        [Fact]
        public void CreateWithHeaderOnly()
        {
            var builder = Csv.Create(new CsvBuilder.Options {NewLine = "\n"});

            builder.WithHeaders("Onion", "Backpack", "Id", "Olive \"Oil\"");

            var result = builder.ToString();

            Assert.Equal("\"Onion\",\"Backpack\",\"Id\",\"Olive \\\"Oil\\\"\"\n", result);

            builder.ChangeOptions(new CsvBuilder.Options
            {
                EndWithNewline = false,
                QuoteAllFields = false
            });

            result  = builder.ToString();

            Assert.Equal("Onion,Backpack,Id,\"Olive \\\"Oil\\\"\"", result);

            builder.ChangeOptions(new CsvBuilder.Options
            {
                EndWithNewline = true,
                QuoteAllFields = true,
                QuoteCharacter = '\'',
                NewLine = "\r\n",
                Separator = '\t'
            });

            result = builder.ToString();

            Assert.Equal("'Onion'\t'Backpack'\t'Id'\t'Olive \"Oil\"'\r\n", result);
        }
    }
}
