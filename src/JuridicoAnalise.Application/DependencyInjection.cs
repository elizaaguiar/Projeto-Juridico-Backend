using FluentValidation;
using JuridicoAnalise.Application.Mappings;
using JuridicoAnalise.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JuridicoAnalise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IDocumentoService, DocumentoService>();

        return services;
    }
}
