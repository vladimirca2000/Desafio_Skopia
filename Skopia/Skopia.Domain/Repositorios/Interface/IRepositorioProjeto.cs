using Skopia.Domain.Entidades; 

namespace Skopia.Domain.Repositorios.Interfaces;


public interface IRepositorioProjeto
{
    
    Task<IEnumerable<Projeto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId);

    Task<Projeto?> ObterPorIdAsync(Guid id);

    Task<Projeto> CriarAsync(Projeto projeto);

    Task<Projeto> AtualizarAsync(Projeto projeto);

    Task<bool> ExcluirAsync(Guid id);

    Task<bool> PossuiTarefasPendentesAsync(Guid projetoId);

    Task<int> ObterContagemTarefasAsync(Guid projetoId);

    

}