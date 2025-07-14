using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Entidades;

public class ComentarioTarefa : EntidadeBase
{
    public Guid TarefaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Conteudo { get; private set; }
    public DateTime DataComentario { get; private set; }

    public Tarefa? Tarefa { get; private set; } 
    protected ComentarioTarefa() : base() { } 

    
    public ComentarioTarefa(Guid tarefaId, Guid usuarioId, string conteudo) : base()
    {
        ExcecaoDominio.Quando(tarefaId == Guid.Empty, "O ID da tarefa não pode ser vazio.");
        ExcecaoDominio.Quando(usuarioId == Guid.Empty, "O ID do usuário não pode ser vazio.");
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(conteudo), "O conteúdo do comentário não pode ser vazio.");

        TarefaId = tarefaId;
        UsuarioId = usuarioId;
        Conteudo = conteudo;
        DataComentario = DateTime.UtcNow;
    }

    
    public void AtualizarConteudo(string novoConteudo)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoConteudo), "O conteúdo do comentário não pode ser vazio.");
        if (Conteudo != novoConteudo)
        {
            Conteudo = novoConteudo;
        }
    }
}