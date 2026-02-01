namespace JuridicoAnalise.Application.Interfaces;

public interface IDocumentReaderService
{
    Task<string> ExtractTextAsync(Stream stream, string fileName);
    bool CanRead(string fileName);
    IEnumerable<string> SupportedExtensions { get; }
}
