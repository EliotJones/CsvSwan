using System;
using System.Collections.Generic;

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
        public void CanMapStringWithColumnHeadersContainingWhitespace()
        {
            const string input = @"surname,FIRST NAME,green
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
            const string input = @"name,price,time,valid
Jim,5,100,true
Jane,,12,false
Bob,3,7,";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassUnmappedNullable>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("Jim", values[0].Name);
                Assert.Equal(5, values[0].Price);
                Assert.Equal(100, values[0].Time);
                Assert.True(values[0].Valid);

                Assert.Equal("Jane", values[1].Name);
                Assert.Null(values[1].Price);
                Assert.Equal(12, values[1].Time);
                Assert.False(values[1].Valid);

                Assert.Equal("Bob", values[2].Name);
                Assert.Equal(3, values[2].Price);
                Assert.Equal(7, values[2].Time);
                Assert.Null(values[2].Valid);
            }
        }

        [Fact]
        public void CanMapWithNonNullableIntFromColumnHeadersUsingDefaultOption()
        {
            const string input = @"name,price,time,valid
Jim,5,100,true
Jane,,12,false
Bob,3,7,";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassUnmappedNonNullable>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("Jim", values[0].Name);
                Assert.Equal(5, values[0].Price);
                Assert.Equal(100, values[0].Time);
                Assert.True(values[0].Valid);

                Assert.Equal("Jane", values[1].Name);
                Assert.Equal(0, values[1].Price);
                Assert.Equal(12, values[1].Time);
                Assert.False(values[1].Valid);

                Assert.Equal("Bob", values[2].Name);
                Assert.Equal(3, values[2].Price);
                Assert.Equal(7, values[2].Time);
                Assert.False(values[2].Valid);
            }
        }

        [Fact]
        public void CanMapWithNonNullableIntFromColumnHeadersUsingThrowOption()
        {
            const string input = @"name,price,time,valid
Jim,5,100,true
Jane,,12,false
Bob,3,7,";

            using (var csv = Csv.FromString(input, new CsvOptions
            {
                DefaultNullValues = false,
                HasHeaderRow = true
            }))
            {
                // ReSharper disable once AccessToDisposedClosure
                Func<List<MyClassUnmappedNonNullable>> func = () => csv.Map<MyClassUnmappedNonNullable>().ToList();

                Assert.Throws<InvalidOperationException>(func);
            }
        }

        [Fact]
        public void CanMapWithDateTime()
        {
            const string input = @"key,created,identifier
any-key,2022-05-16,3873764
other-key,2020-12-05,457473
my-key,,473737";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassUnmappedDateTime>().ToList();

                Assert.Equal(3, values.Count);

                Assert.Equal("any-key", values[0].Key);
                Assert.Equal(new DateTime(2022, 5, 16), values[0].Created);

                Assert.Equal("other-key", values[1].Key);
                Assert.Equal(new DateTime(2020, 12, 5), values[1].Created);

                Assert.Equal("my-key", values[2].Key);
                Assert.Equal(DateTime.MinValue, values[2].Created);
            }
        }

        [Fact]
        public void CanMapWithMultipleAttributes()
        {
            const string input = @"J554323,1st,SKU
camry,toyota,366212
accord,honda,477299";

            using (var csv = Csv.FromString(input, hasHeaderRow: true))
            {
                var values = csv.Map<MyClassMappedMultipleWays>().ToList();

                Assert.Equal(2, values.Count);

                Assert.Equal("toyota", values[0].First);
                Assert.Equal("camry", values[0].Last);
                Assert.Equal("366212", values[0].Sku);

                Assert.Equal("honda", values[1].First);
                Assert.Equal("accord", values[1].Last);
                Assert.Equal("477299", values[1].Sku);
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

            public bool? Valid { get; set; }
        }

        public class MyClassUnmappedNonNullable
        {
            public string Name { get; set; }

            public int Price { get; set; }

            public int Time { get; set; }

            public bool Valid { get; set; }
        }

        public class MyClassUnmappedDateTime
        {
            public string Key { get; set; }

            public DateTime Created { get; set; }
        }

        public class MyClassMappedMultipleWays
        {
            [CsvColumnName("1st")]
            public string First { get; set; }

            [CsvColumnOrder(0)]
            public string Last { get; set; }

            public string Sku { get; set; }
        }
    }
}
