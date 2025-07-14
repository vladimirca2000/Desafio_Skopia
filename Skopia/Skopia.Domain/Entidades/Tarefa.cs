using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Entidades;

public class Tarefa : EntidadeBase
{
    public Guid ProjetoId { get; private set; }
    public Guid UsuarioId { get; private set; } 
    public string Titulo { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public StatusTarefa Status { get; private set; }
    public PrioridadeTarefa Prioridade { get; private set; }
    public DateTime? DataVencimento { get; private set; }
    public DateTime? DataConclusao { get; private set; }

    private readonly List<ComentarioTarefa> _comentarios;
    public IReadOnlyCollection<ComentarioTarefa> Comentarios => _comentarios.AsReadOnly();

    private readonly List<HistoricoAlteracaoTarefa> _historico;
    public IReadOnlyCollection<HistoricoAlteracaoTarefa> Historico => _historico.AsReadOnly();

    public Projeto? Projeto { get; private set; } 

    
    protected Tarefa() : base() 
    {
        _comentarios = new List<ComentarioTarefa>();
        _historico = new List<HistoricoAlteracaoTarefa>();
    }

    
    public Tarefa(Guid projetoId, Guid usuarioId, string titulo, string? descricao, PrioridadeTarefa prioridade, DateTime? dataVencimento) : base() 
    {
        _comentarios = new List<ComentarioTarefa>();
        _historico = new List<HistoricoAlteracaoTarefa>();

        ExcecaoDominio.Quando(projetoId == Guid.Empty, "O ID do projeto não pode ser vazio.");
        ExcecaoDominio.Quando(usuarioId == Guid.Empty, "O ID do usuário não pode ser vazio.");
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(titulo), "O título da tarefa não pode ser vazio.");
        ExcecaoDominio.Quando(dataVencimento.HasValue && dataVencimento.Value.Date < DateTime.UtcNow.Date, "A data de vencimento não pode ser no passado.");

        ProjetoId = projetoId;
        UsuarioId = usuarioId;
        Titulo = titulo;
        Descricao = descricao;
        DataCriacao = DateTime.UtcNow;
        Status = StatusTarefa.Pendente; 
        Prioridade = prioridade;
        DataVencimento = dataVencimento;
        DataConclusao = null;

        
        RegistrarHistorico("Status", null, Status.ToString(), usuarioId);
        RegistrarHistorico("Prioridade", null, Prioridade.ToString(), usuarioId); 
        RegistrarHistorico("DataVencimento", null, DataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), usuarioId);
        RegistrarHistorico("DataConclusao", null, DataConclusao?.ToString("yyyy-MM-dd HH:mm:ss"), usuarioId);
    }

    
    public void AtualizarTitulo(string novoTitulo, Guid usuarioExecutorId)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoTitulo), "O título da tarefa não pode ser vazio.");
        if (Titulo != novoTitulo)
        {
            RegistrarHistorico("Título", Titulo, novoTitulo, usuarioExecutorId); 
            Titulo = novoTitulo;
        }
    }

    public void AtualizarDescricao(string? novaDescricao, Guid usuarioExecutorId) 
    {
        if (Descricao != novaDescricao)
        {
            RegistrarHistorico("Descrição", Descricao, novaDescricao, usuarioExecutorId);
            Descricao = novaDescricao;
        }
    }

    public void AlterarStatus(StatusTarefa novoStatus, Guid usuarioExecutorId) 
    {
       
        if (Status == StatusTarefa.Concluida && novoStatus != StatusTarefa.Concluida && novoStatus != StatusTarefa.Cancelada)
        {
            ExcecaoDominio.Quando(true, "Tarefa concluída não pode ter seu status alterado para um estado ativo.");
        }
        if (Status == StatusTarefa.Cancelada && novoStatus != StatusTarefa.Cancelada && novoStatus != StatusTarefa.Concluida)
        {
            ExcecaoDominio.Quando(true, "Tarefa cancelada não pode ter seu status alterado para um estado ativo.");
        }

        if (Status != novoStatus)
        {
            RegistrarHistorico("Status", Status.ToString(), novoStatus.ToString(), usuarioExecutorId); 
            Status = novoStatus;

            if (Status == StatusTarefa.Concluida)
            {
                DataConclusao = DateTime.UtcNow; 
            }
            else if (DataConclusao.HasValue) 
            {
                DataConclusao = null;
            }
        }
    }

    
    public void AlterarPrioridade(PrioridadeTarefa novaPrioridade)
    {
        
        if (Prioridade != novaPrioridade)
        {
            Prioridade = novaPrioridade;
        }
    }

    public void AtualizarDataVencimento(DateTime? novaDataVencimento, Guid usuarioExecutorId) 
    {
        ExcecaoDominio.Quando(novaDataVencimento.HasValue && novaDataVencimento.Value.Date < DateTime.UtcNow.Date, "A data de vencimento não pode ser no passado.");

        if (DataVencimento != novaDataVencimento)
        {
            RegistrarHistorico("DataVencimento", DataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), novaDataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), usuarioExecutorId); 
            DataVencimento = novaDataVencimento;
        }
    }

    public void AtualizarDataConclusao(DateTime? novaDataConclusao, Guid usuarioExecutorId) 
    {
        ExcecaoDominio.Quando(novaDataConclusao.HasValue && novaDataConclusao.Value.Date < DateTime.UtcNow.Date, "A data de conclusão não pode ser no passado.");

        if (DataConclusao != novaDataConclusao)
        {
            RegistrarHistorico("DataConclusao", DataConclusao?.ToString("yyyy-MM-dd HH:mm:ss"), novaDataConclusao?.ToString("yyyy-MM-dd HH:mm:ss"), usuarioExecutorId); 
            DataConclusao = novaDataConclusao;
        }
    }

    public void AdicionarComentario(ComentarioTarefa comentario)
    {
        ExcecaoDominio.Quando(comentario == null, "O comentário não pode ser nulo.");
        
        ExcecaoDominio.Quando(comentario.TarefaId != Id, "O comentário não pertence a esta tarefa.");
        _comentarios.Add(comentario);
    }

    
    private void RegistrarHistorico(string campo, string? valorAntigo, string? valorNovo, Guid usuarioExecutorId)
    {
        var historico = new HistoricoAlteracaoTarefa(this.Id, campo, valorAntigo, valorNovo, usuarioExecutorId);
        _historico.Add(historico);
    }

    
    public bool EstaPendente()
    {
        return Status == StatusTarefa.Pendente || Status == StatusTarefa.EmAndamento;
    }

    
    public bool EstaAtrasada()
    {
        return DataVencimento.HasValue &&
               DataVencimento.Value.Date < DateTime.UtcNow.Date && 
               Status != StatusTarefa.Concluida && 
               Status != StatusTarefa.Cancelada;   
    }
}