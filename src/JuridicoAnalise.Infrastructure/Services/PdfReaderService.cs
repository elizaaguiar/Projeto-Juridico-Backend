using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using JuridicoAnalise.Application.Interfaces;
using System.Text;

namespace JuridicoAnalise.Infrastructure.Services;

public class PdfReaderService : IDocumentReaderService, IPdfReaderService
{
    public IEnumerable<string> SupportedExtensions => new[] { ".pdf" };

    public bool CanRead(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    public Task<string> ExtractTextAsync(string filePath)
    {
        using var pdfReader = new PdfReader(filePath);
        using var pdfDocument = new PdfDocument(pdfReader);

        return Task.FromResult(ExtractTextFromDocument(pdfDocument));
    }

    public Task<string> ExtractTextAsync(Stream stream)
    {
        using var pdfReader = new PdfReader(stream);
        using var pdfDocument = new PdfDocument(pdfReader);

        return Task.FromResult(ExtractTextFromDocument(pdfDocument));
    }

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        return ExtractTextAsync(stream);
    }

    private static string ExtractTextFromDocument(PdfDocument pdfDocument)
    {
        var text = new StringBuilder();
        var strategy = new SimpleTextExtractionStrategy();

        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
            text.AppendLine(pageText);
        }

        return text.ToString();
    }
}
