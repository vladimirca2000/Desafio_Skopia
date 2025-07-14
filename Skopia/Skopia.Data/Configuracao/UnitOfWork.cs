using Microsoft.Extensions.Logging;
using Skopia.Data; 
using Skopia.Data.Repositorios; 
using Skopia.Domain.Interfaces.UnitOfWork;
using Skopia.Domain.Repositorios.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skopia.Data.UnitOfWork;


public class UnitOfWork : IUnitOfWork
{
    private readonly SkopiaDbContext _context;

    private readonly ILogger<UnitOfWork> _logger;

    private readonly ILogger<RepositorioProjeto> loggerProjeto;
    private readonly ILogger<RepositorioTarefa> loggerTarefa;
    private readonly ILogger<RepositorioUsuario> loggerUsuario;
    private readonly ILogger<RepositorioComentarioTarefa> loggerComentarioTarefa;
    private readonly ILogger<RepositorioHistoricoAlteracaoTarefa> loggerHistoricoAlteracaoTarefa;

    private IRepositorioProjeto? _repositorioProjetos;
    private IRepositorioTarefa? _repositorioTarefas;
    private IRepositorioUsuario? _repositorioUsuarios;
    private IRepositorioComentarioTarefa? _repositorioComentariosTarefas;
    private IRepositorioHistoricoAlteracaoTarefa? _repositorioHistoricosAlteracaoTarefa;

    public UnitOfWork(SkopiaDbContext context, ILogger<UnitOfWork> logger, ILogger<RepositorioProjeto> loggerProjeto, ILogger<RepositorioTarefa> loggerTarefa, ILogger<RepositorioUsuario> loggerUsuario, ILogger<RepositorioComentarioTarefa> loggerComentarioTarefa, ILogger<RepositorioHistoricoAlteracaoTarefa> loggerHistoricoAlteracaoTarefa)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.loggerProjeto = loggerProjeto ?? throw new ArgumentNullException(nameof(loggerProjeto));
        this.loggerTarefa = loggerTarefa ?? throw new ArgumentNullException(nameof(loggerTarefa));
        this.loggerUsuario = loggerUsuario ?? throw new ArgumentNullException(nameof(loggerUsuario));
        this.loggerComentarioTarefa = loggerComentarioTarefa ?? throw new ArgumentNullException(nameof(loggerComentarioTarefa));
        this.loggerHistoricoAlteracaoTarefa = loggerHistoricoAlteracaoTarefa ?? throw new ArgumentNullException(nameof(loggerHistoricoAlteracaoTarefa));
    }

    public IRepositorioProjeto Projetos => _repositorioProjetos ??= new RepositorioProjeto(_context, loggerProjeto);
    public IRepositorioTarefa Tarefas => _repositorioTarefas ??= new RepositorioTarefa(_context, loggerTarefa);
    public IRepositorioUsuario Usuarios => _repositorioUsuarios ??= new RepositorioUsuario(_context, loggerUsuario);
    public IRepositorioComentarioTarefa ComentariosTarefas => _repositorioComentariosTarefas ??= new RepositorioComentarioTarefa(_context, loggerComentarioTarefa);
    public IRepositorioHistoricoAlteracaoTarefa HistoricosAlteracaoTarefa => _repositorioHistoricosAlteracaoTarefa ??= new RepositorioHistoricoAlteracaoTarefa(_context, loggerHistoricoAlteracaoTarefa);


    /// <inheritdoc/>
    public async Task<int> CommitAsync()
    {
        _logger.LogInformation("Comitando alterações no banco de dados.");
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _logger.LogInformation("Liberando recursos do UnitOfWork.");
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
    /// <inheritdoc/>
    public Task RollbackAsync()
    {
        _logger.LogWarning("Revertendo alterações não salvas no banco de dados.");
        _context.ChangeTracker.Clear(); 
        return Task.CompletedTask;
    }
}