using ClosedXML.Excel;
using JuridicoAnalise.Application.Interfaces;
using System.Text;

namespace JuridicoAnalise.Infrastructure.Services;

public class ExcelReaderService : IDocumentReaderService
{
    public IEnumerable<string> SupportedExtensions => new[] { ".xlsx", ".xls", ".xlsm" };

    public bool CanRead(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        var text = new StringBuilder();

        using var workbook = new XLWorkbook(stream);

        foreach (var worksheet in workbook.Worksheets)
        {
            text.AppendLine($"=== Planilha: {worksheet.Name} ===");
            text.AppendLine();

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null) continue;

            foreach (var row in usedRange.Rows())
            {
                var cellValues = new List<string>();

                foreach (var cell in row.Cells())
                {
                    var value = cell.GetFormattedString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        cellValues.Add(value);
                    }
                }

                if (cellValues.Count > 0)
                {
                    text.AppendLine(string.Join(" | ", cellValues));
                }
            }

            text.AppendLine();
        }

        return Task.FromResult(text.ToString());
    }
}
