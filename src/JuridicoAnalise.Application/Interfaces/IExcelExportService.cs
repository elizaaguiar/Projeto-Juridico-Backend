using JuridicoAnalise.Domain.Entities;

namespace JuridicoAnalise.Application.Interfaces;

public interface IExcelExportService
{
    Task<byte[]> ExportDocumentosAsync(IEnumerable<Documento> documentos);
    Task<MemoryStream> ExportToStreamAsync(IEnumerable<Documento> documentos);
}
