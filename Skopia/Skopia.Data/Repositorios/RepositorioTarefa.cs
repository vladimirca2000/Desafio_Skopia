using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades; 
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks; 

namespace Skopia.Data.Repositorios
{
    
    public class RepositorioTarefa : IRepositorioTarefa
    {
        private readonly SkopiaDbContext _context;
        private readonly ILogger<RepositorioTarefa> _logger;
        private readonly DbSet<Tarefa> _dbSet;

        /// <inheritdoc/>
        public RepositorioTarefa(SkopiaDbContext context, ILogger<RepositorioTarefa> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioTarefa.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "O logger não pode ser nulo para o RepositorioTarefa.");
            _dbSet = _context.Set<Tarefa>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tarefa>> ObterTodosPorProjetoIdAsync(Guid projetoId)
        {
            _logger.LogInformation("Obtendo todas as tarefas para o projeto com ID: {ProjetoId}", projetoId);
            return await _dbSet
                         .AsNoTracking()
                         .Where(t => t.ProjetoId == projetoId)
                         .OrderBy(t => t.DataCriacao)
                         .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Tarefa?> ObterPorIdAsync(Guid id)
        {
            _logger.LogInformation("Obtendo tarefa com ID: {Id}", id);
            
            return await _dbSet.FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Tarefa> CriarAsync(Tarefa tarefa)
        {
            _logger.LogInformation("Criando nova tarefa com título: {Titulo}", tarefa.Titulo);
            await _dbSet.AddAsync(tarefa);
            return tarefa; // Retorna a própria instância, que agora está sendo rastreada pelo DbContext.
        }

        /// <inheritdoc/>
        public async Task<Tarefa> AtualizarAsync(Tarefa tarefa)
        {
            _logger.LogInformation("Atualizando tarefa com ID: {Id}", tarefa.Id);
            _dbSet.Update(tarefa);
            return tarefa; 
        }

        /// <inheritdoc/>
        public async Task<bool> ExcluirAsync(Guid id)
        {
            _logger.LogInformation("Excluindo tarefa com ID: {Id}", id);
            var tarefa = await _dbSet.FindAsync(id);
            if (tarefa == null)
            {
                return false; 
            }
            
            _dbSet.Remove(tarefa);
            return true;
        }

        /// <inheritdoc/>
        public async Task<int> ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(Guid usuarioId, DateTime dataInicio)
        {
            _logger.LogInformation("Obtendo contagem de tarefas concluídas para o usuário com ID: {UsuarioId} desde a data: {DataInicio}", usuarioId, dataInicio);
            return await _dbSet
                         .CountAsync(t => t.UsuarioId == usuarioId &&
                                          t.Status == StatusTarefa.Concluida &&
                                          t.DataConclusao >= dataInicio);
        }

        /// <inheritdoc/>
        public async Task AdicionarHistoricoAsync(HistoricoAlteracaoTarefa historico)
        {
            _logger.LogInformation("Adicionando histórico de alteração para a tarefa com ID: {TarefaId}", historico.TarefaId);
            await _context.Set<HistoricoAlteracaoTarefa>().AddAsync(historico);
        }

        

        /// <inheritdoc/>
        public async Task<IEnumerable<Tarefa>> ObterTarefasAtrasadasAsync()
        {
            _logger.LogInformation("Obtendo tarefas atrasadas.");
            var hoje = DateTime.UtcNow.Date;

            return await _dbSet
                         .AsNoTracking() 
                         .Where(t => t.DataVencimento.HasValue && 
                                     t.DataVencimento.Value.Date < hoje && 
                                     t.Status != StatusTarefa.Concluida && 
                                     t.Status != StatusTarefa.Cancelada) 
                         .OrderBy(t => t.DataVencimento) 
                         .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tarefa>> ObterTarefasComVencimentoProximoAsync(TimeSpan periodo)
        {
            _logger.LogInformation("Obtendo tarefas com vencimento próximo dentro do período: {Periodo}", periodo);
            var agora = DateTime.UtcNow;
            
            var limiteSuperior = agora.Add(periodo);

            return await _dbSet
                         .AsNoTracking() 
                         .Where(t => t.DataVencimento.HasValue && 
                                     t.DataVencimento.Value >= agora && 
                                     t.DataVencimento.Value <= limiteSuperior &&
                                     t.Status != StatusTarefa.Concluida &&
                                     t.Status != StatusTarefa.Cancelada)
                         .OrderBy(t => t.DataVencimento) 
                         .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tarefa>> ObterTarefasPorPeriodoDeVencimentoAsync(DateTime dataInicioVencimento, DateTime dataFimVencimento)
        {
            _logger.LogInformation("Obtendo tarefas com vencimento entre as datas: {DataInicio} e {DataFim}", dataInicioVencimento, dataFimVencimento);
            var inicio = dataInicioVencimento.Date;
            
            var fim = dataFimVencimento.Date;

            return await _dbSet
                         .AsNoTracking() 
                         .Where(t => t.DataVencimento.HasValue && 
                                     t.DataVencimento.Value.Date >= inicio && 
                                     t.DataVencimento.Value.Date <= fim) 
                         .OrderBy(t => t.DataVencimento) 
                         .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> PossuiTarefasPendentesParaProjetoAsync(Guid projetoId)
        {
            _logger.LogInformation("Verificando se existem tarefas pendentes para o projeto com ID: {ProjetoId}", projetoId);
            return await _dbSet.AnyAsync(t => t.ProjetoId == projetoId &&
                                           t.Status != StatusTarefa.Concluida &&
                                           t.Status != StatusTarefa.Cancelada);
        }
    }
}