namespace CsvSwan.Tests
{
    using System.Linq;
    using Xunit;

    public class MappingTests
    {
        [Fact]
        public void CanMapStringOnlyClass()
        {
            const string input = @"fred,bob,smith
charles,biggs,frompton
emily,,sutland";

            using (var csv = Csv.FromString(input))
            {
                var values = csv.MapRows<MyClassAllMapped>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("fred", values[0].FirstName);
                Assert.Equal("bob", values[0].MiddleName);
                Assert.Equal("smith", values[0].Surname);

                Assert.Equal("charles", values[1].FirstName);
                Assert.Equal("biggs", values[1].MiddleName);
                Assert.Equal("frompton", values[1].Surname);

                Assert.Equal("emily", values[2].FirstName);
                Assert.Equal(string.Empty, values[2].MiddleName);
                Assert.Equal("sutland", values[2].Surname);
            }
        }

        [Fact]
        public void CanMapStringOnlyClassAllUnmapped()
        {
            const string input = @"fred,bob,smith
charles,biggs,frompton
emily,,sutland";

            using (var csv = Csv.FromString(input))
            {
                var values = csv.MapRows<MyClassAllUnmapped>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("fred", values[0].FirstName);
                Assert.Equal("bob", values[0].MiddleName);
                Assert.Equal("smith", values[0].Surname);

                Assert.Equal("charles", values[1].FirstName);
                Assert.Equal("biggs", values[1].MiddleName);
                Assert.Equal("frompton", values[1].Surname);

                Assert.Equal("emily", values[2].FirstName);
                Assert.Equal(string.Empty, values[2].MiddleName);
                Assert.Equal("sutland", values[2].Surname);
            }
        }

        [Fact]
        public void CanMapStringOnlyClassWithIgnoredProperty()
        {
            const string input = @"fred,bob,smith
charles,biggs,frompton
emily,,sutland";

            using (var csv = Csv.FromString(input))
            {
                var values = csv.MapRows<MyClassIgnoreUnmapped>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Null(values[0].FirstName);
                Assert.Equal("bob", values[0].MiddleName);
                Assert.Equal("smith", values[0].Surname);

                Assert.Null(values[1].FirstName);
                Assert.Equal("biggs", values[1].MiddleName);
                Assert.Equal("frompton", values[1].Surname);

                Assert.Null(values[2].FirstName);
                Assert.Equal(string.Empty, values[2].MiddleName);
                Assert.Equal("sutland", values[2].Surname);
            }
        }

        public class MyClassAllMapped
        {
            [CsvColumnOrder(0)]
            public string FirstName { get; set; }

            [CsvColumnOrder(1)]
            public string MiddleName { get; set; }

            [CsvColumnOrder(2)]
            public string Surname { get; set; }
        }

        public class MyClassIgnoreUnmapped
        {
            public string FirstName { get; set; }

            [CsvColumnOrder(1)]
            public string MiddleName { get; set; }

            [CsvColumnOrder(2)]
            public string Surname { get; set; }
        }

        public class MyClassAllUnmapped
        {
            public string FirstName { get; set; }

            public string MiddleName { get; set; }

            public string Surname { get; set; }
        }
    }
}