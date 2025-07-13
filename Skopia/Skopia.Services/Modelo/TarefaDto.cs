using System.ComponentModel.DataAnnotations; // Necessário para [Required], [StringLength], [RegularExpression]

namespace Skopia.Servicos.Modelos;

/// <summary>
/// DTO para representar uma tarefa.
/// </summary>
public class TarefaDto
{
    public Guid Id { get; set; } // Ajustado para Guid
    public string Titulo { get; set; } = string.Empty; // Traduzido
    public string? Descricao { get; set; } // Traduzido
    public DateTime DataCriacao { get; set; } // Traduzido
    public DateTime? DataVencimento { get; set; } // Traduzido
    public string Status { get; set; } = string.Empty; // Traduzido (valores em PT-BR)
    public string Prioridade { get; set; } = string.Empty; // Traduzido (valores em PT-BR)
    public Guid ProjetoId { get; set; } // Ajustado para Guid, Traduzido
    public Guid UsuarioId { get; set; } // Ajustado para Guid, Traduzido
    public DateTime? DataConclusao { get; set; } // Adicionado para refletir a entidade de domínio

    // Referências aos DTOs de Comentário e Histórico, que estarão em outro arquivo.
    public List<ComentarioTarefaDto> Comentarios { get; set; } = new List<ComentarioTarefaDto>(); // Traduzido
    public List<HistoricoTarefaDto> Historico { get; set; } = new List<HistoricoTarefaDto>(); // Traduzido
}

/// <summary>
/// DTO para criação de uma nova tarefa.
/// </summary>
public class CriarTarefaDto
{
    [Required(ErrorMessage = "O título da tarefa é obrigatório")] // Traduzido
    [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres")] // Traduzido
    public string Titulo { get; set; } = string.Empty; // Traduzido

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] // Traduzido
    public string? Descricao { get; set; } // Traduzido

    public DateTime? DataVencimento { get; set; } // Traduzido

    [Required(ErrorMessage = "A prioridade é obrigatória")] // Traduzido
    [RegularExpression("^(Baixa|Media|Alta)$", ErrorMessage = "A prioridade deve ser Baixa, Media ou Alta")] // Valores e mensagem traduzidos
    public string Prioridade { get; set; } = "Media"; // Valor padrão traduzido

    [Required(ErrorMessage = "O ID do projeto é obrigatório")] // Traduzido
    public Guid ProjetoId { get; set; } // Ajustado para Guid

    [Required(ErrorMessage = "O ID do usuário é obrigatório")] // Traduzido
    public Guid UsuarioId { get; set; } // O usuário criador/responsável pela tarefa
}

/// <summary>
/// DTO para atualização de uma tarefa existente.
/// </summary>
public class AtualizarTarefaDto
{
    [Required(ErrorMessage = "O título da tarefa é obrigatório")] // Traduzido
    [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres")] // Traduzido
    public string Titulo { get; set; } = string.Empty; // Traduzido

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] // Traduzido
    public string? Descricao { get; set; } // Traduzido

    public DateTime? DataVencimento { get; set; } // Traduzido

    [Required(ErrorMessage = "O status é obrigatório")] // Traduzido
    [RegularExpression("^(Pendente|EmAndamento|Concluida|Cancelada)$", ErrorMessage = "O status deve ser Pendente, EmAndamento, Concluida ou Cancelada")] // Valores e mensagem traduzidos
    public string Status { get; set; } = string.Empty; // Traduzido

    [Required(ErrorMessage = "O ID do usuário executor é obrigatório")] // Traduzido
    public Guid UsuarioExecutorId { get; set; } // Traduzido para indicar o usuário que está fazendo a alteração
}