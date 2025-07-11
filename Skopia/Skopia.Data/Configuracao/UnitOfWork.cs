using Skopia.Data; // Assumindo que SkopiaDbContext está neste namespace
using Skopia.Data.Repositorios; // Certifique-se de que este namespace contém suas implementações de repositórios
using Skopia.Domain.Interfaces.UnitOfWork;
using Skopia.Domain.Repositorios.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skopia.Data.UnitOfWork;

/// <summary>
/// Implementação concreta do padrão Unit of Work.
/// Esta classe é responsável por gerenciar o ciclo de vida do DbContext e as instâncias dos repositórios,
/// garantindo que todas as operações de persistência dentro de uma transação lógica sejam atômicas.
/// Ela atua como uma fachada para as operações de persistência, abstraindo os detalhes
/// de como os dados são salvos no banco de dados.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SkopiaDbContext _context;

    // Campos privados para lazy initialization (inicialização preguiçosa) dos repositórios.
    // Isso significa que uma instância de repositório só será criada quando for acessada pela primeira vez,
    // otimizando o uso de memória e recursos, especialmente em cenários onde nem todos os repositórios
    // são necessários em todas as requisições.
    private IRepositorioProjeto? _repositorioProjetos;
    private IRepositorioTarefa? _repositorioTarefas;
    private IRepositorioUsuario? _repositorioUsuarios;
    private IRepositorioComentarioTarefa? _repositorioComentariosTarefas;
    private IRepositorioHistoricoAlteracaoTarefa? _repositorioHistoricosAlteracaoTarefa;

    /// <summary>
    /// Construtor da UnitOfWork. Recebe a instância do DbContext via Injeção de Dependência.
    /// É crucial que o DbContext seja gerenciado pelo sistema de DI (e.g., ASP.NET Core DI)
    /// com um tempo de vida apropriado (geralmente Scoped para aplicações web,
    /// garantindo uma instância por requisição).
    /// </summary>
    /// <param name="context">A instância do SkopiaDbContext.</param>
    /// <exception cref="ArgumentNullException">Lançada se o DbContext fornecido for nulo.</exception>
    public UnitOfWork(SkopiaDbContext context)
    {
        // Validação para garantir que o DbContext não seja nulo, evitando NullReferenceExceptions.
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Propriedades que expõem as interfaces dos repositórios, com lazy initialization.
    // O operador `??=` (null-coalescing assignment) é usado para instanciar o repositório
    // apenas se a variável privada correspondente for nula.

    public IRepositorioProjeto Projetos => _repositorioProjetos ??= new RepositorioProjeto(_context);
    public IRepositorioTarefa Tarefas => _repositorioTarefas ??= new RepositorioTarefa(_context);
    public IRepositorioUsuario Usuarios => _repositorioUsuarios ??= new RepositorioUsuario(_context);
    public IRepositorioComentarioTarefa ComentariosTarefas => _repositorioComentariosTarefas ??= new RepositorioComentarioTarefa(_context);
    public IRepositorioHistoricoAlteracaoTarefa HistoricosAlteracaoTarefa => _repositorioHistoricosAlteracaoTarefa ??= new RepositorioHistoricoAlteracaoTarefa(_context);


    /// <summary>
    /// Persiste todas as mudanças rastreadas pelo DbContext no banco de dados de forma assíncrona.
    /// Este é o único ponto onde `SaveChangesAsync` é chamado na cadeia de repositórios/UoW,
    /// garantindo que todas as operações de escrita (inserções, atualizações, exclusões)
    /// sejam parte de uma única transação lógica. Isso é fundamental para a atomicidade
    /// e consistência dos dados.
    /// </summary>
    /// <returns>O número de estados de entidade gravados no banco de dados.</returns>
    public async Task<int> CommitAsync()
    {
        // O `SaveChangesAsync` do Entity Framework Core detecta automaticamente as mudanças
        // feitas nas entidades rastreadas pelo DbContext e as persiste no banco de dados.
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Libera os recursos não gerenciados (como conexões de banco de dados) utilizados pelo DbContext.
    /// É uma implementação do padrão Dispose para a interface IDisposable.
    /// É crucial chamar `Dispose()` para liberar conexões de banco de dados e outros recursos
    /// gerenciados pelo DbContext, especialmente em aplicações web onde o DbContext
    /// tem um tempo de vida Scoped (por requisição).
    /// O gerenciamento adequado do `IDisposable` previne vazamentos de memória e recursos.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        // `GC.SuppressFinalize(this)` é chamado para informar ao Garbage Collector que o finalizador
        // (se houver) deste objeto não precisa ser executado, pois os recursos já foram liberados
        // explicitamente. Isso otimiza o processo de coleta de lixo.
        GC.SuppressFinalize(this);
    }
}