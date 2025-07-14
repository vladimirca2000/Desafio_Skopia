using System.ComponentModel.DataAnnotations; 

namespace Skopia.Services.Modelos;

/// <summary>
/// DTO para representar um comentário de tarefa.
/// </summary>
public class ComentarioTarefaDto
{
    public Guid Id { get; set; } 
    public string Conteudo { get; set; } = string.Empty; 
    public DateTime DataCriacao { get; set; } 
    public Guid TarefaId { get; set; } 
    public Guid UsuarioId { get; set; } 
}

/// <summary>
/// DTO para criação de um novo comentário de tarefa.
/// </summary>
public class CriarComentarioTarefaDto
{
    [Required(ErrorMessage = "O conteúdo do comentário é obrigatório")] 
    [StringLength(1000, ErrorMessage = "O comentário não pode exceder 1000 caracteres")] 
    public string Conteudo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ID da tarefa é obrigatório")] 
    public Guid TarefaId { get; set; } 

    [Required(ErrorMessage = "O ID do usuário é obrigatório")] 
    public Guid UsuarioId { get; set; } 
}

/// <summary>
/// DTO para representar uma entrada de histórico de tarefa.
/// </summary>
public class HistoricoTarefaDto
{
    public Guid Id { get; set; } 
    public string CampoModificado { get; set; } = string.Empty; 
    public string? ValorAntigo { get; set; } 
    public string? ValorNovo { get; set; } 
    public DateTime DataAlteracao { get; set; } 
    public Guid TarefaId { get; set; } 
    public Guid UsuarioId { get; set; } 
}

/// <summary>
/// DTO para representar um relatório de desempenho do usuário.
/// </summary>
public class RelatorioDesempenhoDto
{
    public Guid UsuarioId { get; set; } 
    public string NomeUsuario { get; set; } = string.Empty; 
    public int ContagemTarefasConcluidas { get; set; } 
    public double MediaTarefasConcluidasPorDia { get; set; } 
}