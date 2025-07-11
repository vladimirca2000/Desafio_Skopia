using Microsoft.EntityFrameworkCore; // Necessário para DbContext, DbSet, ToListAsync, FindAsync, AnyAsync, CountAsync, AddAsync
using Skopia.Domain.Entidades; // Necessário para as entidades Tarefa e HistoricoAlteracaoTarefa
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces; // Necessário para a interface IRepositorioTarefa

namespace Skopia.Data.Repositorios;

/// <summary>
/// Implementa as operações de acesso a dados para a entidade <see cref="Tarefa"/>,
/// conforme o contrato definido pela interface <see cref="IRepositorioTarefa"/>.
/// Este repositório gerencia a persistência de tarefas, incluindo operações CRUD,
/// consultas específicas por projeto e usuário, e a adição de registros de histórico.
///
/// O soft delete é gerenciado de forma transparente pelo <see cref="SkopiaDbContext"/>:
/// <list type="bullet">
///     <item>
///         <term>Consultas de Leitura</term>
///         <description>Utilizam filtros globais de consulta (Global Query Filters) configurados no <see cref="SkopiaDbContext"/>
///         para excluir automaticamente registros marcados como deletados (<c>EstaDeletado == false</c>) de todos os resultados.
///         Isso garante que as aplicações cliente não vejam dados logicamente excluídos por padrão.</description>
///     </item>
///     <item>
///         <term>Operação de Exclusão (<c>Remove()</c>)</term>
///         <description>É interceptada por um <c>SaveChangesInterceptor</c> configurado no <see cref="SkopiaDbContext"/>
///         que, em vez de gerar um comando <c>DELETE</c> SQL, gera um comando <c>UPDATE</c> para marcar a entidade
///         como deletada logicamente (<c>EstaDeletado = true</c>) e preencher a data de exclusão (<c>QuandoDeletou</c>).
///         Isso preserva a integridade dos dados históricos e de auditoria.</description>
///     </item>
/// </list>
/// A persistência das alterações no banco de dados é responsabilidade do
/// <see cref="IUnitOfWork.CommitAsync()"/> (assumindo que você tem uma interface IUnitOfWork e sua implementação),
/// garantindo que as operações sejam atômicas e transacionais.
/// </summary>
public class RepositorioTarefa : IRepositorioTarefa
{
    private readonly SkopiaDbContext _context;
    // DbSet específico para a entidade Tarefa, otimizando o acesso e garantindo tipagem forte.
    // É uma boa prática usar _context.Set<TEntity>() para obter o DbSet, especialmente em repositórios genéricos
    // ou quando o DbSet não é exposto diretamente como uma propriedade no DbContext.
    private readonly DbSet<Tarefa> _dbSet;

