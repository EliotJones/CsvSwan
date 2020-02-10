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

                TestHelpers.RowMatch(rows[0], "a string", "another one", "1.433", "simple");
                TestHelpers.RowMatch(rows[1], "we have", "four columns", "42.564", "that's all 2 rows");
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

                TestHelpers.RowMatch(rows[0], "ham", "egg", string.Empty, "cheese");
                TestHelpers.RowMatch(rows[1], string.Empty, string.Empty, string.Empty, string.Empty);
                TestHelpers.RowMatch(rows[2], "cabbage", "port", "mushroom", "elixir");
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

                TestHelpers.RowMatch(rows[0], "value 1", "value, comma", "no quote");
                TestHelpers.RowMatch(rows[1], "quoted", "not quoted", "7");
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

                TestHelpers.RowMatch(rows[0], "7556", "546", "harp");
                TestHelpers.RowMatch(rows[1], "534", "778", "lute");
                TestHelpers.RowMatch(rows[2], "788", "0.656", "trombone");
                TestHelpers.RowMatch(rows[3], string.Empty, string.Empty, string.Empty);
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
                TestHelpers.RowMatch(rows[0], @"q""", "a");
                TestHelpers.RowMatch(rows[1], "1", "2");
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

                TestHelpers.RowMatch(rows[0], @"""<", "1");
                TestHelpers.RowMatch(rows[1], string.Empty, string.Empty);
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

                TestHelpers.RowMatch(rows[0], string.Empty, "1");
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

                TestHelpers.RowMatch(rows[0], doubleQuote, "1");
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
                TestHelpers.RowMatch(rows[0], "A field with a \"quote\"", "field2");
                TestHelpers.RowMatch(rows[1], "field 1", "quoted field,");
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
                TestHelpers.RowMatch(rows[0], "quote \"and\" rfc-4180 double", "field a");
                TestHelpers.RowMatch(rows[1], "1", "2");
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
                TestHelpers.RowMatch(rows[0], "$", "a-z\"", "blorp");
                TestHelpers.RowMatch(rows[1], "£", "nope", "bleep");
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
                TestHelpers.RowMatch(rows[0], "just a backslash\\", "two");
            }
        }

        [Fact]
        public void SupportsTabSeparator()
        {
            const string input = "\"aesop rock\"\t1.796\tgreen\r\n\"ho99o9\"\t2.732\tblue\nN/A\t7.6\tlilac";

            using (var csv = Csv.FromString(input, '\t'))
            {
                var rows = csv.GetAllRowValues();

                Assert.Equal(3, rows.Count);

                TestHelpers.RowMatch(rows[0], "aesop rock", "1.796", "green");
                TestHelpers.RowMatch(rows[1], "ho99o9", "2.732", "blue");
                TestHelpers.RowMatch(rows[2], "N/A", "7.6", "lilac");
            }
        }
    }
}
