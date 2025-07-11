using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skopia.Domain.Entidades
{
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
            Status = StatusTarefa.Pendente;
            Prioridade = prioridade;
            DataVencimento = dataVencimento;

            // Registro inicial do histórico para os valores definidos na criação
            RegistrarHistorico("Status", null, Status.ToString());
            RegistrarHistorico("Prioridade", null, Prioridade.ToString());
            RegistrarHistorico("DataVencimento", null, DataVencimento?.ToString());
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
            if (Status == StatusTarefa.Concluida && novoStatus != StatusTarefa.Concluida)
            {
                ExcecaoDominio.Quando(true, "Tarefa concluída não pode ter seu status alterado para não-concluída.");
            }
            if (Status == StatusTarefa.Cancelada && novoStatus != StatusTarefa.Cancelada)
            {
                ExcecaoDominio.Quando(true, "Tarefa cancelada não pode ter seu status alterado para não-cancelada.");
            }

            if (Status != novoStatus)
            {
                RegistrarHistorico("Status", Status.ToString(), novoStatus.ToString());
                Status = novoStatus;
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
                RegistrarHistorico("DataVencimento", DataVencimento?.ToString(), novaDataVencimento?.ToString());
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
            var historico = new HistoricoAlteracaoTarefa(this.Id, campo, valorAntigo, valorNovo, this.UsuarioId);
            _historico.Add(historico);
        }

        public bool EstaPendente()
        {
            return Status == StatusTarefa.Pendente || Status == StatusTarefa.EmAndamento;
        }
    }
}