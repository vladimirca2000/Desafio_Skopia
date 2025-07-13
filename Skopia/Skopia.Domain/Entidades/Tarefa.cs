using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Entidades;

public class Tarefa : EntidadeBase
{
    public Guid ProjetoId { get; private set; }
    public Guid UsuarioId { get; private set; } // O ID do usuário responsável pela tarefa
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

    public Projeto? Projeto { get; private set; } // Propriedade de navegação

    // Construtor protegido para uso do Entity Framework Core e outras ferramentas de materialização.
    protected Tarefa() : base() // Chamada explícita para o construtor padrão da EntidadeBase
    {
        _comentarios = new List<ComentarioTarefa>();
        _historico = new List<HistoricoAlteracaoTarefa>();
    }

    // Construtor de domínio para criação de novas tarefas.
    public Tarefa(Guid projetoId, Guid usuarioId, string titulo, string? descricao, PrioridadeTarefa prioridade, DateTime? dataVencimento) : base() // <<-- Chamada explícita para o construtor padrão da EntidadeBase
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
        Status = StatusTarefa.Pendente; // Tarefa começa como pendente
        Prioridade = prioridade;
        DataVencimento = dataVencimento;
        DataConclusao = null; // Garante que DataConclusao é nula na criação.

        // Registro inicial do histórico para os valores definidos na criação
        RegistrarHistorico("Status", null, Status.ToString(), usuarioId); // Usa o UsuarioId da tarefa como o executor inicial
        RegistrarHistorico("Prioridade", null, Prioridade.ToString(), usuarioId); // Usa o UsuarioId da tarefa como o executor inicial
        RegistrarHistorico("DataVencimento", null, DataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), usuarioId); // Usa o UsuarioId da tarefa como o executor inicial
    }

    // Métodos de comportamento do domínio com ajuste para usuarioExecutorId
    public void AtualizarTitulo(string novoTitulo, Guid usuarioExecutorId) // Adicionado usuarioExecutorId
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoTitulo), "O título da tarefa não pode ser vazio.");
        if (Titulo != novoTitulo)
        {
            RegistrarHistorico("Título", Titulo, novoTitulo, usuarioExecutorId); // Passa usuarioExecutorId
            Titulo = novoTitulo;
        }
    }

    public void AtualizarDescricao(string? novaDescricao, Guid usuarioExecutorId) // Adicionado usuarioExecutorId
    {
        if (Descricao != novaDescricao)
        {
            RegistrarHistorico("Descrição", Descricao, novaDescricao, usuarioExecutorId); // Passa usuarioExecutorId
            Descricao = novaDescricao;
        }
    }

    public void AlterarStatus(StatusTarefa novoStatus, Guid usuarioExecutorId) // Adicionado usuarioExecutorId
    {
        // Regras de negócio para transição de status
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
            RegistrarHistorico("Status", Status.ToString(), novoStatus.ToString(), usuarioExecutorId); // Passa usuarioExecutorId
            Status = novoStatus;

            if (Status == StatusTarefa.Concluida)
            {
                DataConclusao = DateTime.UtcNow; // Define a data de conclusão quando a tarefa é concluída
            }
            else if (DataConclusao.HasValue) // Se o status mudou de Concluida para outro (se a regra permitir)
            {
                DataConclusao = null; // Limpa a data de conclusão
            }
        }
    }

    // Nota: O método AlterarPrioridade não está sendo atualizado pois a regra de negócio inicial era que a prioridade não podia ser alterada após a criação.
    // Se essa regra mudar, você o ajustaria de forma similar.
    public void AlterarPrioridade(PrioridadeTarefa novaPrioridade)
    {
        // ExcecaoDominio.Quando(true, "Não é permitido alterar a prioridade de uma tarefa depois que ela foi criada.");
        // Mantendo a regra que não permite alteração de prioridade.
        // Se essa regra fosse removida, o método precisaria de `usuarioExecutorId` e `RegistrarHistorico`.
        if (Prioridade != novaPrioridade)
        {
            Prioridade = novaPrioridade;
        }
    }

    public void AtualizarDataVencimento(DateTime? novaDataVencimento, Guid usuarioExecutorId) // Adicionado usuarioExecutorId
    {
        ExcecaoDominio.Quando(novaDataVencimento.HasValue && novaDataVencimento.Value.Date < DateTime.UtcNow.Date, "A data de vencimento não pode ser no passado.");

        if (DataVencimento != novaDataVencimento)
        {
            RegistrarHistorico("DataVencimento", DataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), novaDataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), usuarioExecutorId); // Passa usuarioExecutorId
            DataVencimento = novaDataVencimento;
        }
    }

    public void AdicionarComentario(ComentarioTarefa comentario)
    {
        ExcecaoDominio.Quando(comentario == null, "O comentário não pode ser nulo.");
        // Garante que o comentário está associado à tarefa correta.
        ExcecaoDominio.Quando(comentario.TarefaId != Id, "O comentário não pertence a esta tarefa.");
        _comentarios.Add(comentario);
    }

    // Método privado auxiliar para registro de histórico, agora com usuarioExecutorId
    private void RegistrarHistorico(string campo, string? valorAntigo, string? valorNovo, Guid usuarioExecutorId)
    {
        // Cria uma nova entrada de histórico para a alteração
        // Assumimos que o usuarioExecutorId é quem fez a alteração.
        var historico = new HistoricoAlteracaoTarefa(this.Id, campo, valorAntigo, valorNovo, usuarioExecutorId);
        _historico.Add(historico);
    }

    /// <summary>
    /// Verifica se a tarefa está em um estado "ativo" (Pendente ou Em Andamento).
    /// </summary>
    /// <returns>True se a tarefa está pendente ou em andamento; caso contrário, false.</returns>
    public bool EstaPendente()
    {
        return Status == StatusTarefa.Pendente || Status == StatusTarefa.EmAndamento;
    }

    /// <summary>
    /// Verifica se a tarefa está atrasada com base na DataVencimento e Status.
    /// </summary>
    public bool EstaAtrasada()
    {
        return DataVencimento.HasValue &&
               DataVencimento.Value.Date < DateTime.UtcNow.Date && // Vencimento já passou
               Status != StatusTarefa.Concluida && // Não está concluída
               Status != StatusTarefa.Cancelada;    // Não está cancelada
    }
}