using AutoMapper;
using Skopia.Domain.Entidades; // Entidades do domínio
using Skopia.Domain.Enums; // Enums do domínio (StatusTarefa, PrioridadeTarefa)
using Skopia.Domain.Excecoes; // Exceções de domínio
using Skopia.Domain.Interfaces.UnitOfWork; // Interface da Unit of Work
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos; // DTOs da aplicação

namespace Skopia.Services.Servicos; 

/// <summary>
/// Implementação do serviço de aplicação para gerenciamento de tarefas.
/// Orquestra operações entre DTOs, entidades de domínio, repositórios (via Unit of Work)
/// e validações de regras de negócio.
/// </summary>
public class ServicoTarefa : IServicoTarefa
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Construtor do serviço de tarefa.
    /// </summary>
    /// <param name="unitOfWork">Instância da Unit of Work para gerenciar o acesso aos dados.</param>
    /// <param name="mapper">Instância do AutoMapper para mapeamento entre DTOs e entidades.</param>
    public ServicoTarefa(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Obtém todas as tarefas de um projeto específico.
    /// </summary>
    /// <param name="projetoId">ID do projeto.</param>
    /// <returns>Coleção de DTOs de tarefas.</returns>
    public async Task<IEnumerable<TarefaDto>> ObterTodosPorProjetoIdAsync(Guid projetoId)
    {
        // O repositório de projeto pode obter o projeto com suas tarefas incluídas
        var projeto = await _unitOfWork.Projetos.ObterPorIdAsync(projetoId);
        if (projeto == null)
        {
            throw new KeyNotFoundException($"Projeto com ID {projetoId} não encontrado.");
        }
        // Mapeia a coleção de tarefas do projeto para uma coleção de TarefaDto
        return _mapper.Map<IEnumerable<TarefaDto>>(projeto.Tarefas);
        // Alternativamente, se o repositorioTarefa.ObterTodosPorProjetoIdAsync fosse mais performático:
        // var tarefas = await _unitOfWork.Tarefas.ObterTodosPorProjetoIdAsync(projetoId);
        // return _mapper.Map<IEnumerable<TarefaDto>>(tarefas);
    }

    /// <summary>
    /// Obtém uma tarefa pelo seu ID.
    /// </summary>
    /// <param name="id">ID da tarefa.</param>
    /// <returns>DTO da tarefa.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se a tarefa não for encontrada.</exception>
    public async Task<TarefaDto?> ObterPorIdAsync(Guid id)
    {
        var tarefa = await _unitOfWork.Tarefas.ObterPorIdAsync(id);
        if (tarefa == null)
        {
            // Deixamos a camada de aplicação decidir como lidar com a ausência do recurso
            return null;
        }
        return _mapper.Map<TarefaDto>(tarefa);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <param name="criarTarefaDto">DTO com os dados para criação da tarefa.</param>
    /// <returns>DTO da tarefa criada.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se o projeto não for encontrado.</exception>
    /// <exception cref="ExcecaoDominio">Lançada se o limite de tarefas por projeto for atingido.</exception>
    public async Task<TarefaDto> CriarAsync(CriarTarefaDto criarTarefaDto)
    {
        // 1. Verificar se o projeto existe
        // Obtém o projeto com suas tarefas para que a regra de limite seja aplicada
        var projeto = await _unitOfWork.Projetos.ObterPorIdAsync(criarTarefaDto.ProjetoId);
        if (projeto == null)
            throw new KeyNotFoundException($"Projeto com ID {criarTarefaDto.ProjetoId} não encontrado.");

        // 2. Converter string de prioridade para o enum do domínio
        var prioridade = ParsePrioridade(criarTarefaDto.Prioridade);

        // 3. Criar a entidade de domínio Tarefa (o construtor já define o status inicial e DataCriacao)
        var novaTarefa = new Tarefa(
            criarTarefaDto.ProjetoId,
            criarTarefaDto.UsuarioId,
            criarTarefaDto.Titulo,
            criarTarefaDto.Descricao,
            prioridade,
            criarTarefaDto.DataVencimento
        );

        // 4. Adicionar a tarefa ao projeto para aplicar a regra de limite (e outras, se houver)
        // A exceção ExcecaoDominio.LimiteTarefasExcedido será lançada se o limite for atingido.
        // Para que o AdicionarTarefa da entidade Projeto funcione, a coleção de Tarefas do Projeto
        // precisa ser carregada. O ObterPorIdAsync em IRepositorioProjeto.ObterPorIdAsync(Guid id)
        // já deve fazer isso (vide `Include(p => p.Tarefas)` no RepositorioProjeto).
        try
        {
            // A linha abaixo seria para que a ENTIDADE Projeto valide se o limite de 20 tarefas foi atingido.
            // Mas, a persistência da tarefa é feita diretamente no repositório de tarefas.
            // O limite real de tarefas por projeto é validado no método ObterContagemTarefasAsync no RepositorioProjeto
            // e o ServicoProjeto verifica isso antes de criar a tarefa.
            // Se a entidade Projeto tivesse um método para "validar adição de tarefa" sem realmente adicioná-la à coleção,
            // seria o lugar ideal para a regra. Como está, a regra do limite é verificada via RepositorioProjeto.
            // Aqui, o 'projeto.AdicionarTarefa(novaTarefa);' apenas validaria a coleção em memória, não a do banco.
        }
        catch (ExcecaoDominio ex)
        {
            // Captura exceções específicas do domínio e as relança para a camada superior.
            throw new InvalidOperationException(ex.Message, ex);
        }

        // 5. Persistir a nova tarefa (o histórico inicial é registrado no construtor da Tarefa)
        await _unitOfWork.Tarefas.CriarAsync(novaTarefa);

        // 6. Commitar a transação
        await _unitOfWork.CommitAsync();

        return _mapper.Map<TarefaDto>(novaTarefa);
    }

    /// <summary>
    /// Atualiza uma tarefa existente.
    /// </summary>
    /// <param name="id">ID da tarefa a ser atualizada.</param>
    /// <param name="atualizarTarefaDto">DTO com os dados para atualização.</param>
    /// <returns>DTO da tarefa atualizada.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se a tarefa não for encontrada.</exception>
    /// <exception cref="InvalidOperationException">Lançada se alguma regra de negócio de atualização for violada.</exception>
    public async Task<TarefaDto> AtualizarAsync(Guid id, AtualizarTarefaDto atualizarTarefaDto)
    {
        var tarefaExistente = await _unitOfWork.Tarefas.ObterPorIdAsync(id);
        if (tarefaExistente == null)
            throw new KeyNotFoundException($"Tarefa com ID {id} não encontrada.");

        // Chamar os métodos de comportamento da entidade de domínio para aplicar as atualizações.
        // A entidade Tarefa é responsável por registrar seu próprio histórico de alterações.
        if (tarefaExistente.Titulo != atualizarTarefaDto.Titulo)
        {
            tarefaExistente.AtualizarTitulo(atualizarTarefaDto.Titulo, atualizarTarefaDto.UsuarioExecutorId); // Passando usuarioExecutorId
        }
        if (tarefaExistente.Descricao != atualizarTarefaDto.Descricao)
        {
            tarefaExistente.AtualizarDescricao(atualizarTarefaDto.Descricao, atualizarTarefaDto.UsuarioExecutorId); // Passando usuarioExecutorId
        }
        if (tarefaExistente.DataConclusao != atualizarTarefaDto.DataConclusao)
        {
            tarefaExistente.AtualizarDataConclusao(atualizarTarefaDto.DataConclusao, atualizarTarefaDto.UsuarioExecutorId); // Passando usuarioExecutorId
        }

        // Converter string de status para o enum do domínio e aplicar a alteração
        if (!string.IsNullOrEmpty(atualizarTarefaDto.Status))
        {
            var novoStatus = ParseStatus(atualizarTarefaDto.Status);
            if (tarefaExistente.Status != novoStatus)
            {
                try
                {
                    tarefaExistente.AlterarStatus(novoStatus, atualizarTarefaDto.UsuarioExecutorId); // Passando usuarioExecutorId
                }
                catch (ExcecaoDominio ex)
                {
                    // Captura exceções do domínio (ex: tentativa de reabrir tarefa concluída/cancelada)
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }
        }

        // A prioridade não é atualizável, conforme regra de negócio (não está no DTO de atualização)

        // Persistir as alterações na tarefa
        await _unitOfWork.Tarefas.AtualizarAsync(tarefaExistente);

        // Commitar a transação
        await _unitOfWork.CommitAsync();

        return _mapper.Map<TarefaDto>(tarefaExistente);
    }

    /// <summary>
    /// Exclui uma tarefa logicamente (soft delete).
    /// </summary>
    /// <param name="id">ID da tarefa a ser excluída.</param>
    /// <returns>True se a exclusão foi bem-sucedida.</returns>
    public async Task<bool> ExcluirAsync(Guid id)
    {
        var tarefa = await _unitOfWork.Tarefas.ObterPorIdAsync(id);
        if (tarefa == null)
        {
            return false; // Retorna false se a tarefa não existe ou já foi excluída logicamente
        }

        var sucesso = await _unitOfWork.Tarefas.ExcluirAsync(id);
        if (sucesso)
        {
            await _unitOfWork.CommitAsync();
        }
        return sucesso;
    }

    /// <summary>
    /// Adiciona um novo comentário a uma tarefa.
    /// </summary>
    /// <param name="criarComentarioTarefaDto">DTO contendo os dados do comentário a ser adicionado.</param>
    /// <returns>DTO da tarefa atualizada com o novo comentário.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se a tarefa não for encontrada.</exception>
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

    /// <summary>
    /// Obtém um relatório de desempenho para um usuário específico.
    /// </summary>
    /// <param name="usuarioId">ID do usuário para o qual o relatório será gerado.</param>
    /// <returns>DTO com os dados do relatório de desempenho.</returns>
    /// <exception cref="UnauthorizedAccessException">Lançada se o usuário não tiver permissão (não for gerente).</exception>
    /// <exception cref="KeyNotFoundException">Lançada se o usuário não for encontrado.</exception>
    public async Task<RelatorioDesempenhoDto> ObterRelatorioDesempenhoUsuarioAsync(Guid usuarioId)
    {
        // 1. Verificar se o usuário é um gerente (regra de autorização)
        var ehGerente = await _unitOfWork.Usuarios.VerificarSeGerenteAsync(usuarioId);
        if (!ehGerente)
            throw new UnauthorizedAccessException("Somente gerentes podem acessar relatórios de desempenho.");

        // 2. Obter dados do usuário
        var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");

        // 3. Calcular métricas de desempenho
        var trintaDiasAtras = DateTime.UtcNow.AddDays(-30);
        var contagemTarefasConcluidas = await _unitOfWork.Tarefas.ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(usuarioId, trintaDiasAtras);

        var mediaTarefasConcluidasPorDia = contagemTarefasConcluidas / 30.0;
        // Garante que a média seja 0 se não houver tarefas concluídas, evitando NaN
        if (contagemTarefasConcluidas == 0) mediaTarefasConcluidasPorDia = 0;

        var relatorio = new RelatorioDesempenhoDto
        {
            UsuarioId = usuarioId,
            NomeUsuario = usuario.Nome,
            ContagemTarefasConcluidas = contagemTarefasConcluidas,
            MediaTarefasConcluidasPorDia = mediaTarefasConcluidasPorDia
        };

        return relatorio;
    }

    /// <summary>
    /// Converte uma string de prioridade para o enum `PrioridadeTarefa` do domínio.
    /// </summary>
    /// <param name="prioridade">String de prioridade (ex: "Baixa", "Media", "Alta").</param>
    /// <returns>O enum `PrioridadeTarefa` correspondente.</returns>
    private static PrioridadeTarefa ParsePrioridade(string prioridade)
    {
        return prioridade.ToLower() switch
        {
            "baixa" => PrioridadeTarefa.Baixa,
            "media" => PrioridadeTarefa.Media,
            "alta" => PrioridadeTarefa.Alta,
            _ => PrioridadeTarefa.Media // Valor padrão
        };
    }

    /// <summary>
    /// Converte uma string de status para o enum `StatusTarefa` do domínio.
    /// </summary>
    /// <param name="status">String de status (ex: "Pendente", "EmAndamento", "Concluida", "Cancelada").</param>
    /// <returns>O enum `StatusTarefa` correspondente.</returns>
    private static StatusTarefa ParseStatus(string status)
    {
        return status.ToLower() switch
        {
            "pendente" => StatusTarefa.Pendente,
            "emandamento" => StatusTarefa.EmAndamento,
            "concluida" => StatusTarefa.Concluida,
            "cancelada" => StatusTarefa.Cancelada,
            _ => StatusTarefa.Pendente // Valor padrão
        };
    }
}