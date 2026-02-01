using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Application.DTOs;

public record PalavraChaveDto(
    Guid Id,
    string Termo,
    TipoDocumento TipoDocumento,
    bool Ativo
);

public record CriarPalavraChaveDto(
    string Termo,
    TipoDocumento TipoDocumento
);
