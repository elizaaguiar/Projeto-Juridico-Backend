using JuridicoAnalise.Application.Interfaces;

namespace JuridicoAnalise.Infrastructure.Services;

public class TextReaderService : IDocumentReaderService
{
    public IEnumerable<string> SupportedExtensions => new[] { ".txt", ".csv", ".rtf", ".xml", ".json", ".html", ".htm" };

    public bool CanRead(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    public async Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
