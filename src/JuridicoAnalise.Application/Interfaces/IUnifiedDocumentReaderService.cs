namespace JuridicoAnalise.Application.Interfaces;

public interface IUnifiedDocumentReaderService
{
    Task<string> ExtractTextAsync(Stream stream, string fileName);
    bool CanRead(string fileName);
    IEnumerable<string> GetAllSupportedExtensions();
}
