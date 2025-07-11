using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skopia.Domain.Enums;

// <summary>
/// Define os possíveis status que uma tarefa pode ter em seu ciclo de vida.
/// A utilização de um enum garante que a tarefa sempre terá um estado válido e bem definido,
/// evitando erros de digitação ou atribuições de valores inválidos.
/// </summary>
public enum StatusTarefa
{
    /// <summary>
    /// A tarefa ainda não foi iniciada. É o estado inicial de uma nova tarefa.
    /// </summary>
    Pendente = 0,

    /// <summary>
    /// A tarefa está atualmente em execução. Indica que o trabalho na tarefa foi iniciado.
    /// </summary>
    EmAndamento = 1,

    /// <summary>
    /// A tarefa foi finalizada com sucesso. Este é geralmente um estado terminal.
    /// </summary>
    Concluida = 2,
    /// <summary>
    /// A tarefa foi cancelada. Este é um estado terminal que indica que a tarefa não será concluída.
    /// </summary>
    Cancelada = 3

}
