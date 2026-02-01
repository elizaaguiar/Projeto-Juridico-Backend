using JuridicoAnalise.Application.Interfaces;

namespace JuridicoAnalise.Infrastructure.Services;

public class UnifiedDocumentReaderService : IUnifiedDocumentReaderService
{
    private readonly IEnumerable<IDocumentReaderService> _readers;

    public UnifiedDocumentReaderService(IEnumerable<IDocumentReaderService> readers)
    {
        _readers = readers;
    }

    public bool CanRead(string fileName)
    {
        return _readers.Any(r => r.CanRead(fileName));
    }

    public IEnumerable<string> GetAllSupportedExtensions()
    {
        return _readers.SelectMany(r => r.SupportedExtensions).Distinct();
    }

    public async Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        var reader = _readers.FirstOrDefault(r => r.CanRead(fileName));

        if (reader == null)
        {
            throw new NotSupportedException($"Tipo de arquivo não suportado: {Path.GetExtension(fileName)}");
        }

        return await reader.ExtractTextAsync(stream, fileName);
    }
}
