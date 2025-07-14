using Skopia.Domain.Entidades; 

namespace Skopia.Domain.Repositorios.Interfaces;


public interface IRepositorioComentarioTarefa
{
    
    Task<IEnumerable<ComentarioTarefa>> ObterTodosPorTarefaIdAsync(Guid tarefaId);
        
    Task<ComentarioTarefa> CriarAsync(ComentarioTarefa comentario);
}
