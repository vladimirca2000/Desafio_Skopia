using Skopia.Servicos.Modelos; // Novo namespace para seus DTOs

namespace Skopia.Servicos.Interfaces;

/// <summary>
/// Define o contrato para os serviços de manipulação de tarefas.
/// Esta interface expõe as operações de negócio relacionadas a tarefas,
/// trabalhando com DTOs (Data Transfer Objects) para desacoplar a camada de aplicação
/// da camada de domínio.
/// </summary>
public interface IServicoTarefa
{
    /// <summary>
    /// Obtém todas as tarefas associadas a um projeto específico.
    /// </summary>
    /// <param name="projetoId">O ID do projeto cujas tarefas serão recuperadas (Guid).</param>
    /// <returns>Uma coleção de objetos TarefaDto.</returns>
    Task<IEnumerable<TarefaDto>> ObterTodosPorProjetoIdAsync(Guid projetoId);

    /// <summary>
    /// Obtém uma tarefa específica pelo seu ID.
    /// </summary>
    /// <param name="id">O ID da tarefa a ser recuperada (Guid).</param>
    /// <returns>O objeto TarefaDto correspondente, ou null se não for encontrado.</returns>
    Task<TarefaDto?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <param name="criarTarefaDto">O DTO contendo os dados para criação da tarefa.</param>
    /// <returns>O objeto TarefaDto da tarefa criada.</returns>
    Task<TarefaDto> CriarAsync(CriarTarefaDto criarTarefaDto);

    /// <summary>
    /// Atualiza uma tarefa existente.
    /// </summary>
    /// <param name="id">O ID da tarefa a ser atualizada (Guid).</param>
    /// <param name="atualizarTarefaDto">O DTO contendo os dados atualizados da tarefa.</param>
    /// <returns>O objeto TarefaDto da tarefa atualizada.</returns>
    Task<TarefaDto> AtualizarAsync(Guid id, AtualizarTarefaDto atualizarTarefaDto);

    /// <summary>
    /// Exclui uma tarefa logicamente (soft delete).
    /// </summary>
    /// <param name="id">O ID da tarefa a ser excluída (Guid).</param>
    /// <returns>True se a tarefa foi excluída com sucesso, False caso contrário.</returns>
    Task<bool> ExcluirAsync(Guid id);

    /// <summary>
    /// Adiciona um novo comentário a uma tarefa.
    /// </summary>
    /// <param name="criarComentarioTarefaDto">O DTO contendo os dados do comentário a ser adicionado.</param>
    /// <returns>O objeto TarefaDto atualizado com o novo comentário.</returns>
    Task<TarefaDto> AdicionarComentarioAsync(CriarComentarioTarefaDto criarComentarioTarefaDto);

    /// <summary>
    /// Obtém um relatório de desempenho para um usuário específico.
    /// Este relatório pode incluir o número médio de tarefas concluídas, etc.
    /// </summary>
    /// <param name="usuarioId">O ID do usuário para o qual o relatório será gerado (Guid).</param>
    /// <returns>O objeto RelatorioDesempenhoDto contendo os dados do relatório.</returns>
    Task<RelatorioDesempenhoDto> ObterRelatorioDesempenhoUsuarioAsync(Guid usuarioId);
}