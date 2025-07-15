using AutoMapper;
using Microsoft.Extensions.Logging; 
using Skopia.Domain.Entidades;
using Skopia.Domain.Excecoes;
using Skopia.Domain.Interfaces.UnitOfWork;
using Skopia.Domain.Servicos.Interfaces;
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos;

namespace Skopia.Services.Servicos
{
    public class ServicoProjeto : IServicoProjeto
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IProjetoServico _servicoProjetoDominio;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicoProjeto> _logger; 

        public ServicoProjeto(IUnitOfWork unitOfWork, 
                              IProjetoServico servicoProjetoDominio,
                              IMapper mapper,
                              ILogger<ServicoProjeto> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork)); 
            _servicoProjetoDominio = servicoProjetoDominio ?? throw new ArgumentNullException(nameof(servicoProjetoDominio));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProjetoDto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId)
        {
            _logger.LogInformation("Obtendo todos os projetos para o usuário ID: {UsuarioId}", usuarioId);

            if (usuarioId == Guid.Empty)
            {
                _logger.LogWarning("Tentativa de obter projetos com ID de usuário vazio.");
                throw new ArgumentException("O ID do usuário não pode ser vazio.", nameof(usuarioId));
            }

            _logger.LogInformation("Iniciando a busca de projetos para o usuário ID: {UsuarioId}", usuarioId);
            var projetos = await _unitOfWork.Projetos.ObterTodosPorUsuarioIdAsync(usuarioId); 

            if (projetos == null || !projetos.Any())
            {
                _logger.LogInformation("Nenhum projeto encontrado para o usuário ID: {UsuarioId}", usuarioId);
                return Enumerable.Empty<ProjetoDto>();
            }
            _logger.LogInformation("Total de projetos encontrados para o usuário ID {UsuarioId}: {TotalProjetos}", usuarioId, projetos.Count());
            return _mapper.Map<IEnumerable<ProjetoDto>>(projetos);
        }

        /// <inheritdoc/>
        public async Task<ProjetoDto?> ObterPorIdAsync(Guid id)
        {
            _logger.LogInformation("Obtendo projeto com ID: {Id}", id);

            if (id == Guid.Empty)
            {
                _logger.LogWarning("Tentativa de obter projeto com ID vazio.");
                throw new ArgumentException("O ID do projeto não pode ser vazio.", nameof(id));
            }

            _logger.LogInformation("Iniciando a busca do projeto com ID: {Id}", id);
            var projeto = await _unitOfWork.Projetos.ObterPorIdAsync(id); 
            if (projeto == null)
            {
                _logger.LogInformation("Projeto com ID {Id} não encontrado.", id);
                return null;
            }

            _logger.LogInformation("Projeto com ID {Id} encontrado.", id);
            return _mapper.Map<ProjetoDto>(projeto);
        }

        /// <inheritdoc/>
        public async Task<ProjetoDto> CriarAsync(CriarProjetoDto criarProjetoDto)
        {
            _logger.LogInformation("Criando novo projeto com dados: {@CriarProjetoDto}", criarProjetoDto);
            if (criarProjetoDto == null)
            {
                _logger.LogWarning("Dados de criação de projeto são nulos.");
                throw new ArgumentNullException(nameof(criarProjetoDto), "Os dados do projeto não podem ser nulos.");
            }
            if (!Guid.TryParse(criarProjetoDto.UsuarioId.ToString(), out var usuarioId) || usuarioId == Guid.Empty)
            {
                _logger.LogWarning("ID de usuário inválido ao tentar criar projeto.");
                throw new ArgumentException("O ID do usuário não pode ser vazio ou inválido.", nameof(criarProjetoDto.UsuarioId));
            }

            var projeto = new Projeto(
                criarProjetoDto.Nome,
                criarProjetoDto.Descricao,
                criarProjetoDto.UsuarioId
            );

            _logger.LogInformation("Iniciando a criação do projeto no repositório.");
            var projetoCriado = await _unitOfWork.Projetos.CriarAsync(projeto); 

            if (projetoCriado == null)
            {
                _logger.LogError("Falha ao criar o projeto.");
                await _unitOfWork.RollbackAsync();
                throw new InvalidOperationException("Não foi possível criar o projeto.");
            }
            await _unitOfWork.CommitAsync(); 
            _logger.LogInformation("Projeto criado com sucesso: {Id}", projetoCriado.Id);
            return _mapper.Map<ProjetoDto>(projetoCriado);
        }

        /// <inheritdoc/>
        public async Task<ProjetoDto> AtualizarAsync(Guid id, AtualizarProjetoDto atualizarProjetoDto)
        {
            _logger.LogInformation("Atualizando projeto com ID: {Id} e dados: {@AtualizarProjetoDto}", id, atualizarProjetoDto);
            if (id == Guid.Empty)
            {
                _logger.LogWarning("ID do projeto vazio ao tentar atualizar projeto.");
                throw new ArgumentException("O ID do projeto não pode ser vazio.", nameof(id));
            }
            if (atualizarProjetoDto == null)
            {
                _logger.LogWarning("Dados de atualização de projeto são nulos.");
                throw new ArgumentNullException(nameof(atualizarProjetoDto), "Os dados do projeto não podem ser nulos.");
            }

            var projetoExistente = await _unitOfWork.Projetos.ObterPorIdAsync(id); 

            if (projetoExistente == null)
            { 
                _logger.LogWarning("Projeto com ID {Id} não encontrado para atualização.", id);
                throw new KeyNotFoundException($"Projeto com ID {id} não encontrado.");
            }

            projetoExistente.AtualizarNome(atualizarProjetoDto.Nome);
            projetoExistente.AtualizarDescricao(atualizarProjetoDto.Descricao);

            var projetoAtualizado = await _unitOfWork.Projetos.AtualizarAsync(projetoExistente); 

            if (projetoAtualizado == null)
            {
                _logger.LogError("Falha ao atualizar o projeto com ID {Id}.", id);
                await _unitOfWork.RollbackAsync();
                throw new InvalidOperationException($"Não foi possível atualizar o projeto com ID {id}.");
            }
            await _unitOfWork.CommitAsync(); 
            _logger.LogInformation("Projeto com ID {Id} atualizado com sucesso.", id);
            return _mapper.Map<ProjetoDto>(projetoAtualizado);
        }

        /// <inheritdoc/>
        public async Task<bool> ExcluirAsync(Guid id)
        {
            _logger.LogInformation("Excluindo projeto com ID: {Id}", id);
            if (id == Guid.Empty)
            {
                _logger.LogWarning("ID do projeto vazio ao tentar excluir projeto.");
                throw new ArgumentException("O ID do projeto não pode ser vazio.", nameof(id));
            }

            _logger.LogInformation("Iniciando a verificação de existência do projeto com ID: {Id}", id);
            var projetoExiste = await _unitOfWork.Projetos.ObterPorIdAsync(id); 
            if (projetoExiste == null)
            {
                _logger.LogWarning("Projeto com ID {Id} não encontrado para exclusão.", id);
                throw new KeyNotFoundException($"Projeto com ID {id} não encontrado para exclusão.");
            }

            var podeRemover = await _servicoProjetoDominio.PodeRemoverProjetoAsync(id);

            if (!podeRemover)
            {
                _logger.LogWarning("Tentativa de exclusão de projeto com ID {Id} falhou porque ele possui tarefas pendentes.", id);
                throw new ExcecaoDominio("Não é possível excluir o projeto porque ele possui tarefas pendentes. Conclua ou remova todas as tarefas primeiro.");
            }

            var sucesso = await _unitOfWork.Projetos.ExcluirAsync(id); 
            if (sucesso)
            {                
                await _unitOfWork.CommitAsync(); 
                _logger.LogInformation("Projeto com ID {Id} excluído com sucesso.", id);
            }
            else
            {
                await _unitOfWork.CommitAsync();
                _logger.LogWarning("Tentativa de exclusão de projeto com ID {Id} falhou. O projeto pode já estar excluído ou não existir.", id);
                throw new InvalidOperationException($"Não foi possível excluir o projeto com ID {id}. Ele pode já estar excluído ou não existir.");
            }

            return sucesso;
        }
    }
}