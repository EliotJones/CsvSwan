namespace CsvSwan.Tests
{
    using System.Linq;
    using Xunit;

    public class MapTests
    {
        [Fact]
        public void CanMapStringOnlyClass()
        {
            const string input = @"fred,bob,smith
charles,biggs,frompton
emily,,sutland";

            using (var csv = Csv.FromString(input))
            {
                var values = csv.Map<MyClassAllMapped>().ToList();

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
                var values = csv.Map<MyClassAllUnmapped>().ToList();

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
                var values = csv.Map<MyClassIgnoreUnmapped>().ToList();

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

        [Fact]
        public void CanMapStringWithColumnHeaders()
        {
            const string input = @"surname,FIRSTNAME,green
bloggs,joe,no
shaw,susan,no
o'neill,sheila,yes";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassAllUnmapped>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("joe", values[0].FirstName);
                Assert.Null(values[0].MiddleName);
                Assert.Equal("bloggs", values[0].Surname);

                Assert.Equal("susan", values[1].FirstName);
                Assert.Null(values[1].MiddleName);
                Assert.Equal("shaw", values[1].Surname);

                Assert.Equal("sheila", values[2].FirstName);
                Assert.Null(values[2].MiddleName);
                Assert.Equal("o'neill", values[2].Surname);
            }
        }

        [Fact]
        public void CanMapStringPrefersAttributeToColumnHeaders()
        {
            const string input = @"surname,middle,lastNAMe
wrong,bigsby,bongsby
plinky,plonky,music";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassIgnoreUnmapped>().ToList();

                Assert.Equal(2, values.Count);

                Assert.Null(values[0].FirstName);
                Assert.Equal("bigsby", values[0].MiddleName);
                Assert.Equal("bongsby", values[0].Surname);

                Assert.Null(values[1].FirstName);
                Assert.Equal("plonky", values[1].MiddleName);
                Assert.Equal("music", values[1].Surname);
            }
        }

        [Fact]
        public void CanMapWithNullableIntFromColumnHeaders()
        {
            const string input = @"name,price,time
Jim,5,100
Jane,,12
Bob,3,7";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassUnmappedNullable>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("Jim", values[0].Name);
                Assert.Equal(5, values[0].Price);
                Assert.Equal(100, values[0].Time);

                Assert.Equal("Jane", values[1].Name);
                Assert.Null(values[1].Price);
                Assert.Equal(12, values[1].Time);

                Assert.Equal("Bob", values[2].Name);
                Assert.Equal(3, values[2].Price);
                Assert.Equal(7, values[2].Time);
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

        public class MyClassUnmappedNullable
        {
            public string Name { get; set; }

            public int? Price { get; set; }

            public int Time { get; set; }
        }
    }
}
