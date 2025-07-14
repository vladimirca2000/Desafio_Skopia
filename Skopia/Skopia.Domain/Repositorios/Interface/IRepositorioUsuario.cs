using Skopia.Domain.Entidades; 

public interface IRepositorioUsuario
{
    
    Task<Usuario?> ObterPorIdAsync(Guid id);

    Task<bool> VerificarSeGerenteAsync(Guid usuarioId);
}