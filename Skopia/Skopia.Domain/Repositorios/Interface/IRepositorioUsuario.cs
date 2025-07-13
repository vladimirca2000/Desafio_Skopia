using Skopia.Domain.Entidades; // Importa a entidade de domínio 'Usuario'

namespace Skopia.Domain.Repositorios.Interfaces;

/// <summary>
/// Define o contrato para a recuperação de entidades 'Usuario'.
/// Esta interface representa uma "Porta" de saída da camada de domínio,
/// um contrato que a camada de aplicação utilizará para solicitar dados de usuário.
/// Ela permite que a lógica de negócio principal opere com informações de usuário
/// sem se preocupar com os detalhes de como esses dados são obtidos (e.g., de um banco de dados relacional,
/// de um serviço de identidade externo como Auth0 ou Azure AD B2C, ou mesmo de um cache).
/// Essa abstração é vital para a testabilidade e a flexibilidade da arquitetura.
/// </summary>
public interface IRepositorioUsuario
{
    /// <summary>
    /// Obtém um usuário pelo seu ID de forma assíncrona.
    /// Este é um método fundamental para a recuperação de uma única entidade 'Usuario' por seu identificador único.
    /// Idealmente, busca a entidade completa que representa o agregado 'Usuario' para que a lógica de negócio
    /// possa operar sobre ela.
    /// </summary>
    /// <param name="id">O ID (Guid) do usuário a ser recuperado.</param>
    /// <returns>
    /// O objeto 'Usuario' se encontrado, ou `null` se nenhum usuário com o ID especificado for localizado.
    /// O retorno `null` é um padrão comum para indicar a ausência de um recurso em operações de consulta,
    /// permitindo que a camada de aplicação (quem chama este método) decida como lidar com a ausência
    /// (e.g., retornar um HTTP 404 Not Found ou lançar uma exceção específica para o caso).
    /// </returns>
    Task<Usuario?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Verifica de forma assíncrona se um usuário possui a função de "Gerente".
    /// Este método é uma otimização para consultas específicas de papel do usuário,
    /// permitindo que a camada de dados execute a verificação de forma eficiente
    /// sem precisar carregar a entidade 'Usuario' completa para a memória do domínio.
    /// Por exemplo, a implementação na camada de dados pode traduzir isso para uma consulta SQL
    /// como `SELECT EXISTS (SELECT 1 FROM Users WHERE Id = @usuarioId AND Role = 'Gerente')`,
    /// que é muito mais performática do que carregar o objeto inteiro e depois verificar a propriedade.
    /// A regra de negócio sobre o que significa "ser gerente" (`FuncaoUsuario.Gerente`) ainda reside
    /// na entidade 'Usuario' ou em um serviço de domínio, mas a *verificação de dados* para suportar
    /// essa regra é otimizada via repositório.
    /// </summary>
    /// <param name="usuarioId">O ID (Guid) do usuário cuja função será verificada.</param>
    /// <returns>
    /// True se o usuário com o ID fornecido tiver a função de "Gerente", False caso contrário.
    /// </returns>
    Task<bool> VerificarSeGerenteAsync(Guid usuarioId);
}