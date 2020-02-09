# CSV Swan #

CsvSwan is a small side-project to build a .NET Standard CSV parsing library that "just works" for the simplest CSV scenarios.

    using (Csv csv = Csv.Open(@"C:\path\to\file.csv"))
    {
        foreach (var row in csv.Rows)
        {
            // Proposed design(s)...
            int id = row.GetInt(0);
            string name = row.GetString(1);
            MyClass obj1 = row.Get<MyClass>();

            // Current design
            IReadOnlyList<string> values = row.GetValues();
        }
    }

The default settings are such that a file is expected to have:

+ A comma separator `,`.
+ Newlines between rows, either Unix `\n` or Windows `\r\n`.
+ Optional quotes using the double quote `"` for fields.
+ Quotes inside quoted fields escaped using either RFC-4180 double-double quotes `""` or backslash escaped `\"`.

This doesn't support support quote-escaping outside quoted fields, but quotes can be turned off entirely in the options. Additionally the user must specify if the file contains a header row prior to parsing.

The separator and quote character can be set to other values.

This was just a small side project to pass time on the train, it probably won't be finished or published on NuGet.