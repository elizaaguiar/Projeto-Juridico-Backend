using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using JuridicoAnalise.Application.Interfaces;
using System.Text;

namespace JuridicoAnalise.Infrastructure.Services;

public class DocxReaderService : IDocumentReaderService
{
    public IEnumerable<string> SupportedExtensions => new[] { ".docx", ".doc" };

    public bool CanRead(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (extension == ".docx")
        {
            return Task.FromResult(ExtractFromDocx(stream));
        }

        // Para .doc antigo, tentamos ler como texto
        // Nota: Para suporte completo a .doc, seria necessário uma biblioteca adicional
        return Task.FromResult(ExtractFromDocx(stream));
    }

    private static string ExtractFromDocx(Stream stream)
    {
        var text = new StringBuilder();

        using var wordDocument = WordprocessingDocument.Open(stream, false);
        var body = wordDocument.MainDocumentPart?.Document?.Body;

        if (body != null)
        {
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var paragraphText = new StringBuilder();

                foreach (var run in paragraph.Elements<Run>())
                {
                    foreach (var textElement in run.Elements<Text>())
                    {
                        paragraphText.Append(textElement.Text);
                    }
                }

                text.AppendLine(paragraphText.ToString());
            }

            // Também extrair texto de tabelas
            foreach (var table in body.Elements<Table>())
            {
                foreach (var row in table.Elements<TableRow>())
                {
                    var rowText = new List<string>();
                    foreach (var cell in row.Elements<TableCell>())
                    {
                        var cellText = new StringBuilder();
                        foreach (var para in cell.Elements<Paragraph>())
                        {
                            foreach (var run in para.Elements<Run>())
                            {
                                foreach (var textElement in run.Elements<Text>())
                                {
                                    cellText.Append(textElement.Text);
                                }
                            }
                        }
                        rowText.Add(cellText.ToString());
                    }
                    text.AppendLine(string.Join(" | ", rowText));
                }
            }
        }

        return text.ToString();
    }
}
