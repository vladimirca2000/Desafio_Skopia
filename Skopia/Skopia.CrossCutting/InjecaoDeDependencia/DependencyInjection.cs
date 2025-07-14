using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; 
using Skopia.Data;
using Skopia.Data.Repositorios;
using Skopia.Data.UnitOfWork; 
using Skopia.Domain.Interfaces.UnitOfWork; 
using Skopia.Domain.Repositorios.Interfaces;
using Skopia.Domain.Servicos.Interfaces;
using Skopia.Domain.Servicos; 
using Skopia.Services.Interfaces;
using Skopia.Services.Mapeamento;
using Skopia.Services.Servicos;
using System.Reflection;

namespace Skopia.CrossCutting;

public static class DependencyInjection
{ 
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, ILogger<DependencyInjection> logger)
    {
       

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            
            throw new InvalidOperationException("A string de conexão não foi configurada. Verifique!");
        }

        services.AddDbContext<SkopiaDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure( 
                    maxRetryCount: 5, 
                    maxRetryDelay: TimeSpan.FromSeconds(30), 
                    errorNumbersToAdd: null 
                ));
        });


        
        services.AddScoped<IRepositorioProjeto, RepositorioProjeto>();
        services.AddScoped<IRepositorioTarefa, RepositorioTarefa>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioComentarioTarefa, RepositorioComentarioTarefa>();
        services.AddScoped<IRepositorioHistoricoAlteracaoTarefa, RepositorioHistoricoAlteracaoTarefa>();

        
        services.AddScoped<IProjetoServico, ProjetoServico>();

        
        services.AddScoped<IServicoProjeto, ServicoProjeto>(); 
        services.AddScoped<IServicoTarefa, ServicoTarefa>();  


        
        services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(MapeamentoPerfil).Assembly);

        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}