﻿using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Entidades;

public class HistoricoAlteracaoTarefa : EntidadeBase
{
    public Guid TarefaId { get; private set; }
    public Guid UsuarioId { get; private set; } 
    public string CampoModificado { get; private set; }
    public string? ValorAntigo { get; private set; }
    public string? ValorNovo { get; private set; }
    public DateTime DataModificacao { get; private set; }

    public Tarefa? Tarefa { get; private set; } 

    
    protected HistoricoAlteracaoTarefa() : base() { } 

    public HistoricoAlteracaoTarefa(Guid tarefaId, string campoModificado, string? valorAntigo, string? valorNovo, Guid usuarioId) : base() 
    {
        ExcecaoDominio.Quando(tarefaId == Guid.Empty, "O ID da tarefa não pode ser vazio para o histórico.");
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(campoModificado), "O campo modificado não pode ser vazio.");
        ExcecaoDominio.Quando(usuarioId == Guid.Empty, "O ID do usuário não pode ser vazio para o histórico.");

        TarefaId = tarefaId;
        CampoModificado = campoModificado;
        ValorAntigo = valorAntigo;
        ValorNovo = valorNovo;
        DataModificacao = DateTime.UtcNow;
        UsuarioId = usuarioId; 
    }
}