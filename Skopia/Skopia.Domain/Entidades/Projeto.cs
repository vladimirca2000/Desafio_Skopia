using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Entidades;


public class Projeto : EntidadeBase 
{
    
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime DataCriacao { get; private set; } 
    public Guid UsuarioId { get; private set; }

    
    private readonly List<Tarefa> _tarefas;

    
    public IReadOnlyCollection<Tarefa> Tarefas => _tarefas.AsReadOnly();

    private const int LIMITE_MAXIMO_TAREFAS = 20;

    
    protected Projeto(Guid guid) : base()
    {
        _tarefas = new List<Tarefa>();
    }

   
    public Projeto(string nome, string? descricao, Guid usuarioId) : base()
    {
        _tarefas = new List<Tarefa>();

        
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(nome), "O nome do projeto não pode ser vazio.");
        ExcecaoDominio.Quando(usuarioId == Guid.Empty, "O ID do usuário não pode ser vazio.");

        
        Nome = nome;
        Descricao = descricao;
        DataCriacao = DateTime.UtcNow; 
        UsuarioId = usuarioId;
    }

    
    public void AtualizarNome(string novoNome)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoNome), "O nome do projeto não pode ser vazio.");
        if (Nome != novoNome) 
        {
            Nome = novoNome;
        }
    }

    
    public void AtualizarDescricao(string? novaDescricao)
    {
        if (Descricao != novaDescricao) // Evita atribuições desnecessárias.
        {
            Descricao = novaDescricao;
        }
    }

    
    public void Testar20Tarefa(Tarefa novaTarefa)
    {
        ExcecaoDominio.Quando(novaTarefa?.ProjetoId != Id, "A tarefa pertence a outro projeto e não pode ser adicionada aqui.");
        ExcecaoDominio.Quando(_tarefas.Count >= LIMITE_MAXIMO_TAREFAS,
            $"Limite máximo de {LIMITE_MAXIMO_TAREFAS} tarefas por projeto atingido. Não é possível adicionar mais tarefas.");
    }

    
    public void RemoverTarefa(Guid tarefaId)
    {
        var tarefaParaRemover = _tarefas.FirstOrDefault(t => t.Id == tarefaId);
        ExcecaoDominio.Quando(tarefaParaRemover == null, "Tarefa não encontrada neste projeto.");
    }

   
    public Tarefa? ObterTarefaPorId(Guid tarefaId)
    {
        return _tarefas.FirstOrDefault(t => t.Id == tarefaId);
    }
}