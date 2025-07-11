using Microsoft.EntityFrameworkCore;
using Skopia.Domain.Entidades;
using Skopia.Domain.Repositorios.Interfaces;

namespace Skopia.Data.Repositorios;

/// <summary>
/// Implementa as operações de acesso a dados para a entidade <see cref="ComentarioTarefa"/>.
/// Este repositório foca apenas na criação de novos comentários e na obtenção de comentários
/// por ID de tarefa, conforme as necessidades específicas da interface <see cref="IRepositorioComentarioTarefa"/>.
/// O mecanismo de soft delete é gerenciado automaticamente pelo <see cref="SkopiaDbContext"/>
/// via filtros globais de consulta, simplificando o código do repositório e garantindo
/// que dados logicamente deletados não sejam expostos em consultas de leitura.
/// </summary>
public class RepositorioComentarioTarefa : IRepositorioComentarioTarefa
{
    private readonly SkopiaDbContext _context;
    private readonly DbSet<ComentarioTarefa> _dbSet; // Referência direta ao DbSet para a entidade ComentarioTarefa

    /// <summary>
    /// Construtor que recebe a instância do <see cref="SkopiaDbContext"/> via injeção de dependência.
    /// Esta é uma prática recomendada para desacoplar a classe do contexto do banco de dados,
    /// facilitando testes unitários e a manutenção.
    /// Inicializa o DbContext e obtém uma referência tipada ao DbSet para <see cref="ComentarioTarefa"/>,
    /// que será usada para interagir com a tabela correspondente no banco de dados.
    /// </summary>
    /// <param name="context">A instância do SkopiaDbContext.</param>
    /// <exception cref="ArgumentNullException">Lançada se o contexto fornecido for nulo,
    /// garantindo que o repositório sempre opere com um contexto válido.</exception>
    public RepositorioComentarioTarefa(SkopiaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<ComentarioTarefa>(); // Obtém o DbSet específico para ComentarioTarefa
    }

    /// <summary>
    /// Cria de forma assíncrona um novo comentário de tarefa no repositório.
    /// A entidade é adicionada ao Change Tracker do Entity Framework Core, que a monitora
    /// para futuras operações de persistência. O uso de <c>AddAsync</c> é preferível em
    /// operações assíncronas para evitar bloqueios de thread.
    /// As mudanças serão efetivamente persistidas no banco de dados somente quando
    /// <see cref="IUnitOfWork.CommitAsync"/> (ou <see cref="DbContext.SaveChangesAsync"/>) for chamado.
    /// Isso permite agrupar múltiplas operações de escrita em uma única transação atômica.
    /// </summary>
    /// <param name="comentario">O objeto <see cref="ComentarioTarefa"/> a ser persistido.</param>
    /// <returns>O objeto <see cref="ComentarioTarefa"/> que foi adicionado (com seu estado atual no Change Tracker).
    /// Este objeto pode conter valores gerados pelo banco de dados, como o ID, após a operação de adição.</returns>
    public async Task<ComentarioTarefa> CriarAsync(ComentarioTarefa comentario)
    {
        await _dbSet.AddAsync(comentario);
        // IMPORTANTE: SaveChangesAsync() NÃO é chamado aqui.
        // A responsabilidade de persistir as mudanças no banco de dados é do IUnitOfWork.CommitAsync().
        // Isso garante que o repositório foca apenas em "preparar" as entidades para a persistência,
        // enquanto o Unit of Work gerencia a unidade de trabalho e a transação.
        return comentario;
    }

    /// <summary>
    /// Obtém de forma assíncrona todos os comentários associados a um determinado ID de tarefa.
    /// </summary>
    /// <param name="tarefaId">O <see cref="Guid"/> único da tarefa à qual os comentários pertencem.</param>
    /// <returns>
    /// Uma coleção de <see cref="ComentarioTarefa"/>. Retorna uma lista vazia se
    /// não houver comentários para o <paramref name="tarefaId"/> fornecido, ou se todos os
    /// comentários existentes estiverem marcados como deletados.
    /// </returns>
    public async Task<IEnumerable<ComentarioTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId)
    {
        // O filtro global de soft delete já atua aqui, então 'c.EstaDeletado == false' não é necessário.
        // .AsNoTracking() otimiza a consulta para cenários de leitura, não rastreia as entidades.
        return await _dbSet
                     .AsNoTracking()
                     .Where(c => c.TarefaId == tarefaId) 
                     .OrderBy(c => c.DataComentario) 
                     .ToListAsync(); 
    }
}
