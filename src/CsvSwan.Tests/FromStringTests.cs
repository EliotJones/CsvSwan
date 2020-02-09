namespace CsvSwan.Tests
{
    using System.Collections.Generic;
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

        [Fact]
        public void RowsCanBeEnumeratedMultipleTimes()
        {
            using (var csv = Csv.FromString(SimplestInput))
            {
                var rows1 = csv.Rows.Select(x => x.GetValues()).ToList();
                var rows2 = csv.Rows.Select(x => x.GetValues()).ToList();

                Assert.Equal(rows1[0], rows2[0]);
                Assert.Equal(rows1[1], rows2[1]);
            }
        }

        [Fact]
        public void HandlesEmptyValues()
        {
            const string input = @"ham, egg, ,cheese
,,,
cabbage,port,mushroom,elixir";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(3, rows.Count);

                Assert.Equal(new[]{ "ham", "egg", string.Empty, "cheese" }, rows[0]);
                Assert.Equal(new[]{ string.Empty, string.Empty, string.Empty, string.Empty }, rows[1]);
                Assert.Equal(new[]{ "cabbage", "port", "mushroom", "elixir" }, rows[2]);
            }
        }

        [Fact]
        public void HandlesQuotedValues()
        {
            const string input = "\"value 1\", \"value, comma\", no quote\r\n\"quoted\", not quoted, 7\r\n\r\n";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(2, rows.Count);

                Assert.Equal(new[]{ "value 1", "value, comma", "no quote" }, rows[0]);
                Assert.Equal(new[]{ "quoted", "not quoted", "7" }, rows[1]);
            }
        }

        [Fact]
        public void HandlesUnixLineBreaks()
        {
            const string input = "7556,546,harp\n534,778,lute\n788,0.656,trombone\n,,";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(4, rows.Count);

                Assert.Equal(new[]{ "7556", "546", "harp" }, rows[0]);
                Assert.Equal(new[]{ "534", "778", "lute" }, rows[1]);
                Assert.Equal(new[]{ "788", "0.656", "trombone" }, rows[2]);
                Assert.Equal(new[]{ string.Empty, string.Empty, string.Empty }, rows[3]);
            }
        }

        [Fact]
        public void HandlesRfc4180EscapedQuotesSingleQuoteOnly()
        {
            const string input = "\"q\"\"\",a\r\n1,2";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(2, rows.Count);
                RowMatch(rows[0], @"q""", "a");
                RowMatch(rows[1], "1", "2");
            }
        }

        [Fact]
        public void HandlesRfc4180EscapedQuotesFollowedByText()
        {
            const string input = "\"\"\"<\",1\r\n,";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(2, rows.Count);

                RowMatch(rows[0], @"""<", "1");
                RowMatch(rows[1], string.Empty, string.Empty);
            }
        }

        [Fact]
        public void HandlesRfc4180EmptyQuotedString()
        {
            const string input = "\"\",1";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(1, rows.Count);

                RowMatch(rows[0], string.Empty, "1");
            }
        }
        
        [Fact]
        public void HandlesRfc4180QuotedStringWithDoubleQuoteOnly()
        {
            const string doubleQuote = "\"\"";
            var input = $"\"{doubleQuote}{doubleQuote}\", 1";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(1, rows.Count);

                RowMatch(rows[0], doubleQuote, "1");
            }
        }

        [Fact]
        public void HandlesRfc4180EscapedQuotesComplex()
        {
            const string input = "\"A field with a \"\"quote\"\"\",field2\r\nfield 1,\"quoted field,\"";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(2, rows.Count);
                RowMatch(rows[0], "A field with a \"quote\"", "field2");
                RowMatch(rows[1], "field 1", "quoted field,");
            }
        }

        [Fact]
        public void HandlesBackslashEscapedQuotes()
        {
            const string input = "\"quote \\\"and\"\" rfc-4180 double\", field a\r\n1,2";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(2, rows.Count);
                RowMatch(rows[0], "quote \"and\" rfc-4180 double", "field a");
                RowMatch(rows[1], "1", "2");
            }
        }

        [Fact]
        public void HandlesBackslashEscapedQuotesAtEnd()
        {
            const string input = "$,\"a-z\\\"\",blorp\r\n£,nope,bleep";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(2, rows.Count);
                RowMatch(rows[0], "$", "a-z\"", "blorp");
                RowMatch(rows[1], "£", "nope", "bleep");
            }
        }

        [Fact]
        public void IgnoresEscapedBackslashPrecedingQuote()
        {
            const string input = "\"just a backslash\\\\\", two";

            using (var csv = Csv.FromString(input))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(1, rows.Count);
                RowMatch(rows[0], "just a backslash\\", "two");
            }
        }

        private static void RowMatch(IReadOnlyList<string> row, params string[] values)
        {
            Assert.Equal(values, row);
        }
    }
}
