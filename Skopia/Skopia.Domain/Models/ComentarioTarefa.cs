using Skopia.Domain.Excecoes;
using System;

namespace Skopia.Domain.Entidades;

public class ComentarioTarefa : EntidadeBase
{
    public Guid TarefaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Conteudo { get; private set; }
    public DateTime DataComentario { get; private set; }

    public Tarefa? Tarefa { get; private set; } // Propriedade de navegação

    // Construtor protegido para uso do Entity Framework Core.
    protected ComentarioTarefa() : base() { } // Chamada explícita para o construtor padrão da EntidadeBase

    // Construtor de domínio para criação de novos comentários.
    public ComentarioTarefa(Guid tarefaId, Guid usuarioId, string conteudo) : base() // <<-- Chamada explícita para o construtor padrão da EntidadeBase
    {
        ExcecaoDominio.Quando(tarefaId == Guid.Empty, "O ID da tarefa não pode ser vazio.");
        ExcecaoDominio.Quando(usuarioId == Guid.Empty, "O ID do usuário não pode ser vazio.");
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(conteudo), "O conteúdo do comentário não pode ser vazio.");

        TarefaId = tarefaId;
        UsuarioId = usuarioId;
        Conteudo = conteudo;
        DataComentario = DateTime.UtcNow;
    }

    // Método de comportamento do domínio
    public void AtualizarConteudo(string novoConteudo)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoConteudo), "O conteúdo do comentário não pode ser vazio.");
        if (Conteudo != novoConteudo)
        {
            Conteudo = novoConteudo;
        }
    }
}