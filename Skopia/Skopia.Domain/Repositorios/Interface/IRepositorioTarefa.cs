using Skopia.Domain.Entidades; // Importa as entidades de domínio 'Tarefa' e 'HistoricoAlteracaoTarefa'

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
    /// Em sistemas maiores e com grande volume de dados, este método pode ser estendido para incluir
    /// mecanismos de paginação (e.g., skip, take) e filtros adicionais para otimizar a carga de dados.
    /// </returns>
    Task<IEnumerable<Tarefa>> ObterTodosPorProjetoIdAsync(Guid projetoId);

    /// <summary>
    /// Obtém uma tarefa pelo seu ID de forma assíncrona.
    /// Este é um método fundamental para a recuperação de uma única entidade por seu identificador único.
    /// </summary>
    /// <param name="id">O ID (Guid) da tarefa a ser recuperada.</param>
    /// <returns>
    /// A entidade 'Tarefa' se encontrada. Retorna `null` se a tarefa com o ID especificado não existir,
    /// o que é um padrão comum para indicar a ausência de um recurso e permitir que a camada de aplicação
    /// trate essa situação (e.g., retornando um HTTP 404 Not Found).
    /// </returns>
    Task<Tarefa?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Adiciona uma nova tarefa ao armazenamento de forma assíncrona.
    /// A entidade 'Tarefa' deve ser criada e validada no domínio antes de ser persistida.
    /// </summary>
    /// <param name="tarefa">A entidade 'Tarefa' a ser criada. Espera-se que esta entidade já possua
    /// um 'Guid' gerado no domínio para seu ID, seguindo a prática de "ID generation by domain",
    /// o que simplifica a lógica de persistência e permite que o domínio controle a identidade da entidade.</param>
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
    /// É crucial que a entidade passada represente o estado completo e final da tarefa,
    /// pois o Entity Framework Core rastreará as mudanças a partir deste objeto.</param>
    /// <returns>
    /// A entidade 'Tarefa' atualizada, refletindo o estado persistido no armazenamento.
    /// </returns>
    Task<Tarefa> AtualizarAsync(Tarefa tarefa);

    /// <summary>
    /// Exclui uma tarefa do armazenamento de forma assíncrona.
    /// Esta operação deve ser tratada com cuidado. Em um sistema que utiliza soft delete,
    /// esta operação não remove fisicamente o registro, mas o marca como inativo.
    /// </summary>
    /// <param name="id">O ID (Guid) da tarefa a ser excluída logicamente.</param>
    /// <returns>
    /// True se a tarefa foi encontrada e marcada para exclusão lógica com sucesso; False se a tarefa
    /// com o ID especificado não foi encontrada ou se a operação falhou por outro motivo.
    /// O retorno booleano permite que a camada de aplicação reaja adequadamente (e.g., informando ao usuário).
    /// </returns>
    Task<bool> ExcluirAsync(Guid id);

    /// <summary>
    /// Obtém a contagem de tarefas concluídas por um usuário específico a partir de uma data de início, de forma assíncrona.
    /// Este método é um exemplo de uma consulta otimizada e específica para uma regra de negócio,
    /// visando a geração de relatórios de desempenho, métricas de produtividade individual ou painéis de controle (dashboards).
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
    /// (se a lógica de domínio for encapsulada nela) ou por um serviço de domínio quando uma modificação relevante ocorre,
    /// garantindo que o histórico de eventos seja persistido de forma consistente e granular.
    /// </summary>
    /// <param name="historico">A entidade 'HistoricoAlteracaoTarefa' a ser persistida,
    /// contendo os detalhes da alteração (quem, o quê, quando, quais campos foram alterados).</param>
    /// <returns>
    /// Uma 'Task' que representa a operação assíncrona.
    /// </returns>
    Task AdicionarHistoricoAsync(HistoricoAlteracaoTarefa historico);

    // --- Novas Sugestões para Gerenciamento de Prazos e Visibilidade ---

    /// <summary>
    /// Obtém de forma assíncrona todas as tarefas que estão atrasadas.
    /// Uma tarefa é considerada atrasada se sua 'DataVencimento' já passou e seu status
    /// não é 'Concluida' ou 'Cancelada'. Esta consulta é vital para painéis de controle de gestão
    /// e para identificar proativamente gargalos ou itens que requerem atenção imediata.
    /// O filtro global de soft delete será aplicado automaticamente, garantindo que apenas
    /// tarefas ativas (não deletadas logicamente) sejam consideradas.
    /// </summary>
    /// <returns>
    /// Uma coleção de entidades 'Tarefa' que estão atrasadas, ordenadas pela data de vencimento
    /// (as mais antigas primeiro). Retorna uma coleção vazia se nenhuma tarefa atrasada for encontrada.
    /// </returns>
    Task<IEnumerable<Tarefa>> ObterTarefasAtrasadasAsync();

    /// <summary>
    /// Obtém de forma assíncrona todas as tarefas com vencimento próximo em um período especificado.
    /// Considera tarefas cujo 'DataVencimento' está entre a data atual e a data atual + 'periodo',
    /// e que não estão 'Concluida' ou 'Cancelada'. Este método é útil para alertas proativos,
    /// ajudando equipes a se prepararem para prazos futuros e a evitar atrasos.
    /// O filtro global de soft delete será aplicado automaticamente.
    /// </summary>
    /// <param name="periodo">O período de tempo (ex: TimeSpan.FromDays(7) para 7 dias, TimeSpan.FromHours(24) para 24 horas)
    /// que define "vencimento próximo" a partir do momento da consulta.</param>
    /// <returns>
    /// Uma coleção de entidades 'Tarefa' com vencimento próximo, ordenadas pela data de vencimento
    /// (as mais próximas primeiro). Retorna uma coleção vazia se nenhuma tarefa for encontrada no período especificado.
    /// </returns>
    Task<IEnumerable<Tarefa>> ObterTarefasComVencimentoProximoAsync(TimeSpan periodo);

    /// <summary>
    /// Obtém de forma assíncrona todas as tarefas com 'DataVencimento' dentro de um intervalo de datas.
    /// Este método é útil para cenários de planejamento de sprints, relatórios de carga de trabalho para um
    /// período específico (e.g., próximo mês, próximo trimestre) ou para visualizações de calendário.
    /// O filtro global de soft delete será aplicado automaticamente.
    /// </summary>
    /// <param name="dataInicioVencimento">A data de início do intervalo (inclusive) para a DataVencimento.
    /// A porção de hora será ignorada para esta comparação, focando apenas na data.</param>
    /// <param name="dataFimVencimento">A data de fim do intervalo (inclusive) para a DataVencimento.
    /// A porção de hora será ignorada para esta comparação, focando apenas na data.</param>
    /// <returns>
    /// Uma coleção de entidades 'Tarefa' cujo vencimento está no período especificado, ordenadas pela data de vencimento.
    /// Retorna uma coleção vazia se nenhuma tarefa for encontrada no intervalo.
    /// </returns>
    Task<IEnumerable<Tarefa>> ObterTarefasPorPeriodoDeVencimentoAsync(DateTime dataInicioVencimento, DateTime dataFimVencimento);


    /// <summary>
    /// Verifica de forma assíncrona se um projeto possui tarefas que ainda não foram concluídas ou canceladas.
    /// Esta é uma consulta otimizada para verificar a existência de tarefas "ativas" para um dado projeto.
    /// O filtro global de soft delete será aplicado automaticamente.
    /// </summary>
    /// <param name="projetoId">O ID do projeto a ser verificado.</param>
    /// <returns>True se houver tarefas pendentes ou em andamento para o projeto; caso contrário, false.</returns>
    Task<bool> PossuiTarefasPendentesParaProjetoAsync(Guid projetoId);
}