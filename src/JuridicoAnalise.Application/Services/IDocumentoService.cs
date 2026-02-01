using JuridicoAnalise.Application.DTOs;
using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Application.Services;

public interface IDocumentoService
{
    Task<IEnumerable<DocumentoDto>> GetAllAsync();
    Task<DocumentoDetalheDto?> GetByIdAsync(Guid id);
    Task<DocumentoDto> ProcessarDocumentoAsync(Stream fileStream, string fileName, UploadDocumentoDto uploadDto);
    Task<IEnumerable<DocumentoDto>> ProcessarMultiplosAsync(IEnumerable<(Stream stream, string fileName)> files, UploadDocumentoDto uploadDto);
    Task<DocumentoDto> AtualizarAsync(Guid id, AtualizarDocumentoDto dto);
    Task DeletarAsync(Guid id);
    Task<byte[]> ExportarParaExcelAsync(TipoDocumento? tipo = null);
    Task<byte[]> ProcessarEExportarAsync(IEnumerable<(Stream stream, string fileName)> files);
}
