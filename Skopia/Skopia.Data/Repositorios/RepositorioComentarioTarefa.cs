using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades;
using Skopia.Domain.Repositorios.Interfaces;

namespace Skopia.Data.Repositorios;


public class RepositorioComentarioTarefa : IRepositorioComentarioTarefa
{
    private readonly SkopiaDbContext _context;
    private readonly ILogger<RepositorioComentarioTarefa> _logger;
    private readonly DbSet<ComentarioTarefa> _dbSet;

    /// <inheritdoc/>
    public RepositorioComentarioTarefa(SkopiaDbContext context, ILogger<RepositorioComentarioTarefa> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = _context.Set<ComentarioTarefa>();
    }

    /// <inheritdoc/>
    public async Task<ComentarioTarefa> CriarAsync(ComentarioTarefa comentario)
    {
        _logger.LogInformation("Criando novo comentário para a tarefa com ID: {TarefaId}", comentario.TarefaId);
        await _dbSet.AddAsync(comentario);
        
        return comentario;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ComentarioTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId)
    {
        _logger.LogInformation("Obtendo todos os comentários para a tarefa com ID: {TarefaId}", tarefaId);
        return await _dbSet
                     .AsNoTracking()
                     .Where(c => c.TarefaId == tarefaId) 
                     .OrderBy(c => c.DataComentario) 
                     .ToListAsync(); 
    }
}
