using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades; 
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces;

namespace Skopia.Data.Repositorios;


public class RepositorioProjeto : IRepositorioProjeto
{
    private readonly SkopiaDbContext _context;
    private readonly ILogger<RepositorioProjeto> _logger;

    [ActivatorUtilitiesConstructor]
    public RepositorioProjeto(SkopiaDbContext context, ILogger<RepositorioProjeto> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Projeto> CriarAsync(Projeto projeto)
    {
        _logger.LogInformation($"Criando novo projeto com Nome: {projeto.Nome}");
        await _context.Set<Projeto>().AddAsync(projeto);

        return projeto;
    }

    /// <inheritdoc/>
    public async Task<Projeto> AtualizarAsync(Projeto projeto)
    {
        _logger.LogInformation($"Atualizando projeto com Nome: {projeto.Nome}");
        _context.Set<Projeto>().Update(projeto);

        return projeto;
    }

    /// <inheritdoc/>
    public async Task<bool> ExcluirAsync(Guid id)
    {
        _logger.LogInformation("Excluindo projeto com ID: {Id}", id);
        var projeto = await _context.Set<Projeto>().FindAsync(id);
        if (projeto == null)
        {
            return false;
        }

        _context.Set<Projeto>().Remove(projeto);

        return true;
    }

    /// <inheritdoc/>
    public async Task<Projeto?> ObterPorIdAsync(Guid id)
    {
        _logger.LogInformation("Obtendo projeto com ID: {Id}", id);
        return await _context.Set<Projeto>()
            .Include(p => p.Tarefas)
            .ThenInclude(c => c.Comentarios)
            .Include(p => p.Tarefas)
            .ThenInclude(c => c.Historico)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Projeto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId)
    {
        _logger.LogInformation("Obtendo todos os projetos para o usuário com ID: {UsuarioId}", usuarioId);
        return await _context.Set<Projeto>()
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

    /// <inheritdoc/>
    public async Task<bool> ProjetoPossuiTarefasPendentesOuCanceladasAsync(Guid projetoId)
    {
        _logger.LogInformation("Verificando se o projeto com ID: {ProjetoId} possui tarefas pendentes ou canceladas.", projetoId);
        return await _context.Set<Projeto>()
            .Include(p => p.Tarefas)
            .AnyAsync(p => p.Id == projetoId &&
                           p.Tarefas.Any(t => t.Status != StatusTarefa.Concluida && t.Status != StatusTarefa.Cancelada));
    }
}