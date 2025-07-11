using Skopia.Domain.Entidades; // Importa a entidade de domínio 'ComentarioTarefa'
using System; // Necessário para Guid
using System.Collections.Generic; // Necessário para IEnumerable
using System.Threading.Tasks; // Necessário para Task

namespace Skopia.Domain.Repositorios.Interfaces;

/// <summary>
/// Define o contrato (ou "Porta" em uma arquitetura hexagonal) para a persistência e recuperação
/// de entidades 'ComentarioTarefa'. Esta interface reside na camada de domínio, garantindo que a
/// lógica de negócio principal opere apenas com suas próprias entidades e não tenha dependência
/// direta de como os comentários são armazenados (banco de dados, serviço externo, etc.).
///
/// Ao definir esta interface no domínio, estamos aplicando o Princípio da Inversão de Dependência (DIP):
/// o domínio define a abstração que a camada de infraestrutura (dados) deve implementar.
/// Todos os métodos retornam 'Task' para indicar que são operações assíncronas, o que é uma
/// boa prática para operações de I/O, melhorando a escalabilidade e a responsividade da aplicação.
/// </summary>
public interface IRepositorioComentarioTarefa
{
    /// <summary>
    /// Obtém todos os comentários associados a um ID de tarefa específico de forma assíncrona.
    /// </summary>
    /// <param name="tarefaId">O ID (Guid) da tarefa cujos comentários serão recuperados.</param>
    /// <returns>
    /// Uma coleção de entidades 'ComentarioTarefa'. Retorna uma coleção vazia (não null)
    /// se nenhum comentário for encontrado para o 'tarefaId' fornecido.
    /// </returns>
    Task<IEnumerable<ComentarioTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId);

    /// <summary>
    /// Adiciona um novo comentário de tarefa ao armazenamento de forma assíncrona.
    /// </summary>
    /// <param name="comentario">A entidade 'ComentarioTarefa' a ser criada.</param>
    /// <returns>
    /// A entidade 'ComentarioTarefa' criada, que pode incluir um ID gerado pelo sistema
    /// de persistência (ex: banco de dados) ou quaisquer outros valores padrão definidos
    /// durante a operação de criação.
    /// </returns>
    Task<ComentarioTarefa> CriarAsync(ComentarioTarefa comentario);
}
