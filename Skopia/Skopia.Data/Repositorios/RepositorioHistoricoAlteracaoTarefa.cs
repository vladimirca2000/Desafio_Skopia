using Microsoft.EntityFrameworkCore; // Necessário para DbContext, DbSet, ToListAsync, Where, OrderBy, AsNoTracking
using Skopia.Data.Context; // Assumindo que SkopiaDbContext está neste namespace
using Skopia.Domain.Entidades; // Necessário para a entidade HistoricoAlteracaoTarefa
using Skopia.Domain.Interfaces.Repositorios; // Necessário para a interface IRepositorioHistoricoAlteracaoTarefa
using Skopia.Domain.Repositorios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq; // Necessário para métodos de extensão LINQ como Where, OrderBy
using System.Threading.Tasks;

namespace Skopia.Data.Repositorios;

/// <summary>
/// Implementa as operações de acesso a dados (CRUD - Create, Read, Update, Delete) para a entidade
/// <see cref="HistoricoAlteracaoTarefa"/>, aderindo estritamente ao contrato definido por
/// <see cref="IRepositorioHistoricoAlteracaoTarefa"/>.
///
/// Este repositório é responsável por abstrair a lógica de persistência de dados do domínio,
/// focando na criação de novos registros de histórico e na obtenção do histórico
/// de alterações de tarefas por ID de tarefa.
///
/// O mecanismo de soft delete é gerenciado automaticamente pelo <see cref="SkopiaDbContext"/>
/// através de [filtros globais de consulta (Global Query Filters)](https://learn.microsoft.com/en-us/ef/core/querying/filters)
/// para leituras e [interceptors](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8/whatsnew#interceptors)
/// ou [eventos do SaveChanges](https://learn.microsoft.com/en-us/ef/core/saving/events) para remoções.
/// Isso garante que dados logicamente deletados não sejam expostos em consultas de leitura
/// e que as exclusões sejam lógicas por padrão, mantendo a camada de repositório
/// limpa e focada na lógica de negócio específica.
/// </summary>
public class RepositorioHistoricoAlteracaoTarefa : IRepositorioHistoricoAlteracaoTarefa
{
    private readonly SkopiaDbContext _context;
    // Referência direta ao DbSet para a entidade HistoricoAlteracaoTarefa.
    // Isso otimiza o acesso e evita chamadas repetidas a _context.Set<TEntity>().
    private readonly DbSet<HistoricoAlteracaoTarefa> _dbSet;

    /// <summary>
    /// Construtor que recebe a instância do <see cref="SkopiaDbContext"/> via Injeção de Dependência.
    /// A Injeção de Dependência (DI) é um pilar fundamental para a construção de aplicações
    /// escaláveis e de fácil manutenção. Ela promove:
    /// <list type="bullet">
    ///     <item>
    ///         <term>Testabilidade</term>
    ///         <description>Permite a substituição de dependências reais por mocks ou stubs em testes unitários.</description>
    ///     </item>
    ///     <item>
    ///         <term>Manutenibilidade</term>
    ///         <description>Facilita a alteração e extensão do código, pois os componentes são fracamente acoplados.</description>
    ///     </item>
    ///     <item>
    ///         <term>Reusabilidade</term>
    ///         <description>Componentes podem ser reutilizados em diferentes contextos sem modificações.</description>
    ///     </item>
    ///     <item>
    ///         <term>Configurabilidade</term>
    ///         <description>Permite configurar diferentes implementações para uma mesma interface em tempo de execução.</description>
    ///     </item>
    /// </list>
    /// </summary>
    /// <param name="context">A instância do SkopiaDbContext.</param>
    /// <exception cref="ArgumentNullException">Lançada se o contexto fornecido for nulo, garantindo a integridade do objeto.</exception>
    public RepositorioHistoricoAlteracaoTarefa(SkopiaDbContext context)
    {
        // Validação de argumento para garantir que o contexto não seja nulo.
        // Isso evita NullReferenceExceptions em tempo de execução.
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioHistoricoAlteracaoTarefa.");
        // Obtém o DbSet específico para HistoricoAlteracaoTarefa.
        // Usar _context.Set<TEntity>() é uma prática robusta que funciona mesmo se o DbSet não estiver
        // exposto como uma propriedade DbSet<TEntity> no DbContext, tornando o código mais flexível.
        _dbSet = _context.Set<HistoricoAlteracaoTarefa>();
    }

