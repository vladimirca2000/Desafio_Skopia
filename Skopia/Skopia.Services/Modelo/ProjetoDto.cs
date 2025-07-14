using System.ComponentModel.DataAnnotations; 

namespace Skopia.Services.Modelos;

/// <summary>
/// DTO para representar um projeto.
/// </summary>
public class ProjetoDto
{
    public Guid Id { get; set; } 
    public string Nome { get; set; } = string.Empty; 
    public string? Descricao { get; set; } 
    public DateTime DataCriacao { get; set; } 
    public Guid UsuarioId { get; set; } 
    public int ContagemTarefas { get; set; } 
}

/// <summary>
/// DTO para criação de um novo projeto.
/// </summary>
public class CriarProjetoDto
{
    [Required(ErrorMessage = "O nome do projeto é obrigatório")] 
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres")] 
    public string Nome { get; set; } = string.Empty; 

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] 
    public string? Descricao { get; set; } 

    [Required(ErrorMessage = "O ID do usuário é obrigatório")] 
    public Guid UsuarioId { get; set; } 
}

/// <summary>
/// DTO para atualização de um projeto existente.
/// </summary>
public class AtualizarProjetoDto
{
    [Required(ErrorMessage = "O nome do projeto é obrigatório")] 
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres")] 
    public string Nome { get; set; } = string.Empty; 

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] 
    public string? Descricao { get; set; } 
}