    /// <summary>
    /// Construtor que recebe a instância do <see cref="SkopiaDbContext"/> via Injeção de Dependência.
    /// O DbContext é a unidade de trabalho do Entity Framework Core e gerencia o estado das entidades
    /// dentro de uma sessão, rastreando as alterações e persistindo-as no banco de dados.
    /// </summary>
    /// <param name="context">A instância do SkopiaDbContext.</param>
    /// <exception cref="ArgumentNullException">Lançada se o contexto for nulo, garantindo que a dependência seja fornecida
    /// e evitando NullReferenceExceptions em tempo de execução.</exception>
    public RepositorioTarefa(SkopiaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioTarefa.");
        _dbSet = _context.Set<Tarefa>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Tarefa>> ObterTodosPorProjetoIdAsync(Guid projetoId)
    {
        // AsNoTracking() é usado para consultas de leitura que não precisam rastrear alterações.
        // Isso melhora o desempenho pois o Entity Framework Core não precisa configurar o Change Tracker
        // para as entidades retornadas, reduzindo o consumo de memória e CPU.
        return await _dbSet
                     .AsNoTracking()
                     .Where(t => t.ProjetoId == projetoId)
                     .OrderBy(t => t.DataCriacao)
                     .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Tarefa?> ObterPorIdAsync(Guid id)
    {
        // AsNoTracking() é aplicado aqui também, pois é uma operação de leitura simples.
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Tarefa> CriarAsync(Tarefa tarefa)
    {
        // AddAsync() adiciona a entidade ao contexto, marcando-a como 'Added'.
        // A persistência real no banco de dados ocorrerá quando SaveChangesAsync() (via CommitAsync()) for chamado.
        await _dbSet.AddAsync(tarefa);
        return tarefa; // Retorna a própria instância, que agora está sendo rastreada pelo DbContext.
    }

    /// <inheritdoc/>
    public async Task<Tarefa> AtualizarAsync(Tarefa tarefa)
    {
        // Update() marca a entidade como 'Modified'. O EF Core comparará o estado atual
        // com o estado original (se rastreado) para gerar um UPDATE SQL eficiente.
        _dbSet.Update(tarefa);
        return tarefa; // Retorna a instância atualizada.
    }

    /// <inheritdoc/>
    public async Task<bool> ExcluirAsync(Guid id)
    {
        var tarefa = await _dbSet.FindAsync(id); // FindAsync primeiro verifica o cache do DbContext, depois o banco de dados.
        if (tarefa == null)
        {
            return false; // Tarefa não encontrada, não há o que excluir.
        }
        // Remove() marca a entidade para exclusão. Devido ao interceptor de soft delete,
        // isso resultará em um UPDATE e não um DELETE físico.
        _dbSet.Remove(tarefa);
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(Guid usuarioId, DateTime dataInicio)
    {
        // CountAsync() é otimizado para retornar apenas a contagem, sem carregar as entidades para a memória.
        // O filtro global de soft delete já é aplicado aqui automaticamente.
        return await _dbSet
                     .CountAsync(t => t.UsuarioId == usuarioId &&
                                      t.Status == StatusTarefa.Concluida &&
                                      t.DataConclusao >= dataInicio);
    }

    /// <inheritdoc/>
    public async Task AdicionarHistoricoAsync(HistoricoAlteracaoTarefa historico)
    {
        // Adiciona um registro de histórico. Note que HistoricoAlteracaoTarefa pode não ter um soft delete,
        // pois registros de auditoria geralmente são mantidos permanentemente.
        await _context.Set<HistoricoAlteracaoTarefa>().AddAsync(historico);
    }

    // --- Novas Implementações para Gerenciamento de Prazos ---

    /// <inheritdoc/>
    public async Task<IEnumerable<Tarefa>> ObterTarefasAtrasadasAsync()
    {
        // Retorna tarefas que estão atrasadas, ou seja, cuja DataVencimento já passou
        // e que não estão Concluídas nem Canceladas.
        // O filtro global de soft delete do DbContext já garante que apenas tarefas não deletadas sejam consideradas.

        // Usamos DateTime.UtcNow para garantir consistência em ambientes distribuídos e fusos horários diferentes.
        // .Date é usado para comparar apenas a porção da data, ignorando a hora,
        // o que é comum para "vencimento no dia X". Se a precisão da hora for necessária, remova .Date.
        var hoje = DateTime.UtcNow.Date;

        return await _dbSet
                     .AsNoTracking() // Para otimizar a leitura, já que não haverá atualização.
                     .Where(t => t.DataVencimento.HasValue && // Garante que a tarefa tem um DataVencimento definido.
                                 t.DataVencimento.Value.Date < hoje && // DataVencimento já passou em relação ao dia de hoje.
                                 t.Status != StatusTarefa.Concluida && // Não está concluída.
                                 t.Status != StatusTarefa.Cancelada) // Nem cancelada (ainda é uma tarefa "ativa").
                     .OrderBy(t => t.DataVencimento) // Ordena as tarefas atrasadas pela data de vencimento, as mais antigas primeiro.
                     .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Tarefa>> ObterTarefasComVencimentoProximoAsync(TimeSpan periodo)
    {
        // Retorna tarefas que vencem em um período próximo a partir de agora.
        // Considera tarefas ativas (não concluídas/canceladas).

        // Usamos DateTime.UtcNow para consistência.
        var agora = DateTime.UtcNow;
        // Calcula o limite superior do período de vencimento.
        var limiteSuperior = agora.Add(periodo);

        return await _dbSet
                     .AsNoTracking() // Otimização para leitura.
                     .Where(t => t.DataVencimento.HasValue && // Garante que a tarefa tem um DataVencimento.
                                 t.DataVencimento.Value >= agora && // Vence a partir de agora (inclusive).
                                 t.DataVencimento.Value <= limiteSuperior && // Vence dentro do período especificado.
                                 t.Status != StatusTarefa.Concluida && // Não está concluída.
                                 t.Status != StatusTarefa.Cancelada) // Nem cancelada.
                     .OrderBy(t => t.DataVencimento) // Ordena as tarefas pelo vencimento, as mais próximas primeiro.
                     .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Tarefa>> ObterTarefasPorPeriodoDeVencimentoAsync(DateTime dataInicioVencimento, DateTime dataFimVencimento)
    {
        // Obtém tarefas cuja DataVencimento cai dentro de um intervalo específico.
        // Isso é útil para planejamento e relatórios.

        // Normalizamos as datas para considerar apenas o dia, caso a DataVencimento seja armazenada com hora.
        // Isso garante que o intervalo seja inclusivo para o dia inteiro.
        var inicio = dataInicioVencimento.Date;
        // Para garantir que o último dia do intervalo seja totalmente incluído,
        // podemos adicionar um dia e subtrair um milissegundo, ou simplesmente usar .Date
        // e confiar que o EF Core traduzirá corretamente para um BETWEEN inclusivo.
        // A abordagem .Date <= .Date é geralmente suficiente e mais legível.
        var fim = dataFimVencimento.Date;

        return await _dbSet
                     .AsNoTracking() // Otimização para leitura.
                     .Where(t => t.DataVencimento.HasValue && // Garante que a tarefa tem um DataVencimento.
                                 t.DataVencimento.Value.Date >= inicio && // Vence a partir da data de início (inclusive).
                                 t.DataVencimento.Value.Date <= fim) // E até a data de fim (inclusive).
                     .OrderBy(t => t.DataVencimento) // Ordena as tarefas pelo vencimento.
                     .ToListAsync();
    }
}