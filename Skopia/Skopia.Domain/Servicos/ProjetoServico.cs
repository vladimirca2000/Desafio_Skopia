using Skopia.Domain.Repositorios.Interfaces;
using Skopia.Domain.Servicos.Interfaces;

namespace Skopia.Domain.Servicos;

/// <summary>
/// Implementa os serviços de domínio para a entidade Projeto.
/// Este serviço encapsula a lógica de negócio que envolve a consulta a outras entidades
/// (neste caso, Tarefas) para determinar o estado de um Projeto ou aplicar políticas.
/// </summary>
public class ProjetoServico : IProjetoServico
{
    private readonly IRepositorioTarefa _repositorioTarefa;

    /// <summary>
    /// Construtor que injeta as dependências necessárias (repositórios, etc.).
    /// </summary>
    /// <param name="repositorioTarefa">O repositório de tarefas, usado para consultar o estado das tarefas de um projeto.</param>
    /// <exception cref="ArgumentNullException">Lançada se alguma dependência for nula.</exception>
    public ProjetoServico(IRepositorioTarefa repositorioTarefa)
    {
        _repositorioTarefa = repositorioTarefa ?? throw new ArgumentNullException(nameof(repositorioTarefa));
    }

    /// <inheritdoc/>
    public async Task<bool> PodeRemoverProjetoAsync(Guid projetoId)
    {
        // A regra de negócio permanece: um projeto só pode ser soft-deletado se não tiver tarefas pendentes.
        // Para verificar isso, o serviço consulta o IRepositorioTarefa, que irá executar uma consulta
        // eficiente no banco de dados para verificar a existência de tarefas ativas e não concluídas
        // associadas a este projeto.
        var possuiTarefasPendentes = await _repositorioTarefa.PossuiTarefasPendentesParaProjetoAsync(projetoId);

        return !possuiTarefasPendentes;
    }
}