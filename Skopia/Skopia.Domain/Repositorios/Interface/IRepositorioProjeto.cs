using Skopia.Domain.Entidades; // Importa a entidade de domínio 'Projeto'

namespace Skopia.Domain.Repositorios.Interfaces;

/// <summary>
/// Define o contrato para a persistência e recuperação de entidades 'Projeto'.
/// Esta interface está na camada de domínio, garantindo que a lógica de negócio
/// não tenha dependência direta de como os dados são armazenados (banco de dados, etc.).
/// Todos os métodos retornam 'Task' para indicar que são operações assíncronas,
/// o que é uma boa prática para operações de I/O (Input/Output) como acesso a banco de dados,
/// pois libera a thread de execução para outras tarefas enquanto aguarda o resultado,
/// melhorando a responsividade da aplicação.
/// </summary>
public interface IRepositorioProjeto
{
    /// <summary>
    /// Obtém todos os projetos associados a um usuário específico.
    /// </summary>
    /// <param name="usuarioId">O ID (Guid) do usuário.</param>
    /// <returns>Uma coleção de entidades 'Projeto'.</returns>
    Task<IEnumerable<Projeto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId);

    /// <summary>
    /// Obtém um projeto pelo seu ID.
    /// </summary>
    /// <param name="id">O ID (Guid) do projeto.</param>
    /// <returns>O objeto 'Projeto' se encontrado, ou null.</returns>
    Task<Projeto?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Adiciona um novo projeto ao armazenamento.
    /// </summary>
    /// <param name="projeto">A entidade 'Projeto' a ser criada. O ID do projeto
    /// deve ser gerado antes de chamar este método, idealmente pela própria entidade
    /// ou por um serviço de domínio.</param>
    /// <returns>A entidade 'Projeto' criada (com o ID gerado e outros valores padrão, se aplicável).</returns>
    Task<Projeto> CriarAsync(Projeto projeto);

    /// <summary>
    /// Atualiza um projeto existente no armazenamento.
    /// </summary>
    /// <param name="projeto">A entidade 'Projeto' com os dados atualizados.
    /// O método deve usar o ID da entidade para localizar o registro a ser atualizado.</param>
    /// <returns>A entidade 'Projeto' atualizada.</returns>
    Task<Projeto> AtualizarAsync(Projeto projeto);

    /// <summary>
    /// Exclui um projeto do armazenamento.
    /// </summary>
    /// <param name="id">O ID (Guid) do projeto a ser excluído.</param>
    /// <returns>True se o projeto foi excluído com sucesso, False caso contrário (e.g., projeto não encontrado).</returns>
    Task<bool> ExcluirAsync(Guid id);

    /// <summary>
    /// Verifica se um projeto possui tarefas pendentes (Status Pendente ou EmAndamento).
    /// Esta é uma consulta de otimização/conveniência que a camada de dados pode implementar eficientemente
    /// para evitar carregar todas as tarefas do projeto para o domínio.
    /// A regra de negócio completa para 'PodeSerRemovido' reside na entidade 'Projeto',
    /// que pode usar este método do repositório para sua validação.
    /// </summary>
    /// <param name="projetoId">O ID (Guid) do projeto.</param>
    /// <returns>True se o projeto tiver tarefas pendentes, False caso contrário.</returns>
    Task<bool> PossuiTarefasPendentesAsync(Guid projetoId);

    /// <summary>
    /// Obtém a contagem total de tarefas para um projeto específico.
    /// Similar ao método de verificação de tarefas pendentes, este método otimiza
    /// a recuperação de uma contagem simples diretamente do armazenamento de dados,
    /// sem a necessidade de carregar todas as entidades de tarefa para a memória.
    /// </summary>
    /// <param name="projetoId">O ID (Guid) do projeto.</param>
    /// <returns>O número total de tarefas no projeto.</returns>
    Task<int> ObterContagemTarefasAsync(Guid projetoId);

    

}