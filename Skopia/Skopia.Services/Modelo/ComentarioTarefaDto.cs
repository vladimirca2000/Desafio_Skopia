using System.ComponentModel.DataAnnotations; 

namespace Skopia.Services.Modelos;

/// <summary>
/// DTO para representar um comentário de tarefa.
/// </summary>
public class ComentarioTarefaDto
{
    public Guid Id { get; set; } // Ajustado para Guid
    public string Conteudo { get; set; } = string.Empty; // Traduzido
    public DateTime DataCriacao { get; set; } // Traduzido
    public Guid TarefaId { get; set; } // Ajustado para Guid, Traduzido
    public Guid UsuarioId { get; set; } // Ajustado para Guid, Traduzido
}

/// <summary>
/// DTO para criação de um novo comentário de tarefa.
/// </summary>
public class CriarComentarioTarefaDto
{
    [Required(ErrorMessage = "O conteúdo do comentário é obrigatório")] // Traduzido
    [StringLength(1000, ErrorMessage = "O comentário não pode exceder 1000 caracteres")] // Traduzido
    public string Conteudo { get; set; } = string.Empty; // Traduzido

    [Required(ErrorMessage = "O ID da tarefa é obrigatório")] // Traduzido
    public Guid TarefaId { get; set; } // Ajustado para Guid

    [Required(ErrorMessage = "O ID do usuário é obrigatório")] // Traduzido
    public Guid UsuarioId { get; set; } // Ajustado para Guid
}

/// <summary>
/// DTO para representar uma entrada de histórico de tarefa.
/// </summary>
public class HistoricoTarefaDto
{
    public Guid Id { get; set; } // Ajustado para Guid
    public string CampoModificado { get; set; } = string.Empty; // Traduzido
    public string? ValorAntigo { get; set; } // Traduzido
    public string? ValorNovo { get; set; } // Traduzido
    public DateTime DataAlteracao { get; set; } // Traduzido
    public Guid TarefaId { get; set; } // Ajustado para Guid, Traduzido
    public Guid UsuarioId { get; set; } // Ajustado para Guid, Traduzido
}

/// <summary>
/// DTO para representar um relatório de desempenho do usuário.
/// </summary>
public class RelatorioDesempenhoDto
{
    public Guid UsuarioId { get; set; } // Ajustado para Guid, Traduzido
    public string NomeUsuario { get; set; } = string.Empty; // Traduzido
    public int ContagemTarefasConcluidas { get; set; } // Traduzido
    public double MediaTarefasConcluidasPorDia { get; set; } // Traduzido
}