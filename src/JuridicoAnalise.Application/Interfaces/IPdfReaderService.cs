namespace JuridicoAnalise.Application.Interfaces;

public interface IPdfReaderService
{
    Task<string> ExtractTextAsync(string filePath);
    Task<string> ExtractTextAsync(Stream stream);
}
