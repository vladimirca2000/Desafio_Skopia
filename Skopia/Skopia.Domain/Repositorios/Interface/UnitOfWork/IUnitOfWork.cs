using Skopia.Domain.Repositorios.Interfaces;

namespace Skopia.Domain.Interfaces.UnitOfWork;

public interface IUnitOfWork : IDisposable
{    
    IRepositorioProjeto Projetos { get; }
        
    IRepositorioTarefa Tarefas { get; }
        
    IRepositorioUsuario Usuarios { get; }
        
    IRepositorioComentarioTarefa ComentariosTarefas { get; }
        
    IRepositorioHistoricoAlteracaoTarefa HistoricosAlteracaoTarefa { get; }
        
    Task<int> CommitAsync();

    Task RollbackAsync();
}