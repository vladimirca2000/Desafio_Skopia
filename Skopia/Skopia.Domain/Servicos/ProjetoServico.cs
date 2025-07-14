using Skopia.Domain.Repositorios.Interfaces;
using Skopia.Domain.Servicos.Interfaces;

namespace Skopia.Domain.Servicos;


public class ProjetoServico : IProjetoServico
{
    private readonly IRepositorioTarefa _repositorioTarefa;

    public ProjetoServico(IRepositorioTarefa repositorioTarefa)
    {
        _repositorioTarefa = repositorioTarefa ?? throw new ArgumentNullException(nameof(repositorioTarefa));
    }

    public async Task<bool> PodeRemoverProjetoAsync(Guid projetoId)
    {
        var possuiTarefasPendentes = await _repositorioTarefa.PossuiTarefasPendentesParaProjetoAsync(projetoId);

        return !possuiTarefasPendentes;
    }
}