namespace Skopia.Domain.Servicos.Interfaces;

/// <summary>
/// Define o contrato para serviços de domínio relacionados a projetos.
/// Estes serviços encapsulam regras de negócio que envolvem a interação entre
/// múltiplos agregados ou que dependem de infraestrutura (como repositórios).
/// </summary>
public interface IProjetoService
{
    /// <summary>
    /// Avalia de forma assíncrona se um projeto pode ser logicamente removido (soft delete),
    /// com base nas regras de negócio definidas (ex: não possuir tarefas pendentes).
    /// </summary>
    /// <param name="projetoId">O ID do projeto a ser avaliado.</param>
    /// <returns>True se o projeto puder ser removido; caso contrário, false.</returns>
    Task<bool> PodeRemoverProjetoAsync(Guid projetoId);
}
