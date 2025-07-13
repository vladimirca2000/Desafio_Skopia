using System.ComponentModel.DataAnnotations; 

namespace Skopia.Services.Modelos;

/// <summary>
/// DTO para representar um projeto.
/// </summary>
public class ProjetoDto
{
    public Guid Id { get; set; } // Ajustado para Guid
    public string Nome { get; set; } = string.Empty; // Traduzido
    public string? Descricao { get; set; } // Traduzido
    public DateTime DataCriacao { get; set; } // Traduzido
    public Guid UsuarioId { get; set; } // Ajustado para Guid, Traduzido
    public int ContagemTarefas { get; set; } // Traduzido
}

/// <summary>
/// DTO para criação de um novo projeto.
/// </summary>
public class CriarProjetoDto
{
    [Required(ErrorMessage = "O nome do projeto é obrigatório")] // Traduzido
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres")] // Traduzido
    public string Nome { get; set; } = string.Empty; // Traduzido

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] // Traduzido
    public string? Descricao { get; set; } // Traduzido

    [Required(ErrorMessage = "O ID do usuário é obrigatório")] // Traduzido
    public Guid UsuarioId { get; set; } // Ajustado para Guid
}

/// <summary>
/// DTO para atualização de um projeto existente.
/// </summary>
public class AtualizarProjetoDto
{
    [Required(ErrorMessage = "O nome do projeto é obrigatório")] // Traduzido
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres")] // Traduzido
    public string Nome { get; set; } = string.Empty; // Traduzido

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres")] // Traduzido
    public string? Descricao { get; set; } // Traduzido
}