using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades; 
using Skopia.Domain.Repositorios.Interfaces; 

namespace Skopia.Data.Repositorios;


public class RepositorioHistoricoAlteracaoTarefa : IRepositorioHistoricoAlteracaoTarefa
{
    private readonly SkopiaDbContext _context;
    private readonly ILogger<RepositorioHistoricoAlteracaoTarefa> _logger;
    private readonly DbSet<HistoricoAlteracaoTarefa> _dbSet;

    /// <inheritdoc/>
    public RepositorioHistoricoAlteracaoTarefa(SkopiaDbContext context, ILogger<RepositorioHistoricoAlteracaoTarefa> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioHistoricoAlteracaoTarefa.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "O logger não pode ser nulo para o RepositorioHistoricoAlteracaoTarefa.");
        _dbSet = _context.Set<HistoricoAlteracaoTarefa>();
    }

    /// <inheritdoc/>
    public async Task<HistoricoAlteracaoTarefa> CriarAsync(HistoricoAlteracaoTarefa historicoAlteracaoTarefa)
    {
        _logger.LogInformation("Criando histórico de alteração para a tarefa com ID: {TarefaId}", historicoAlteracaoTarefa.TarefaId);
        await _dbSet.AddAsync(historicoAlteracaoTarefa);

        
        return historicoAlteracaoTarefa; 
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<HistoricoAlteracaoTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId)
    {
        _logger.LogInformation("Obtendo todos os históricos de alteração para a tarefa com ID: {TarefaId}", tarefaId);
        return await _dbSet
                     .AsNoTracking()
                     .Where(h => h.TarefaId == tarefaId)
                     .OrderBy(h => h.DataModificacao) 
                     .ToListAsync();
    }
}