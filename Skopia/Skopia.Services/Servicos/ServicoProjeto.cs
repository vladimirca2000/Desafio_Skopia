using AutoMapper;
using Skopia.Domain.Entidades;
using Skopia.Domain.Servicos.Interfaces;
using Skopia.Services.Modelos;
using Skopia.Domain.Interfaces.UnitOfWork;
using Skopia.Services.Interfaces; // Adicione este using

namespace Skopia.Services.Servicos
{
    public class ServicoProjeto : IServicoProjeto
    {
        private readonly IUnitOfWork _unitOfWork; // Mudei de IRepositorioProjeto para IUnitOfWork
        private readonly IProjetoServico _servicoProjetoDominio;
        private readonly IMapper _mapper;

        public ServicoProjeto(IUnitOfWork unitOfWork, // Mudei o parâmetro para IUnitOfWork
                              IProjetoServico servicoProjetoDominio,
                              IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork)); // Atribuição
            _servicoProjetoDominio = servicoProjetoDominio ?? throw new ArgumentNullException(nameof(servicoProjetoDominio));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Métodos de leitura (não precisam de CommitAsync)
        public async Task<IEnumerable<ProjetoDto>> ObterTodosPorUsuarioIdAsync(Guid usuarioId)
        {
            var projetos = await _unitOfWork.Projetos.ObterTodosPorUsuarioIdAsync(usuarioId); // Acessa via UnitOfWork
            return _mapper.Map<IEnumerable<ProjetoDto>>(projetos);
        }

        public async Task<ProjetoDto?> ObterPorIdAsync(Guid id)
        {
            var projeto = await _unitOfWork.Projetos.ObterPorIdAsync(id); // Acessa via UnitOfWork
            if (projeto == null)
            {
                return null;
            }
            return _mapper.Map<ProjetoDto>(projeto);
        }

        // Métodos de escrita (precisam de CommitAsync)
        public async Task<ProjetoDto> CriarAsync(CriarProjetoDto criarProjetoDto)
        {
            var projeto = new Projeto(
                criarProjetoDto.Nome,
                criarProjetoDto.Descricao,
                criarProjetoDto.UsuarioId
            );

            var projetoCriado = await _unitOfWork.Projetos.CriarAsync(projeto); // Acessa via UnitOfWork
            await _unitOfWork.CommitAsync(); // Commita a transação
            return _mapper.Map<ProjetoDto>(projetoCriado);
        }

        public async Task<ProjetoDto> AtualizarAsync(Guid id, AtualizarProjetoDto atualizarProjetoDto)
        {
            var projetoExistente = await _unitOfWork.Projetos.ObterPorIdAsync(id); // Acessa via UnitOfWork
            if (projetoExistente == null)
                throw new KeyNotFoundException($"Projeto com ID {id} não encontrado.");

            projetoExistente.AtualizarNome(atualizarProjetoDto.Nome);
            projetoExistente.AtualizarDescricao(atualizarProjetoDto.Descricao);

            var projetoAtualizado = await _unitOfWork.Projetos.AtualizarAsync(projetoExistente); // Acessa via UnitOfWork
            await _unitOfWork.CommitAsync(); // Commita a transação
            return _mapper.Map<ProjetoDto>(projetoAtualizado);
        }

        public async Task<bool> ExcluirAsync(Guid id)
        {
            var projetoExiste = await _unitOfWork.Projetos.ObterPorIdAsync(id); // Acessa via UnitOfWork
            if (projetoExiste == null)
                throw new KeyNotFoundException($"Projeto com ID {id} não encontrado para exclusão.");

            var podeRemover = await _servicoProjetoDominio.PodeRemoverProjetoAsync(id);

            if (!podeRemover)
            {
                throw new InvalidOperationException("Não é possível excluir o projeto porque ele possui tarefas pendentes. Conclua ou remova todas as tarefas primeiro.");
            }

            var sucesso = await _unitOfWork.Projetos.ExcluirAsync(id); // Acessa via UnitOfWork
            if (sucesso)
            {
                await _unitOfWork.CommitAsync(); // Commita a transação
            }
            return sucesso;
        }
    }
}