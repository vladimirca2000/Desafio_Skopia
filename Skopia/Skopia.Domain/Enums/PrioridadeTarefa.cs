namespace Skopia.Domain.Enums;

/// <summary>
/// Define os níveis de prioridade que podem ser atribuídos a uma tarefa.
/// A prioridade é um atributo crucial para a gestão e ordenação de tarefas.
/// </summary>
public enum PrioridadeTarefa
{
    /// <summary>
    /// Tarefa de baixa importância. Geralmente pode ser adiada ou tem menos impacto.
    /// </summary>
    Baixa = 0,

    /// <summary>
    /// Tarefa de importância média. Deve ser tratada em um prazo razoável.
    /// </summary>
    Media = 1,

    /// <summary>
    /// Tarefa de alta importância. Requer atenção imediata e geralmente bloqueia outras atividades.
    /// </summary>
    Alta = 2
}
