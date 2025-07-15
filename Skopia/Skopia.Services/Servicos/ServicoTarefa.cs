using AutoMapper;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades; 
using Skopia.Domain.Enums; 
using Skopia.Domain.Excecoes; 
using Skopia.Domain.Interfaces.UnitOfWork; 
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos; 

namespace Skopia.Services.Servicos;

public class ServicoTarefa : IServicoTarefa
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ServicoTarefa> _logger;

    /// <inheritdoc/>
    public ServicoTarefa(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ServicoTarefa> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TarefaDto>> ObterTodosPorProjetoIdAsync(Guid projetoId)
    {
        _logger.LogInformation("Obtendo todas as tarefas para o projeto ID: {ProjetoId}", projetoId);

        if (projetoId == Guid.Empty)
        {
            _logger.LogWarning("ID do projeto vazio ao tentar obter tarefas.");
            throw new ArgumentException("O ID do projeto não pode ser vazio.", nameof(projetoId));
        }

        var projeto = await _unitOfWork.Projetos.ObterPorIdAsync(projetoId);
        if (projeto == null)
        {
            _logger.LogWarning("Projeto com ID {ProjetoId} não encontrado ao tentar obter tarefas.", projetoId);
            throw new KeyNotFoundException($"Projeto com ID {projetoId} não encontrado.");
        }
        
        _logger.LogInformation("Projeto com ID {ProjetoId} encontrado. Obtendo tarefas associadas.", projetoId);
        return _mapper.Map<IEnumerable<TarefaDto>>(projeto.Tarefas);
    }

    /// <inheritdoc/>
    public async Task<TarefaDto?> ObterPorIdAsync(Guid id)
    {
        _logger.LogInformation("Obtendo tarefa com ID: {Id}", id);

        if (id == Guid.Empty) 
        {
            _logger.LogWarning("ID da tarefa vazio ao tentar obter tarefa.");
            throw new ArgumentException("O ID da tarefa não pode ser vazio.", nameof(id));
        }
        var tarefa = await _unitOfWork.Tarefas.ObterPorIdAsync(id);
        if (tarefa == null)
        {
            _logger.LogWarning("Tarefa com ID {Id} não encontrada.", id);
            return null;
        }

        _logger.LogInformation("Tarefa com ID {Id} encontrada.", id);
        return _mapper.Map<TarefaDto>(tarefa);
    }

    /// <inheritdoc/>
    public async Task<TarefaDto> CriarAsync(CriarTarefaDto criarTarefaDto)
    {
        _logger.LogInformation("Criando nova tarefa com dados: {@CriarTarefaDto}", criarTarefaDto);

        // 1. Verificar se o projeto existe
        var projeto = await _unitOfWork.Projetos.ObterPorIdAsync(criarTarefaDto.ProjetoId);
        if (projeto == null)
        {
            _logger.LogWarning("Projeto com ID {ProjetoId} não encontrado ao tentar criar tarefa.", criarTarefaDto.ProjetoId);
            throw new KeyNotFoundException($"Projeto com ID {criarTarefaDto.ProjetoId} não encontrado.");
        }

        // 2. Converter string de prioridade para o enum do domínio
        var prioridade = ParsePrioridade(criarTarefaDto.Prioridade);
        _logger.LogInformation("Prioridade convertida para enum: {Prioridade}", prioridade);

        // 3. Criar a entidade de domínio Tarefa (o construtor já define o status inicial e DataCriacao)
        var novaTarefa = new Tarefa(
            criarTarefaDto.ProjetoId,
            criarTarefaDto.UsuarioId,
            criarTarefaDto.Titulo,
            criarTarefaDto.Descricao,
            prioridade,
            criarTarefaDto.DataVencimento
        );

        projeto.Testar20Tarefa(novaTarefa);

        // 4. Persistir a nova tarefa (o histórico inicial é registrado no construtor da Tarefa)
        await _unitOfWork.Tarefas.CriarAsync(novaTarefa);
        _logger.LogInformation("Nova tarefa persistida no repositório com ID: {Id}", novaTarefa.Id);

        // 5. Commitar a transação
        await _unitOfWork.CommitAsync();
        _logger.LogInformation("Commit realizado após criação da nova tarefa.");

        // 6. Retornar o DTO da nova tarefa criada
        _logger.LogInformation("Tarefa criada com sucesso. Retornando DTO.");
        return _mapper.Map<TarefaDto>(novaTarefa);
    }

    /// <inheritdoc/>
    public async Task<TarefaDto> AtualizarAsync(Guid id, AtualizarTarefaDto atualizarTarefaDto)
    {
        _logger.LogInformation("Atualizando tarefa com ID: {Id} e dados: {@AtualizarTarefaDto}", id, atualizarTarefaDto);
        if (id == Guid.Empty)
        {
            _logger.LogWarning("ID da tarefa vazio ao tentar atualizar tarefa.");
            throw new ArgumentException("O ID da tarefa não pode ser vazio.", nameof(id));
        }

        var tarefaExistente = await _unitOfWork.Tarefas.ObterPorIdAsync(id);
        if (tarefaExistente == null)
        {
            _logger.LogWarning("Tarefa com ID {Id} não encontrada ao tentar atualizar.", id);
            throw new KeyNotFoundException($"Tarefa com ID {id} não encontrada.");
        }

        _logger.LogInformation("Tarefa com ID {Id} encontrada. Verificando alterações.", id);
        if (tarefaExistente.Titulo != atualizarTarefaDto.Titulo)
        {
            tarefaExistente.AtualizarTitulo(atualizarTarefaDto.Titulo, atualizarTarefaDto.UsuarioExecutorId); 
        }
        _logger.LogInformation("Título da tarefa atualizado para: {Titulo}", atualizarTarefaDto.Titulo);
        if (tarefaExistente.Descricao != atualizarTarefaDto.Descricao)
        {
            tarefaExistente.AtualizarDescricao(atualizarTarefaDto.Descricao, atualizarTarefaDto.UsuarioExecutorId); 
        }
        _logger.LogInformation("Descrição da tarefa atualizada para: {Descricao}", atualizarTarefaDto.Descricao);
        if (tarefaExistente.DataConclusao != atualizarTarefaDto.DataConclusao)
        {
            tarefaExistente.AtualizarDataConclusao(atualizarTarefaDto.DataConclusao, atualizarTarefaDto.UsuarioExecutorId); 
        }

        
        if (!string.IsNullOrEmpty(atualizarTarefaDto.Status))
        {
            var novoStatus = ParseStatus(atualizarTarefaDto.Status);
            if (tarefaExistente.Status != novoStatus)
            {
                try
                {
                    _logger.LogInformation("Alterando status da tarefa de {StatusAntigo} para {StatusNovo}", tarefaExistente.Status, novoStatus);
                    tarefaExistente.AlterarStatus(novoStatus, atualizarTarefaDto.UsuarioExecutorId); 
                }
                catch (ExcecaoDominio ex)
                {
                    _logger.LogError("Erro ao alterar status da tarefa: {Message}", ex.Message);
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }
        }

        
        await _unitOfWork.Tarefas.AtualizarAsync(tarefaExistente);
        _logger.LogInformation("Tarefa com ID {Id} atualizada no repositório.", id);
               
        await _unitOfWork.CommitAsync();
        _logger.LogInformation("Commit realizado após atualização da tarefa com ID {Id}.", id);

        _logger.LogInformation("Tarefa com ID {Id} atualizada com sucesso.", id);
        return _mapper.Map<TarefaDto>(tarefaExistente);
    }

    /// <inheritdoc/>
    public async Task<bool> ExcluirAsync(Guid id)
    {
        _logger.LogInformation("Excluindo tarefa com ID: {Id}", id);
        var tarefa = await _unitOfWork.Tarefas.ObterPorIdAsync(id);

        if (tarefa == null)
        {
            _logger.LogWarning("Tarefa com ID {Id} não encontrada ou já foi excluída logicamente.", id);
            return false; 
        }

        if (tarefa.Status == StatusTarefa.Concluida)
        {
            _logger.LogWarning("Não é possível excluir uma tarefa concluída. Tarefa ID: {Id}", id);
            throw new ExcecaoDominio("Não é possível excluir uma tarefa que já foi concluída.");
        }

        var sucesso = await _unitOfWork.Tarefas.ExcluirAsync(id);
        if (sucesso)
        {
            _logger.LogInformation("Tarefa com ID {Id} excluída logicamente com sucesso.", id);
            await _unitOfWork.CommitAsync();
        }
        else
        {
            _logger.LogWarning("Falha ao excluir tarefa com ID {Id}. A tarefa pode já ter sido excluída logicamente.", id);
            await _unitOfWork.RollbackAsync(); 
        }

        return sucesso;
    }

    /// <inheritdoc/>
    public async Task<TarefaDto> AdicionarComentarioAsync(CriarComentarioTarefaDto criarComentarioTarefaDto)
    {
        // 1. Verificar se a tarefa existe
        var tarefa = await _unitOfWork.Tarefas.ObterPorIdAsync(criarComentarioTarefaDto.TarefaId);
        if (tarefa == null)
            throw new KeyNotFoundException($"Tarefa com ID {criarComentarioTarefaDto.TarefaId} não encontrada.");

        // 2. Criar a entidade de domínio ComentarioTarefa
        var comentario = new ComentarioTarefa(
            criarComentarioTarefaDto.TarefaId,
            criarComentarioTarefaDto.UsuarioId,
            criarComentarioTarefaDto.Conteudo
        );

        // 3. Adicionar o comentário à tarefa (adiciona à coleção interna da entidade)
        // Isso é importante para que, se o DTO de Tarefa retornar a coleção de comentários, ele esteja atualizado.
        tarefa.AdicionarComentario(comentario);

        // 4. Persistir o comentário
        await _unitOfWork.ComentariosTarefas.CriarAsync(comentario);

        // 5. Registrar no histórico da tarefa que um comentário foi adicionado (regra específica)
        // A mensagem pode ser truncada para evitar histórico muito longo.
        var comentarioResumido = criarComentarioTarefaDto.Conteudo.Length > 50
                                 ? criarComentarioTarefaDto.Conteudo.Substring(0, 50) + "..."
                                 : criarComentarioTarefaDto.Conteudo;
        var historicoComentario = new HistoricoAlteracaoTarefa(
            criarComentarioTarefaDto.TarefaId,
            "Comentário Adicionado", // Campo Modificado
            null, // Valor Antigo
            $"Comentário: {comentarioResumido}", // Valor Novo
            criarComentarioTarefaDto.UsuarioId // O UsuarioId do DTO de Comentário é o executor
        );
        await _unitOfWork.HistoricosAlteracaoTarefa.CriarAsync(historicoComentario);

        // 6. Commitar a transação (criação do comentário e registro do histórico)
        await _unitOfWork.CommitAsync();

        // 7. Recarregar a tarefa para incluir o novo comentário no DTO retornado
        // Isso garante que o DTO retornado contenha o comentário recém-adicionado
        var tarefaAtualizada = await _unitOfWork.Tarefas.ObterPorIdAsync(criarComentarioTarefaDto.TarefaId);
        return _mapper.Map<TarefaDto>(tarefaAtualizada);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RelatorioDesempenhoDto>> ObterRelatorioDesempenhoUsuarioAsync(Guid usuarioId)
    {
        var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(usuarioId);
        if(usuario == null)
        {
            _logger.LogWarning("Usuário com ID {UsuarioId} não encontrado ao tentar obter relatório de desempenho.", usuarioId);
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");
        }


        var ehGerente = await _unitOfWork.Usuarios.VerificarSeGerenteAsync(usuarioId);
        if (!ehGerente)
            throw new UnauthorizedAccessException("Somente gerentes podem acessar relatórios de desempenho.");
            


        var contagemPorUsuario = await _unitOfWork.Tarefas.ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(DateTime.UtcNow.AddDays(-30));


        var relatorios = contagemPorUsuario.Select(c => new RelatorioDesempenhoDto
        {
            UsuarioId = c.Usuario.Id,
            NomeUsuario = c.Usuario.Nome,
            ContagemTarefasConcluidas = c.Contagem,
            MediaTarefasConcluidasPorDia = c.Contagem / 30.0 
        }).ToList();


        return relatorios;
    }

    /// <inheritdoc/>
    private static PrioridadeTarefa ParsePrioridade(string prioridade)
    {
        return prioridade.ToLower() switch
        {
            "baixa" => PrioridadeTarefa.Baixa,
            "media" => PrioridadeTarefa.Media,
            "alta" => PrioridadeTarefa.Alta,
            _ => PrioridadeTarefa.Media 
        };
    }

    /// <inheritdoc/>
    private static StatusTarefa ParseStatus(string status)
    {
        return status.ToLower() switch
        {
            "pendente" => StatusTarefa.Pendente,
            "emandamento" => StatusTarefa.EmAndamento,
            "concluida" => StatusTarefa.Concluida,
            "cancelada" => StatusTarefa.Cancelada,
            _ => StatusTarefa.Pendente 
        };
    }
}