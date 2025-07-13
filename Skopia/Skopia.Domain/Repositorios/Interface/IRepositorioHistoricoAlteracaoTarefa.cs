using Skopia.Domain.Entidades; // Importa a entidade de domínio 'HistoricoAlteracaoTarefa'

namespace Skopia.Domain.Repositorios.Interfaces;

/// <summary>
/// Define o contrato (ou "Porta" em uma arquitetura hexagonal) para a persistência e recuperação
/// de entidades 'HistoricoAlteracaoTarefa'. Esta interface reside na camada de domínio, garantindo que a
/// lógica de negócio principal opere apenas com suas próprias entidades e não tenha dependência
/// direta de como o histórico é armazenado (banco de dados, serviço externo, etc.).
///
/// Ao definir esta interface no domínio, estamos aplicando o Princípio da Inversão de Dependência (DIP):
/// o domínio define a abstração que a camada de infraestrutura (dados) deve implementar.
/// Todos os métodos retornam 'Task' para indicar que são operações assíncronas, o que é uma
/// boa prática para operações de I/O, melhorando a escalabilidade e a responsividade da aplicação.
/// </summary>
public interface IRepositorioHistoricoAlteracaoTarefa
{
    /// <summary>
    /// Obtém todos os históricos de alteração associados a um ID de tarefa específico de forma assíncrona.
    /// </summary>
    /// <param name="tarefaId">O ID (Guid) da tarefa cujo histórico será recuperado.</param>
    /// <returns>
    /// Uma coleção de entidades 'HistoricoAlteracaoTarefa'. Retorna uma coleção vazia (não null)
    /// se nenhum histórico for encontrado para o 'tarefaId' fornecido.
    /// </returns>
    Task<IEnumerable<HistoricoAlteracaoTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId);

    /// <summary>
    /// Adiciona um novo histórico de alteração de tarefa ao armazenamento de forma assíncrona.
    /// </summary>
    /// <param name="historicoAlteracaoTarefa">A entidade 'HistoricoAlteracaoTarefa' a ser criada.</param>
    /// <returns>
    /// A entidade 'HistoricoAlteracaoTarefa' criada, que pode incluir um ID gerado pelo sistema
    /// de persistência (ex: banco de dados) ou quaisquer outros valores padrão definidos
    /// durante a operação de criação.
    /// </returns>
    Task<HistoricoAlteracaoTarefa> CriarAsync(HistoricoAlteracaoTarefa historicoAlteracaoTarefa);
}
