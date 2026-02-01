using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Domain.Entities;

public class PalavraChave : BaseEntity
{
    public string Termo { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento { get; set; }
    public bool Ativo { get; set; } = true;
}
