using System;
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

        [Fact]
        public void CreateCsvWithRows()
        {
            var builder = Csv.Create();

            builder.WithHeaders("Id", "Cost", "Created", "Name");

            builder.AddRow(new object[] {512, 6.70m, new DateTime(2021, 5, 16, 13, 42, 16, DateTimeKind.Utc), "Algonquin"});
            builder.AddRow(new object[] {164323, 12221.23, new DateTime(2021, 5, 16, 13, 55, 20, DateTimeKind.Utc), "Richard"});

            var result = builder.ToString();

            var expected = @"""Id"",""Cost"",""Created"",""Name""
""512"",""6.70"",""05/16/2021 13:42:16"",""Algonquin""
""164323"",""12221.23"",""05/16/2021 13:55:20"",""Richard""
";

            Assert.Equal(expected, result);
        }
    }
}
