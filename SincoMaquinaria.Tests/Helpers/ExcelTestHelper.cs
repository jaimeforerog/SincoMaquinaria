using System.IO;
using ClosedXML.Excel;
using System.Data;

namespace SincoMaquinaria.Tests.Helpers;

public static class ExcelTestHelper
{
    public static Stream CreateExcelStream(string sheetName, DataTable data)
    {
        var ms = new MemoryStream();
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).InsertTable(data);
            workbook.SaveAs(ms);
        }
        ms.Position = 0;
        return ms;
    }

    public static Stream CreateExcelStream(string sheetName, IEnumerable<Dictionary<string, object>> rows)
    {
        var dt = new DataTable();
        
        if (!rows.Any()) return new MemoryStream();

        // Create columns
        foreach (var key in rows.First().Keys)
        {
            dt.Columns.Add(key);
        }

        foreach (var row in rows)
        {
            var dataRow = dt.NewRow();
            foreach (var key in row.Keys)
            {
                dataRow[key] = row[key];
            }
            dt.Rows.Add(dataRow);
        }

        return CreateExcelStream(sheetName, dt);
    }
}
