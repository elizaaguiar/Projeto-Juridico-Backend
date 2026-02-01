using JuridicoAnalise.Domain.Entities;
using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Domain.Interfaces;

public interface IDocumentoRepository
{
    Task<Documento?> GetByIdAsync(Guid id);
    Task<IEnumerable<Documento>> GetAllAsync();
    Task<Documento> AddAsync(Documento documento);
    Task UpdateAsync(Documento documento);
    Task DeleteAsync(Guid id);
}
