using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Application.DTOs;

public record DocumentoDto(
    Guid Id,
    string NumeroProcesso,
    string Setor,
    DateTime DataPublicacao,
    DateTime? InicioPrazo,
    string? Responsavel,
    TipoDocumento Tipo,
    StatusProcessamento Status,
    string NomeArquivo,
    DateTime CriadoEm
);

public record DocumentoDetalheDto(
    Guid Id,
    string NumeroProcesso,
    string Setor,
    DateTime DataPublicacao,
    DateTime? InicioPrazo,
    string? Responsavel,
    TipoDocumento Tipo,
    StatusProcessamento Status,
    string NomeArquivo,
    string CaminhoArquivo,
    string? Conteudo,
    string? MensagemErro,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);

public record UploadDocumentoDto(
    string NomeArquivo,
    string? Setor = null,
    string? Responsavel = null
);

public record AtualizarDocumentoDto(
    string? Setor,
    string? Responsavel,
    DateTime? InicioPrazo,
    TipoDocumento? Tipo
);
