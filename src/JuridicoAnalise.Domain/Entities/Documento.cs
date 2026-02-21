using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Domain.Entities;

public class Documento : BaseEntity
{
    public string NumeroProcesso { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string? PalavraChaveUsada { get; set; }
    public DateTime DataPublicacao { get; set; }
    public DateTime? InicioPrazo { get; set; }
    public string? Conteudo { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string CaminhoArquivo { get; set; } = string.Empty;
    public string? MensagemErro { get; set; }
}
