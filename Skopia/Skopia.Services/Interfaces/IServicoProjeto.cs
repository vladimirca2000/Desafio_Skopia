using Skopia.Services.Modelos; // Novo namespace para seus DTOs, seguindo a tradução

namespace Skopia.Services.Interfaces;


public interface IServicoProjeto
{
   
    Task<IEnumerable<ProjetoDto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId);
        
    Task<ProjetoDto?> ObterPorIdAsync(Guid id);
        
    Task<ProjetoDto> CriarAsync(CriarProjetoDto projetoDto);
        
    Task<ProjetoDto> AtualizarAsync(Guid id, AtualizarProjetoDto projetoDto);
        
    Task<bool> ExcluirAsync(Guid id);
}
