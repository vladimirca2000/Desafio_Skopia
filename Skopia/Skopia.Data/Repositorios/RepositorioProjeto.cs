using Microsoft.EntityFrameworkCore; // Necessário para DbContext, DbSet, ToListAsync, FirstOrDefaultAsync, AnyAsync, CountAsync
using Skopia.Domain.Entidades; // Necessário para a entidade Projeto
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces;

namespace Skopia.Data.Repositorios;

/// <summary>
/// Implementa as operações de acesso a dados para a entidade <see cref="Projeto"/>, conforme definido em <see cref="IRepositorioProjeto"/>.
/// Este repositório gerencia a persistência de projetos, incluindo sua criação, atualização,
/// exclusão lógica (soft delete) e recuperação, além de consultas específicas relacionadas a tarefas.
/// O soft delete é gerenciado transparentemente pelo <see cref="SkopiaDbContext"/> através de interceptors
/// e filtros de consulta globais, garantindo que as operações de leitura e exclusão lógica
/// sejam aplicadas corretamente sem a necessidade de lógica explícita no repositório.
/// Isso promove um código mais limpo e menos propenso a erros.
/// </summary>
public class RepositorioProjeto : IRepositorioProjeto
{
    private readonly SkopiaDbContext _context;
    private readonly DbSet<Projeto> _dbSet; // DbSet específico para a entidade Projeto, otimizando o acesso.

    /// <summary>
    /// Construtor que recebe a instância do <see cref="SkopiaDbContext"/> via injeção de dependência.
    /// O DbContext é a unidade de trabalho do Entity Framework Core e gerencia o estado das entidades
    /// dentro de uma transação ou operação.
    /// </summary>
    /// <param name="context">A instância do SkopiaDbContext. É uma dependência essencial para o repositório funcionar.</param>
    /// <exception cref="ArgumentNullException">Lançada se o contexto for nulo, garantindo que a dependência seja fornecida corretamente.</exception>
    public RepositorioProjeto(SkopiaDbContext context)
    {
        // Validação de argumento para garantir que o DbContext seja injetado corretamente.
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioProjeto.");
        // Obtém o DbSet para a entidade Projeto, permitindo operações específicas e tipadas.
        _dbSet = _context.Set<Projeto>();
    }

    /// <inheritdoc/>
    public async Task<Projeto> CriarAsync(Projeto projeto)
    {
        // Adiciona o projeto ao Change Tracker do DbContext.
        // Neste ponto, o objeto 'projeto' está no estado 'Added'.
        await _dbSet.AddAsync(projeto);
        // IMPORTANTE: SaveChangesAsync() NÃO é chamado aqui.
        // A persistência das mudanças no banco de dados é responsabilidade exclusiva
        // do IUnitOfWork.CommitAsync(), que será chamado na camada de aplicação/serviço.
        // Isso garante que múltiplas operações de repositório possam ser agrupadas em uma única transação atômica.
        return projeto;
    }

    /// <inheritdoc/>
    public async Task<Projeto> AtualizarAsync(Projeto projeto)
    {
        // Marca o projeto como modificado no Change Tracker.
        // O Entity Framework Core rastreará as mudanças no objeto 'projeto' e as persistirá
        // quando IUnitOfWork.CommitAsync() for invocado.
        // O objeto 'projeto' deve ter o Id de um registro existente para que a atualização funcione corretamente.
        _dbSet.Update(projeto);
        // IMPORTANTE: SaveChangesAsync() NÃO é chamado aqui, seguindo o padrão Unit of Work.
        return projeto;
    }

    /// <inheritdoc/>
    public async Task<bool> ExcluirAsync(Guid id) // RENOMEADO: Implementação de 'ExcluirAsync'
    {
        // Primeiro, tenta encontrar o projeto pelo ID.
        // O filtro global de soft delete já garante que apenas projetos *não* deletados logicamente serão encontrados aqui.
        // Se o projeto já estiver logicamente deletado, FindAsync não o retornará, e o método retornará false.
        var projeto = await _dbSet.FindAsync(id);
        if (projeto == null)
        {
            // Projeto não encontrado (nunca existiu) ou já estava logicamente deletado e, portanto, invisível para FindAsync.
            return false;
        }

        // Ao chamar Remove(), o interceptor de soft delete configurado no SkopiaDbContext
        // irá automaticamente interceptar esta operação. Em vez de gerar um comando DELETE SQL,
        // ele modificará o comando para um UPDATE que define 'EstaDeletado = true' e preenche 'QuandoDeletou'.
        // Isso mantém o registro no banco de dados, mas o marca como inativo para futuras consultas.
        _dbSet.Remove(projeto);
        // IMPORTANTE: SaveChangesAsync() NÃO é chamado aqui.
        // A efetivação da exclusão lógica ocorrerá no CommitAsync do Unit of Work.
        return true;
    }

