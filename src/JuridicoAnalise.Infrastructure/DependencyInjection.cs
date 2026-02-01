using JuridicoAnalise.Application.Interfaces;
using JuridicoAnalise.Domain.Interfaces;
using JuridicoAnalise.Infrastructure.Data;
using JuridicoAnalise.Infrastructure.Repositories;
using JuridicoAnalise.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IUnifiedDocumentReaderService = JuridicoAnalise.Application.Interfaces.IUnifiedDocumentReaderService;

namespace JuridicoAnalise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        services.AddScoped<IPalavraChaveRepository, PalavraChaveRepository>();

        // Document Readers - registrar todos os leitores
        services.AddScoped<IDocumentReaderService, PdfReaderService>();
        services.AddScoped<IDocumentReaderService, DocxReaderService>();
        services.AddScoped<IDocumentReaderService, ExcelReaderService>();
        services.AddScoped<IDocumentReaderService, TextReaderService>();

        // Unified Document Reader
        services.AddScoped<IUnifiedDocumentReaderService, UnifiedDocumentReaderService>();

        // Legacy PDF Reader (para compatibilidade)
        services.AddScoped<IPdfReaderService, PdfReaderService>();

        // Other Services
        services.AddScoped<IClassificationService, ClassificationService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();

        return services;
    }
}
