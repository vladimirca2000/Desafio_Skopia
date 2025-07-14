using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades; 
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces;

namespace Skopia.Data.Repositorios;


public class RepositorioProjeto : IRepositorioProjeto
{
    private readonly SkopiaDbContext _context;
    private readonly DbSet<Projeto> _dbSet; 
    private readonly ILogger<RepositorioProjeto> _logger;

    public ILogger<UnitOfWork.UnitOfWork> Logger { get; }

    /// <inheritdoc/>
    public RepositorioProjeto(SkopiaDbContext context, ILogger<RepositorioProjeto> logger)
    {
        
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioProjeto.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "O logger não pode ser nulo para o RepositorioProjeto.");
        _dbSet = _context.Set<Projeto>();
    }

    public RepositorioProjeto(SkopiaDbContext context, ILogger<UnitOfWork.UnitOfWork> logger)
    {
        _context = context;
        Logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Projeto> CriarAsync(Projeto projeto)
    {
        _logger.LogInformation($"Criando novo projeto com Nome: {projeto.Nome}");
        await _dbSet.AddAsync(projeto);
        
        return projeto;
    }

    /// <inheritdoc/>
    public async Task<Projeto> AtualizarAsync(Projeto projeto)
    {
        _logger.LogInformation($"Atualizando projeto com Nome: {projeto.Nome}");
        _dbSet.Update(projeto);
        
        return projeto;
    }

    /// <inheritdoc/>
    public async Task<bool> ExcluirAsync(Guid id) 
    {
        _logger.LogInformation("Excluindo projeto com ID: {Id}", id);
        var projeto = await _dbSet.FindAsync(id);
        if (projeto == null)
        {
            
            return false;
        }
                
        _dbSet.Remove(projeto);
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<Projeto?> ObterPorIdAsync(Guid id)
    {       
        _logger.LogInformation("Obtendo projeto com ID: {Id}", id);
        return await _dbSet
            .Include(p => p.Tarefas) 
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Projeto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId)
    {
        _logger.LogInformation("Obtendo todos os projetos para o usuário com ID: {UsuarioId}", usuarioId);
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId) 
            .OrderBy(p => p.DataCriacao) 
            .ToListAsync(); 
    }


    /// <inheritdoc/>
    public async Task<bool> PossuiTarefasPendentesAsync(Guid projetoId) 
    {
        _logger.LogInformation("Verificando se o projeto com ID: {ProjetoId} possui tarefas pendentes.", projetoId);
        return await _context.Set<Tarefa>()
            .AnyAsync(t => t.ProjetoId == projetoId && t.Status != StatusTarefa.Concluida); 
    }

    /// <inheritdoc/>
    public async Task<int> ObterContagemTarefasAsync(Guid projetoId) 
    {
        _logger.LogInformation("Obtendo contagem de tarefas para o projeto com ID: {ProjetoId}", projetoId);
        return await _context.Set<Tarefa>()
            .CountAsync(t => t.ProjetoId == projetoId); 
    }

  
    
}