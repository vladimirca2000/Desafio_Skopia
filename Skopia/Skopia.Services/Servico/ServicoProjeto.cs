using AutoMapper;
using Skopia.Domain.Entidades; // Entidades do domínio: **VERIFIQUE SE ESTA LINHA ESTÁ PRESENTE**
using Skopia.Domain.Repositorios.Interfaces; // Interfaces dos repositórios
using Skopia.Domain.Servicos.Interfaces; // Interfaces dos serviços de domínio
using Skopia.Servicos.Interfaces; // Interfaces dos serviços da aplicação
using Skopia.Servicos.Modelos; // DTOs da aplicação

namespace Skopia.Services.Servico;

/// <summary>
/// Implementação do serviço de aplicação para gerenciamento de projetos.
/// Orquestra operações entre DTOs, entidades de domínio, repositórios e serviços de domínio.
/// </summary>
public class ServicoProjeto : IServicoProjeto
{
    private readonly IRepositorioProjeto _repositorioProjeto;
    private readonly IProjetoService _servicoProjetoDominio; // Injeção do serviço de domínio
    private readonly IMapper _mapper;

    /// <summary>
    /// Construtor do serviço de projeto.
    /// </summary>
    /// <param name="repositorioProjeto">Repositório para acesso aos dados de projetos.</param>
    /// <param name="servicoProjetoDominio">Serviço de domínio para validações específicas de projeto.</param>
    /// <param name="mapper">Instância do AutoMapper para mapeamento entre DTOs e entidades.</param>
    public ServicoProjeto(IRepositorioProjeto repositorioProjeto,
                          IProjetoService servicoProjetoDominio,
                          IMapper mapper)
    {
        _repositorioProjeto = repositorioProjeto ?? throw new ArgumentNullException(nameof(repositorioProjeto));
        _servicoProjetoDominio = servicoProjetoDominio ?? throw new ArgumentNullException(nameof(servicoProjetoDominio));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Obtém todos os projetos de um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <returns>Coleção de DTOs de projetos.</returns>
    public async Task<IEnumerable<ProjetoDto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId)
    {
        var projetos = await _repositorioProjeto.ObterTodosPorUsuarioIdAsync(usuarioId);
        return _mapper.Map<IEnumerable<ProjetoDto>>(projetos);
    }

    /// <summary>
    /// Obtém um projeto pelo seu ID.
    /// </summary>
    /// <param name="id">ID do projeto.</param>
    /// <returns>DTO do projeto.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se o projeto não for encontrado.</exception>
    public async Task<ProjetoDto?> ObterPorIdAsync(Guid id)
    {
        var projeto = await _repositorioProjeto.ObterPorIdAsync(id);
        if (projeto == null)
        {
            return null;
        }
        return _mapper.Map<ProjetoDto>(projeto);
    }

    /// <summary>
    /// Cria um novo projeto.
    /// </summary>
    /// <param name="criarProjetoDto">DTO com os dados para criação do projeto.</param>
    /// <returns>DTO do projeto criado.</returns>
    public async Task<ProjetoDto> CriarAsync(CriarProjetoDto criarProjetoDto)
    {
        // O construtor da entidade Projeto já define DataCriacao = DateTime.UtcNow e UsuarioId
        // Ele também faz as validações iniciais.
        var projeto = new Projeto(
            criarProjetoDto.Nome,
            criarProjetoDto.Descricao,
            criarProjetoDto.UsuarioId // Este deve ser do tipo Guid
        );

        var projetoCriado = await _repositorioProjeto.CriarAsync(projeto);
        return _mapper.Map<ProjetoDto>(projetoCriado);
    }

    /// <summary>
    /// Atualiza um projeto existente.
    /// </summary>
    /// <param name="id">ID do projeto a ser atualizado.</param>
    /// <param name="atualizarProjetoDto">DTO com os dados para atualização.</param>
    /// <returns>DTO do projeto atualizado.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se o projeto não for encontrado.</exception>
    public async Task<ProjetoDto> AtualizarAsync(Guid id, AtualizarProjetoDto atualizarProjetoDto)
    {
        var projetoExistente = await _repositorioProjeto.ObterPorIdAsync(id);
        if (projetoExistente == null)
            throw new KeyNotFoundException($"Projeto com ID {id} não encontrado.");

        // Chamando os métodos da entidade de domínio para aplicar as atualizações
        projetoExistente.AtualizarNome(atualizarProjetoDto.Nome);
        projetoExistente.AtualizarDescricao(atualizarProjetoDto.Descricao);

        var projetoAtualizado = await _repositorioProjeto.AtualizarAsync(projetoExistente);
        return _mapper.Map<ProjetoDto>(projetoAtualizado);
    }

    /// <summary>
    /// Exclui um projeto logicamente (soft delete).
    /// </summary>
    /// <param name="id">ID do projeto a ser excluído.</param>
    /// <returns>True se a exclusão foi bem-sucedida.</returns>
    /// <exception cref="KeyNotFoundException">Lançada se o projeto não for encontrado.</exception>
    /// <exception cref="InvalidOperationException">Lançada se o projeto não puder ser excluído devido a tarefas pendentes.</exception>
    public async Task<bool> ExcluirAsync(Guid id)
    {
        var projetoExiste = await _repositorioProjeto.ObterPorIdAsync(id);
        if (projetoExiste == null)
            throw new KeyNotFoundException($"Projeto com ID {id} não encontrado para exclusão.");

        var podeRemover = await _servicoProjetoDominio.PodeRemoverProjetoAsync(id);

        if (!podeRemover)
        {
            throw new InvalidOperationException("Não é possível excluir o projeto porque ele possui tarefas pendentes. Conclua ou remova todas as tarefas primeiro.");
        }

        return await _repositorioProjeto.ExcluirAsync(id);
    }
}