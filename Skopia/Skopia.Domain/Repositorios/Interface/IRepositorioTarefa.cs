using Skopia.Domain.Entidades; // Importa as entidades de domínio 'Tarefa' e 'HistoricoAlteracaoTarefa'
using System; // Necessário para Guid e DateTime
using System.Collections.Generic; // Necessário para IEnumerable
using System.Threading.Tasks; // Necessário para Task

namespace Skopia.Domain.Repositorios.Interfaces;

/// <summary>
/// Define o contrato para operações de persistência e recuperação da entidade 'Tarefa'.
/// Esta interface representa uma "Porta" de saída da camada de domínio, permitindo que a lógica de negócio
/// (que utiliza esta interface) seja independente dos detalhes de implementação do acesso a dados.
/// A implementação concreta desta interface residirá na camada de infraestrutura (Skopia.Data),
/// atuando como um adaptador para o mecanismo de persistência real (e.g., um banco de dados SQL, NoSQL, etc.).
/// </summary>
public interface IRepositorioTarefa
{
    /// <summary>
    /// Obtém todas as tarefas associadas a um ID de projeto específico de forma assíncrona.
    /// Este método permite recuperar um conjunto de tarefas que pertencem a um contexto específico de projeto,
    /// otimizando a recuperação de dados para cenários de visualização ou processamento em massa.
    /// </summary>
    /// <param name="projetoId">O ID (Guid) do projeto cujas tarefas serão recuperadas.</param>
    /// <returns>
    /// Uma coleção de entidades 'Tarefa'. Retorna uma coleção vazia (não null)
    /// se nenhuma tarefa for encontrada para o 'projetoId' fornecido, evitando NullReferenceExceptions.
    /// Em sistemas maiores, este método poderia ser estendido para incluir paginação ou filtros adicionais.
    /// </returns>
    Task<IEnumerable<Tarefa>> ObterTodosPorProjetoIdAsync(Guid projetoId);

    /// <summary>
    /// Obtém uma tarefa pelo seu ID de forma assíncrona.
    /// Este é um método fundamental para a recuperação de uma única entidade por seu identificador único.
    /// </summary>
    /// <param name="id">O ID (Guid) da tarefa a ser recuperada.</param>
    /// <returns>
    /// A entidade 'Tarefa' se encontrada. Retorna `null` se a tarefa com o ID especificado não existir,
    /// o que é um padrão comum para indicar a ausência de um recurso.
    /// </returns>
    Task<Tarefa?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Adiciona uma nova tarefa ao armazenamento de forma assíncrona.
    /// A entidade 'Tarefa' deve ser criada e validada no domínio antes de ser persistida.
    /// </summary>
    /// <param name="tarefa">A entidade 'Tarefa' a ser criada. Espera-se que esta entidade já possua
    /// um 'Guid' gerado no domínio para seu ID, seguindo a prática de "ID generation by domain".</param>
    /// <returns>
    /// A entidade 'Tarefa' criada, que pode incluir um ID gerado (se aplicável, embora usemos Guid gerado pelo domínio)
    /// ou outros valores padrão definidos durante a operação de persistência (e.g., timestamps de criação).
    /// </returns>
    Task<Tarefa> CriarAsync(Tarefa tarefa);

    /// <summary>
    /// Atualiza uma tarefa existente no armazenamento de forma assíncrona.
    /// O método é idempotente e espera que a entidade 'Tarefa' fornecida contenha o estado desejado após a atualização.
    /// </summary>
    /// <param name="tarefa">A entidade 'Tarefa' com os dados atualizados.
    /// O método deve usar o ID da entidade para localizar o registro a ser atualizado no armazenamento.
    /// É crucial que a entidade passada represente o estado completo e final da tarefa.</param>
    /// <returns>
    /// A entidade 'Tarefa' atualizada, refletindo o estado persistido no armazenamento.
    /// </returns>
    Task<Tarefa> AtualizarAsync(Tarefa tarefa);

    /// <summary>
    /// Exclui uma tarefa do armazenamento de forma assíncrona.
    /// Esta operação deve ser tratada com cuidado, pois a exclusão de dados é irreversível.
    /// </summary>
    /// <param name="id">O ID (Guid) da tarefa a ser excluída.</param>
    /// <returns>
    /// True se a tarefa foi excluída com sucesso; False se a tarefa com o ID especificado não foi encontrada
    /// para exclusão, ou se a operação falhou por outro motivo (e.g., restrições de integridade referencial).
    /// O retorno booleano permite que a camada de aplicação reaja adequadamente.
    /// </returns>
    Task<bool> ExcluirAsync(Guid id);

    /// <summary>
    /// Obtém a contagem de tarefas concluídas por um usuário específico a partir de uma data de início, de forma assíncrona.
    /// Este método é um exemplo de uma consulta otimizada e específica para uma regra de negócio,
    /// visando a geração de relatórios de desempenho ou métricas de produtividade.
    /// </summary>
    /// <param name="usuarioId">O ID (Guid) do usuário.</param>
    /// <param name="dataInicio">A data a partir da qual as tarefas concluídas devem ser contadas (inclusive).</param>
    /// <returns>
    /// O número total de tarefas concluídas pelo usuário desde a 'dataInicio'.
    /// </returns>
    Task<int> ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(Guid usuarioId, DateTime dataInicio);

    /// <summary>
    /// Adiciona um registro de histórico de alteração para uma tarefa de forma assíncrona.
    /// Este método é crucial para a trilha de auditoria e pode ser invocado pela própria entidade 'Tarefa'
    /// ou por um serviço de domínio quando uma modificação relevante ocorre,
    /// garantindo que o histórico de eventos seja persistido de forma consistente.
    /// </summary>
    /// <param name="historico">A entidade 'HistoricoAlteracaoTarefa' a ser persistida,
    /// contendo os detalhes da alteração (quem, o quê, quando).</param>
    /// <returns>
    /// Uma 'Task' que representa a operação assíncrona.
    /// </returns>
    Task AdicionarHistoricoAsync(HistoricoAlteracaoTarefa historico);
}
