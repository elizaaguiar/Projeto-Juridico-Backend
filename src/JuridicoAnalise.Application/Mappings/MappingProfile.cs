using AutoMapper;
using JuridicoAnalise.Application.DTOs;
using JuridicoAnalise.Domain.Entities;

namespace JuridicoAnalise.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Documento, DocumentoDto>();
        CreateMap<Documento, DocumentoDetalheDto>();

        CreateMap<PalavraChave, PalavraChaveDto>();
        CreateMap<CriarPalavraChaveDto, PalavraChave>();
    }
}
