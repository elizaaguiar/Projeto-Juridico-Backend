using JuridicoAnalise.Domain.Entities;
using JuridicoAnalise.Domain.Enums;
using JuridicoAnalise.Domain.Interfaces;
using JuridicoAnalise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JuridicoAnalise.Infrastructure.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Documento?> GetByIdAsync(Guid id)
    {
        return await _context.Documentos.FindAsync(id);
    }

    public async Task<IEnumerable<Documento>> GetAllAsync()
    {
        return await _context.Documentos
            .OrderByDescending(d => d.CriadoEm)
            .ToListAsync();
    }

    public async Task<Documento> AddAsync(Documento documento)
    {
        documento.Id = Guid.NewGuid();
        await _context.Documentos.AddAsync(documento);
        await _context.SaveChangesAsync();
        return documento;
    }

    public async Task UpdateAsync(Documento documento)
    {
        _context.Documentos.Update(documento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var documento = await _context.Documentos.FindAsync(id);
        if (documento != null)
        {
            _context.Documentos.Remove(documento);
            await _context.SaveChangesAsync();
        }
    }
}
