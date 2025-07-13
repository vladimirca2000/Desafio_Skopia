using Skopia.Servicos.Modelos; // Novo namespace para seus DTOs, seguindo a tradução

namespace Skopia.Servicos.Interfaces;

// <summary>
/// Define o contrato para os serviços de manipulação de projetos.
/// Esta interface expõe as operações de negócio relacionadas a projetos,
/// trabalhando com DTOs (Data Transfer Objects) para desacoplar a camada de aplicação
/// da camada de domínio.
/// </summary>
public interface IServicoProjeto
{
    /// <summary>
    /// Obtém todos os projetos associados a um usuário específico.
    /// </summary>
    /// <param name="usuarioId">O ID do usuário cujos projetos serão recuperados (Guid).</param>
    /// <returns>Uma coleção de objetos ProjetoDto.</returns>
    Task<IEnumerable<ProjetoDto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId);

    /// <summary>
    /// Obtém um projeto específico pelo seu ID.
    /// </summary>
    /// <param name="id">O ID do projeto a ser recuperado (Guid).</param>
    /// <returns>O objeto ProjetoDto correspondente, ou null se não for encontrado.</returns>
    Task<ProjetoDto?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Cria um novo projeto.
    /// </summary>
    /// <param name="projetoDto">O DTO contendo os dados para criação do projeto.</param>
    /// <returns>O objeto ProjetoDto do projeto criado.</returns>
    Task<ProjetoDto> CriarAsync(CriarProjetoDto projetoDto);

    /// <summary>
    /// Atualiza um projeto existente.
    /// </summary>
    /// <param name="id">O ID do projeto a ser atualizado (Guid).</param>
    /// <param name="projetoDto">O DTO contendo os dados atualizados do projeto.</param>
    /// <returns>O objeto ProjetoDto do projeto atualizado.</returns>
    Task<ProjetoDto> AtualizarAsync(Guid id, AtualizarProjetoDto projetoDto);

    /// <summary>
    /// Exclui um projeto logicamente (soft delete).
    /// </summary>
    /// <param name="id">O ID do projeto a ser excluído (Guid).</param>
    /// <returns>True se o projeto foi excluído com sucesso, False caso contrário.</returns>
    Task<bool> ExcluirAsync(Guid id);
}
