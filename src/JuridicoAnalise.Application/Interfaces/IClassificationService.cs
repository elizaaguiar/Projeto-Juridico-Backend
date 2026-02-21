using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Application.Interfaces;

public interface IClassificationService
{
    Task<TipoDocumento> ClassifyDocumentAsync(string content);
    Task<DocumentClassificationResult> ClassifyAndExtractAsync(string content);
    Task<List<DocumentClassificationResult>> ExtractMultiplePublicationsAsync(string content);
}

public class DocumentClassificationResult
{
    public TipoDocumento Tipo { get; set; }
    public string? NumeroProcesso { get; set; }
    public DateTime? DataPublicacao { get; set; }
    public string? Setor { get; set; }
    public string? PalavraChaveUsada { get; set; }
    public double Confidence { get; set; }
}
