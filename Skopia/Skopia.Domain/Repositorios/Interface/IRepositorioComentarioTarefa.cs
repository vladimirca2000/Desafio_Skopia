using Skopia.Domain.Entidades; // Necessário para referenciar a entidade ComentarioTarefa
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skopia.Domain.Repositorios.Interfaces;

/// <summary>
/// Define o contrato para operações de acesso a dados (Data Access Operations)
/// específicas da entidade <see cref="ComentarioTarefa"/>.
/// Esta interface herda de <see cref="IRepositoryBase{TEntity}"/> para operações CRUD genéricas,
/// promovendo reuso de código e padronização das interações com o banco de dados.
/// </summary>
public interface IRepositorioComentarioTarefa
{
    /// <summary>
    /// Obtém de forma assíncrona todos os comentários associados a um determinado ID de tarefa.
    /// Este método é crucial para exibir o histórico de comentários de uma tarefa específica,
    /// otimizando a consulta ao banco de dados para buscar apenas os dados relevantes.
    /// </summary>
    /// <param name="tarefaId">O <see cref="Guid"/> único da tarefa à qual os comentários pertencem.</param>
    /// <returns>
    /// Uma <see cref="IEnumerable{T}"/> de <see cref="ComentarioTarefa"/> contendo todos os comentários
    /// encontrados para o <paramref name="tarefaId"/> especificado.
    /// Retorna uma coleção vazia se nenhum comentário for encontrado, nunca <c>null</c>.
    /// </returns>
    Task<IEnumerable<ComentarioTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId);

    /// <summary>
    /// Cria de forma assíncrona um novo comentário de tarefa no repositório.
    /// Embora <see cref="IRepositoryBase{TEntity}"/> já forneça um método <c>AddAsync</c>,
    /// este método específico <c>CriarAsync</c> pode ser utilizado para encapsular
    /// lógicas de negócio adicionais que são exclusivas do processo de criação de um <see cref="ComentarioTarefa"/>,
    /// como validações específicas, processamento de texto, ou integração com outros serviços
    /// antes da persistência. Se não houver lógica adicional, <c>AddAsync</c> pode ser preferível.
    /// </summary>
    /// <param name="comentario">O objeto <see cref="ComentarioTarefa"/> a ser persistido.</param>
    /// <returns>
    /// O objeto <see cref="ComentarioTarefa"/> que foi criado e persistido,
    /// geralmente com seu <c>Id</c> gerado pelo banco de dados, se aplicável.
    /// </returns>
    Task<ComentarioTarefa> CriarAsync(ComentarioTarefa comentario);
}
