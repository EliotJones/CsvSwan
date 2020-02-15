# CSV Swan #

<img src="https://raw.githubusercontent.com/EliotJones/CsvSwan/master/icon.png" width="128px"/>

CsvSwan is a small .NET Standard CSV parsing library that "just works" for the simplest CSV scenarios. It aims to be high-performance and provide easy-to-use defaults.

    using (Csv csv = Csv.Open(@"C:\path\to\file.csv"))
    {
        foreach (var row in csv.Rows)
        {
            // Get all values
            IReadOnlyList<string> values = row.GetValues();

            // Map to a numeric type
            int id = row.GetInt(0);
            decimal value = row.GetDecimal(1);
        }
    }

You can also map to a list of objects of a given type:

    public class MyClass
    {
        [CsvColumnOrder(0)]
        public int Id { get; set; }

        [CsvColumnOrder(1)]
        public string Name { get; set; }
    }

    using (Csv cvs = Csv.Open(@"C:\path\to\file.csv"))
    {
        List<MyClass> results = csv.Map<MyClass>().ToList();
    }

The default settings are for a file with:

+ A comma separator `,`.
+ Newlines between rows, either Unix `\n` or Windows `\r\n`.
+ Optional quotes using the double quote `"` for fields.
+ Quotes inside quoted fields escaped using either RFC-4180 double-double quotes `""` (or optionally backslash escaped `\"`, use `CsvOptions.BackslashEscapesQuotes`, off by default).

Additionally the user must specify if the file contains a header row prior to parsing using `CsvOptions.HasHeaderRow`, this is off by default.

The separator and quote character can be set to other values using the `CsvOptions` parameter to the `Csv.Open` methods.

## Installation ##

Get it from [NuGet](https://www.nuget.org/packages/CsvSwan) or install from the package manager command line:

    > Install-Package CsvSwan