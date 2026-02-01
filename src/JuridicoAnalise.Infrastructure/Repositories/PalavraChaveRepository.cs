using JuridicoAnalise.Domain.Entities;
using JuridicoAnalise.Domain.Enums;
using JuridicoAnalise.Domain.Interfaces;
using JuridicoAnalise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JuridicoAnalise.Infrastructure.Repositories;

public class PalavraChaveRepository : IPalavraChaveRepository
{
    private readonly ApplicationDbContext _context;

    public PalavraChaveRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PalavraChave>> GetAllAsync()
    {
        return await _context.PalavrasChave
            .Where(p => p.Ativo)
            .OrderBy(p => p.TipoDocumento)
            .ThenBy(p => p.Termo)
            .ToListAsync();
    }

    public async Task<IEnumerable<PalavraChave>> GetByTipoAsync(TipoDocumento tipo)
    {
        return await _context.PalavrasChave
            .Where(p => p.TipoDocumento == tipo && p.Ativo)
            .ToListAsync();
    }

    public async Task<PalavraChave> AddAsync(PalavraChave palavraChave)
    {
        palavraChave.Id = Guid.NewGuid();
        await _context.PalavrasChave.AddAsync(palavraChave);
        await _context.SaveChangesAsync();
        return palavraChave;
    }

    public async Task UpdateAsync(PalavraChave palavraChave)
    {
        _context.PalavrasChave.Update(palavraChave);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var palavraChave = await _context.PalavrasChave.FindAsync(id);
        if (palavraChave != null)
        {
            palavraChave.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }
}
