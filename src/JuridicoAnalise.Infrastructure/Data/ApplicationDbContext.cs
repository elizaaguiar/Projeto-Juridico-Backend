using JuridicoAnalise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JuridicoAnalise.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<PalavraChave> PalavrasChave => Set<PalavraChave>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroProcesso).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Setor).HasMaxLength(100);
            entity.Property(e => e.NomeArquivo).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CaminhoArquivo).HasMaxLength(1000).IsRequired();
            entity.HasIndex(e => e.NumeroProcesso);
        });

        modelBuilder.Entity<PalavraChave>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Termo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TipoDocumento).HasConversion<string>();
            entity.HasIndex(e => e.TipoDocumento);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CriadoEm = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.AtualizadoEm = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
