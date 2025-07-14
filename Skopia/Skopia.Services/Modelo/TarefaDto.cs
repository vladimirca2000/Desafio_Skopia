using System.ComponentModel.DataAnnotations;
namespace Skopia.Services.Modelos;

/// <summary>
/// DTO para representar uma tarefa.
/// </summary>
public class TarefaDto
{
    public Guid Id { get; set; } 
    public string Titulo { get; set; } = string.Empty; 
    public string? Descricao { get; set; } 
    public DateTime DataCriacao { get; set; } 
    public DateTime? DataVencimento { get; set; } 
    public string Status { get; set; } = string.Empty; 
    public string Prioridade { get; set; } = string.Empty; 
    public Guid ProjetoId { get; set; } 
    public Guid UsuarioId { get; set; } 
    public DateTime? DataConclusao { get; set; } 

    
    public List<ComentarioTarefaDto> Comentarios { get; set; } = new List<ComentarioTarefaDto>(); 
    public List<HistoricoTarefaDto> Historico { get; set; } = new List<HistoricoTarefaDto>(); 
}

/// <summary>
/// DTO para criação de uma nova tarefa.
/// </summary>
public class CriarTarefaDto
{
    [Required(ErrorMessage = "O título da tarefa é obrigatório")] 
    [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres")] 
    public string Titulo { get; set; } = string.Empty; 

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] 
    public string? Descricao { get; set; } 

    public DateTime? DataVencimento { get; set; } 

    [Required(ErrorMessage = "A prioridade é obrigatória")] 
    [RegularExpression("^(Baixa|Media|Alta)$", ErrorMessage = "A prioridade deve ser Baixa, Media ou Alta")] 
    public string Prioridade { get; set; } = "Media"; 

    [Required(ErrorMessage = "O ID do projeto é obrigatório")] 
    public Guid ProjetoId { get; set; } 

    [Required(ErrorMessage = "O ID do usuário é obrigatório")] 
    public Guid UsuarioId { get; set; } 
}

/// <summary>
/// DTO para atualização de uma tarefa existente.
/// </summary>
public class AtualizarTarefaDto
{
    [Required(ErrorMessage = "O título da tarefa é obrigatório")] 
    [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres")] 
    public string Titulo { get; set; } = string.Empty; 

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] 
    public string? Descricao { get; set; } 

    public DateTime? DataConclusao { get; set; } 

    [Required(ErrorMessage = "O status é obrigatório")] 
    [RegularExpression("^(Pendente|EmAndamento|Concluida|Cancelada)$", ErrorMessage = "O status deve ser Pendente, EmAndamento, Concluida ou Cancelada")] 
    public string Status { get; set; } = string.Empty; 

    [Required(ErrorMessage = "O ID do usuário executor é obrigatório")] 
    public Guid UsuarioExecutorId { get; set; } 
}