    /// <summary>
    /// Adiciona um novo registro de histórico de alteração de tarefa ao contexto de persistência de forma assíncrona.
    /// </summary>
    /// <param name="historicoAlteracaoTarefa">O objeto <see cref="HistoricoAlteracaoTarefa"/> a ser adicionado.</param>
    /// <returns>O objeto <see cref="HistoricoAlteracaoTarefa"/> que foi adicionado (com seu estado atual no Change Tracker).</returns>
    public async Task<HistoricoAlteracaoTarefa> CriarAsync(HistoricoAlteracaoTarefa historicoAlteracaoTarefa)
    {
        // O método AddAsync adiciona a entidade ao Change Tracker do Entity Framework Core.
        // O Change Tracker monitora o estado das entidades (Added, Modified, Deleted, Unchanged, Detached)
        // e é responsável por gerar os comandos SQL apropriados quando SaveChangesAsync() é chamado.
        // A operação é assíncrona, o que é uma boa prática para operações de I/O, liberando a thread
        // para outras tarefas enquanto aguarda a conclusão da operação.
        await _dbSet.AddAsync(historicoAlteracaoTarefa);

        // IMPORTANTE: SaveChangesAsync() NÃO é chamado aqui.
        // Esta é uma aderência crucial ao padrão Unit of Work (UoW).
        // A responsabilidade de persistir as alterações no banco de dados é delegada ao
        // IUnitOfWork.CommitAsync() (ou SaveChangesAsync() do DbContext) que será invocado
        // na camada de aplicação (e.g., Service Layer).
        // Isso garante que múltiplas operações de repositório (e.g., criar um histórico E atualizar uma tarefa)
        // possam ser agrupadas em uma única transação atômica, promovendo a consistência dos dados
        // e otimizando as operações de banco de dados ao reduzir o número de roundtrips.
        // Se SaveChangesAsync() fosse chamado aqui, cada criação de histórico seria uma transação separada,
        // o que poderia levar a inconsistências se uma operação subsequente falhasse.
        return historicoAlteracaoTarefa; // Retorna a entidade que foi adicionada.
    }

    /// <summary>
    /// Obtém de forma assíncrona todos os registros de histórico de alteração associados a um determinado ID de tarefa.
    /// </summary>
    /// <param name="tarefaId">O ID único (Guid) da tarefa à qual os registros de histórico pertencem.</param>
    /// <returns>Uma coleção (<see cref="IEnumerable{T}"/>) de <see cref="HistoricoAlteracaoTarefa"/>.</returns>
    public async Task<IEnumerable<HistoricoAlteracaoTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId)
    {
        // 1. Aplicação do Global Query Filter para Soft Delete:
        //    O filtro global de soft delete configurado no SkopiaDbContext (e.g., via HasQueryFilter)
        //    é aplicado automaticamente a esta consulta. Isso significa que apenas registros onde
        //    HistoricoAlteracaoTarefa.EstaDeletado == false serão retornados, sem a necessidade
        //    de uma cláusula .Where(h => h.EstaDeletado == false) explícita no repositório.
        //    Isso centraliza a lógica de soft delete no DbContext, garantindo consistência
        //    e evitando repetição de código (DRY - Don't Repeat Yourself).
        //    Caso seja necessário incluir registros deletados em uma consulta específica,
        //    poder-se-ia usar .IgnoreQueryFilters() antes do .Where().

        // 2. Otimização de Leitura com AsNoTracking():
        //    AsNoTracking() é utilizado para otimização de performance em cenários de leitura.
        //    Quando você não pretende modificar as entidades retornadas (como é o caso de um histórico
        //    que é geralmente imutável após a criação), AsNoTracking() impede que o EF Core as rastreie
        //    no Change Tracker. Os benefícios incluem:
        //    - Redução do consumo de memória: O Change Tracker não precisa armazenar uma cópia das entidades.
        //    - Melhoria na performance da consulta: Menos sobrecarga de processamento, pois o EF Core
        //      não precisa configurar o rastreamento de mudanças.
        //    É crucial usar AsNoTracking() apenas quando as entidades não serão atualizadas ou deletadas
        //    no mesmo contexto de DbContext, pois elas não serão monitoradas para persistência.

        // 3. Filtragem por ID da Tarefa:
        //    Where(h => h.TarefaId == tarefaId) filtra os registros para a tarefa específica.
        //    Para otimizar a performance desta consulta, especialmente em tabelas grandes,
        //    é altamente recomendável que a coluna `TarefaId` na tabela `HistoricoAlteracaoTarefa`
        //    tenha um índice de banco de dados (e.g., um índice não-clustered).
        //    Índices aceleram a recuperação de dados ao permitir que o banco de dados localize
        //    linhas rapidamente sem ter que escanear a tabela inteira.

        // 4. Ordenação Cronológica:
        //    OrderBy(h => h.DataAlteracao) é usado para garantir que o histórico seja retornado
        //    em ordem cronológica ascendente, o que é fundamental para a compreensão da sequência
        //    de eventos de alteração. Assumimos a existência de uma propriedade `DataAlteracao`
        //    na entidade `HistoricoAlteracaoTarefa` que registra o timestamp da alteração.
        //    Para cenários onde o histórico pode ser muito extenso, considere implementar paginação
        //    (e.g., usando Skip() e Take()) para evitar carregar um volume excessivo de dados
        //    na memória e melhorar a responsividade da aplicação.

        // 5. Execução Assíncrona e Materialização:
        //    ToListAsync() executa a consulta no banco de dados de forma assíncrona e materializa
        //    os resultados em uma lista. O await garante que a execução do método seja suspensa
        //    até que os dados sejam completamente recuperados do banco de dados, liberando a thread
        //    para outras operações. A lista é então retornada como IEnumerable<HistoricoAlteracaoTarefa>.
        return await _dbSet
                     .AsNoTracking()
                     .Where(h => h.TarefaId == tarefaId)
                     .OrderBy(h => h.DataModificacao) // Assumindo uma propriedade DataModificacao na entidade HistoricoAlteracaoTarefa
                     .ToListAsync();
    }
}