using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;
using System;
using System.Collections.Generic;
using System.Linq;

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
        // Validação da DataVencimento no construtor
        ExcecaoDominio.Quando(dataVencimento.HasValue && dataVencimento.Value.Date < DateTime.UtcNow.Date, "A data de vencimento não pode ser no passado.");
        // REMOVIDA: Validação de DataConclusao no construtor, pois ela é nula na criação.

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
        RegistrarHistorico("Status", null, Status.ToString());
        RegistrarHistorico("Prioridade", null, Prioridade.ToString());
        RegistrarHistorico("DataVencimento", null, DataVencimento?.ToString("yyyy-MM-dd HH:mm:ss")); // Formata para evitar problemas de cultura
    }

    // Métodos de comportamento do domínio
    public void AtualizarTitulo(string novoTitulo)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoTitulo), "O título da tarefa não pode ser vazio.");
        if (Titulo != novoTitulo)
        {
            RegistrarHistorico("Título", Titulo, novoTitulo);
            Titulo = novoTitulo;
        }
    }

    public void AtualizarDescricao(string? novaDescricao)
    {
        if (Descricao != novaDescricao)
        {
            RegistrarHistorico("Descrição", Descricao, novaDescricao);
            Descricao = novaDescricao;
        }
    }

    public void AlterarStatus(StatusTarefa novoStatus)
    {
        // Regras de negócio para transição de status
        if (Status == StatusTarefa.Concluida && novoStatus != StatusTarefa.Concluida && novoStatus != StatusTarefa.Cancelada)
        {
            // Se já está concluída, não pode voltar para um status ativo (a menos que seja explicitamente reaberta)
            // Ou ir para Cancelada (se essa for uma transição válida, mas geralmente não é)
            ExcecaoDominio.Quando(true, "Tarefa concluída não pode ter seu status alterado para um estado ativo.");
        }
        if (Status == StatusTarefa.Cancelada && novoStatus != StatusTarefa.Cancelada && novoStatus != StatusTarefa.Concluida)
        {
            // Se já está cancelada, não pode voltar para um status ativo
            ExcecaoDominio.Quando(true, "Tarefa cancelada não pode ter seu status alterado para um estado ativo.");
        }

        if (Status != novoStatus)
        {
            RegistrarHistorico("Status", Status.ToString(), novoStatus.ToString());
            Status = novoStatus;

            // Lógica para DataConclusao baseada na transição de status
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

    public void AlterarPrioridade(PrioridadeTarefa novaPrioridade)
    {
        if (Prioridade != novaPrioridade)
        {
            RegistrarHistorico("Prioridade", Prioridade.ToString(), novaPrioridade.ToString());
            Prioridade = novaPrioridade;
        }
    }

    public void AtualizarDataVencimento(DateTime? novaDataVencimento)
    {
        ExcecaoDominio.Quando(novaDataVencimento.HasValue && novaDataVencimento.Value.Date < DateTime.UtcNow.Date, "A data de vencimento não pode ser no passado.");

        if (DataVencimento != novaDataVencimento)
        {
            RegistrarHistorico("DataVencimento", DataVencimento?.ToString("yyyy-MM-dd HH:mm:ss"), novaDataVencimento?.ToString("yyyy-MM-dd HH:mm:ss")); // Formata para evitar problemas de cultura
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

    private void RegistrarHistorico(string campo, string? valorAntigo, string? valorNovo)
    {
        // Cria uma nova entrada de histórico para a alteração
        // Assumimos que o UsuarioId da Tarefa é quem fez a alteração.
        // Em um sistema real, o UsuarioId do contexto de segurança seria passado aqui.
        var historico = new HistoricoAlteracaoTarefa(this.Id, campo, valorAntigo, valorNovo, this.UsuarioId);
        _historico.Add(historico);
    }

    /// <summary>
    /// Verifica se a tarefa está em um estado "ativo" (Pendente ou Em Andamento).
    /// </summary>
    /// <returns>True se a tarefa está pendente ou em andamento; caso contrário, false.</returns>
    public bool EstaPendente()
    {
        // Esta lógica é intrínseca à tarefa.
        return Status == StatusTarefa.Pendente || Status == StatusTarefa.EmAndamento;
    }

    /// <summary>
    /// Verifica se a tarefa está atrasada com base na DataVencimento e Status.
    /// Esta lógica é intrínseca à tarefa.
    /// </summary>
    public bool EstaAtrasada()
    {
        return DataVencimento.HasValue &&
               DataVencimento.Value.Date < DateTime.UtcNow.Date && // Vencimento já passou
               Status != StatusTarefa.Concluida && // Não está concluída
               Status != StatusTarefa.Cancelada;    // Não está cancelada
    }
}