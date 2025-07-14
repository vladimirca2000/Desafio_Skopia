namespace Skopia.Domain.Servicos.Interfaces;

public interface IProjetoServico
{
    Task<bool> PodeRemoverProjetoAsync(Guid projetoId);
}
