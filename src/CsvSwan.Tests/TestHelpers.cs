namespace CsvSwan.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Xunit;

    internal static class TestHelpers
    {
        public static string GetDocumentPath(string documentName)
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "documents");
            var path = Path.Combine(root, documentName);

            return path;
        }

        public static void RowMatch(IReadOnlyList<string> row, params string[] values)
        {
            Assert.Equal(values, row);
        }
    }
}
