using JuridicoAnalise.Domain.Entities;
using JuridicoAnalise.Domain.Enums;

namespace JuridicoAnalise.Domain.Interfaces;

public interface IPalavraChaveRepository
{
    Task<IEnumerable<PalavraChave>> GetAllAsync();
    Task<IEnumerable<PalavraChave>> GetByTipoAsync(TipoDocumento tipo);
    Task<PalavraChave> AddAsync(PalavraChave palavraChave);
    Task UpdateAsync(PalavraChave palavraChave);
    Task DeleteAsync(Guid id);
}
