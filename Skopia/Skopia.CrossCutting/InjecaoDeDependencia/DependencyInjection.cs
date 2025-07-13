using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Embora não usado diretamente no AddInfrastructure, é comum para logging.
using Skopia.Data;
using Skopia.Data.Repositorios; // Namespace para as implementações concretas dos repositórios
using Skopia.Data.UnitOfWork; // Namespace para a implementação concreta do UnitOfWork
using Skopia.Domain.Interfaces.UnitOfWork; // Namespace para a interface do UnitOfWork
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
    /// <summary>
    /// Configura os serviços de infraestrutura da aplicação, incluindo o DbContext,
    /// os repositórios e o Unit of Work.
    /// </summary>
    /// <param name="services">A coleção de serviços onde as dependências serão registradas.</param>
    /// <param name="configuration">A configuração da aplicação para acessar a string de conexão.</param>
    /// <returns>A coleção de serviços atualizada.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            // É crucial que a string de conexão esteja configurada corretamente.
            // Uma exceção explícita aqui evita problemas em tempo de execução.
            throw new InvalidOperationException("A string de conexão não foi configurada. Verifique!");
        }

        services.AddDbContext<SkopiaDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure( // Adiciona resiliência à conexão com o banco de dados.
                    maxRetryCount: 5, // Número máximo de tentativas de repetição.
                    maxRetryDelay: TimeSpan.FromSeconds(30), // Atraso máximo entre as tentativas.
                    errorNumbersToAdd: null // Erros adicionais que devem ser considerados transitórios.
                ));
        });


        // REGISTRO DOS REPOSITÓRIOS ESPECÍFICOS (com nomenclatura em português):
        // O lifetime 'Scoped' garante que uma única instância do repositório seja criada por requisição HTTP,
        // e que essa instância compartilhe o mesmo DbContext, o que é ideal para operações de unidade de trabalho.
        services.AddScoped<IRepositorioProjeto, RepositorioProjeto>();
        services.AddScoped<IRepositorioTarefa, RepositorioTarefa>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioComentarioTarefa, RepositorioComentarioTarefa>();
        services.AddScoped<IRepositorioHistoricoAlteracaoTarefa, RepositorioHistoricoAlteracaoTarefa>();

        // REGISTRO DOS SERVIÇOS DE DOMÍNIO (com nomenclatura em português):
        services.AddScoped<IProjetoServico, ProjetoServico>();

        // No método AddInfrastructure:
        services.AddScoped<IServicoProjeto, ServicoProjeto>(); 
        services.AddScoped<IServicoTarefa, ServicoTarefa>();  


        // Adicione esta linha no método AddInfrastructure
        services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(MapeamentoPerfil).Assembly);

        // Registro do Unit of Work (Scoped Lifetime)
        // O Unit of Work também é Scoped para garantir que todas as operações dentro de uma requisição
        // compartilhem o mesmo contexto de banco de dados e sejam comitadas ou revertidas juntas.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}