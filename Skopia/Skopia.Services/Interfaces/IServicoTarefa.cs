using Skopia.Services.Modelos; // Novo namespace para seus DTOs

namespace Skopia.Services.Interfaces;


public interface IServicoTarefa
{    
    Task<IEnumerable<TarefaDto>> ObterTodosPorProjetoIdAsync(Guid projetoId);

    Task<TarefaDto?> ObterPorIdAsync(Guid id);

    Task<TarefaDto> CriarAsync(CriarTarefaDto criarTarefaDto);

    Task<TarefaDto> AtualizarAsync(Guid id, AtualizarTarefaDto atualizarTarefaDto);

    Task<bool> ExcluirAsync(Guid id);

    Task<TarefaDto> AdicionarComentarioAsync(CriarComentarioTarefaDto criarComentarioTarefaDto);

    Task<IEnumerable<RelatorioDesempenhoDto>> ObterRelatorioDesempenhoUsuarioAsync(Guid usuarioId);
}