    /// <inheritdoc/>
    public async Task<Projeto?> ObterPorIdAsync(Guid id)
    {
        // Include(p => p.Tarefas) realiza o "eager loading", carregando as tarefas relacionadas
        // junto com o projeto em uma única consulta ao banco de dados. Isso é uma prática recomendada
        // para evitar o problema de N+1 queries, onde uma consulta separada seria feita para cada projeto.
        // O filtro global de soft delete aplicado a 'Projeto' e 'Tarefa' garantirá
        // que apenas itens não deletados logicamente sejam retornados, se configurado corretamente no DbContext.
        // AsNoTracking() é usado para consultas de leitura, pois desabilita o rastreamento de mudanças do EF Core.
        // Isso otimiza a performance e o uso de memória, pois o EF não precisa monitorar o objeto para futuras atualizações.
        // É ideal para cenários onde o objeto retornado não será modificado e salvo de volta no banco de dados.
        return await _dbSet
            .Include(p => p.Tarefas) // Supondo que Projeto tenha uma propriedade de navegação ICollection<Tarefa> Tarefas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id); // Retorna o primeiro projeto com o Id correspondente ou null.
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Projeto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId)
    {
        // AsNoTracking() é essencial para otimização de consultas de leitura em coleções,
        // especialmente quando um grande número de registros pode ser retornado.
        // O filtro global de soft delete do DbContext já filtra projetos com EstaDeletado = true,
        // então não precisamos adicionar .Where(p => p.EstaDeletado == false) explicitamente,
        // mantendo o código mais limpo e focado na lógica de negócio.
        // Ordena por DataCriacao para garantir uma ordem consistente nos resultados,
        // o que é útil para exibição em interfaces de usuário.
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId) // Filtra os projetos pelo ID do usuário associado.
            .OrderBy(p => p.DataCriacao) // Ordena os resultados pela data de criação do projeto.
            .ToListAsync(); // Executa a consulta e retorna uma lista de projetos.
    }

    /// <inheritdoc/>
    public async Task<bool> PossuiTarefasPendentesAsync(Guid projetoId) // RENOMEADO: Implementação de 'PossuiTarefasPendentesAsync'
    {
        // Acessa o DbSet de Tarefas diretamente do _context para realizar a consulta.
        // O filtro global de soft delete também se aplica à entidade Tarefa,
        // garantindo que apenas tarefas ativas (não deletadas logicamente) sejam consideradas.
        // AnyAsync verifica a existência de qualquer elemento que satisfaça a condição.
        // É significativamente mais eficiente do que carregar todas as tarefas e depois verificar ou contar,
        // pois o banco de dados pode parar de procurar assim que encontrar a primeira correspondência.
        return await _context.Set<Tarefa>()
            .AnyAsync(t => t.ProjetoId == projetoId && t.Status != StatusTarefa.Concluida); // Verifica se existe alguma tarefa para o projeto que não esteja concluída.
    }

    /// <inheritdoc/>
    public async Task<int> ObterContagemTarefasAsync(Guid projetoId) // RENOMEADO: Implementação de 'ObterContagemTarefasAsync'
    {
        // Acessa o DbSet de Tarefas diretamente do _context para realizar a contagem.
        // O filtro global de soft delete também se aplica à entidade Tarefa,
        // garantindo que a contagem inclua apenas tarefas ativas (não deletadas logicamente).
        // CountAsync retorna a contagem de elementos que satisfazem a condição de forma eficiente,
        // delegando a contagem ao banco de dados.
        return await _context.Set<Tarefa>()
            .CountAsync(t => t.ProjetoId == projetoId); // Conta todas as tarefas associadas ao projeto especificado.
    }

    /// <inheritdoc/>
    
}