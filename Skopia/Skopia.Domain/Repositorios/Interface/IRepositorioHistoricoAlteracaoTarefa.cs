using Skopia.Domain.Entidades;
namespace Skopia.Domain.Repositorios.Interfaces;


public interface IRepositorioHistoricoAlteracaoTarefa
{
    
    Task<IEnumerable<HistoricoAlteracaoTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId);

    Task<HistoricoAlteracaoTarefa> CriarAsync(HistoricoAlteracaoTarefa historicoAlteracaoTarefa);
}
