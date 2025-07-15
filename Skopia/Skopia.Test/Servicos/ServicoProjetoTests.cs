using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Skopia.Domain.Entidades;
using Skopia.Domain.Excecoes;
using Skopia.Domain.Interfaces.UnitOfWork;
using Skopia.Domain.Servicos.Interfaces;
using Skopia.Services.Modelos;
using Skopia.Services.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Skopia.Test.Servicos;

public class ServicoProjetoTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProjetoServico> _mockProjetoServicoDominio;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ServicoProjeto>> _mockLogger;
    private readonly ServicoProjeto _servicoProjeto;

    public ServicoProjetoTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProjetoServicoDominio = new Mock<IProjetoServico>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ServicoProjeto>>();
        _servicoProjeto = new ServicoProjeto(
            _mockUnitOfWork.Object,
            _mockProjetoServicoDominio.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region Testes do Construtor

    [Fact]
    public void Construtor_ComParametrosValidos_DeveCriarInstancia()
    {
        // Arrange & Act
        var servico = new ServicoProjeto(
            _mockUnitOfWork.Object,
            _mockProjetoServicoDominio.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );

        // Assert
        Assert.NotNull(servico);
    }

    [Fact]
    public void Construtor_ComUnitOfWorkNulo_DeveLancarArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServicoProjeto(
            null,
            _mockProjetoServicoDominio.Object,
            _mockMapper.Object,
            _mockLogger.Object
        ));
    }

    [Fact]
    public void Construtor_ComProjetoServicoDominioNulo_DeveLancarArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServicoProjeto(
            _mockUnitOfWork.Object,
            null,
            _mockMapper.Object,
            _mockLogger.Object
        ));
    }

    [Fact]
    public void Construtor_ComMapperNulo_DeveLancarArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServicoProjeto(
            _mockUnitOfWork.Object,
            _mockProjetoServicoDominio.Object,
            null,
            _mockLogger.Object
        ));
    }

    [Fact]
    public void Construtor_ComLoggerNulo_DeveLancarArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServicoProjeto(
            _mockUnitOfWork.Object,
            _mockProjetoServicoDominio.Object,
            _mockMapper.Object,
            null
        ));
    }

    #endregion

    #region Testes ObterTodosPorUsuarioIdAsync

    [Fact]
    public async Task ObterTodosPorUsuarioIdAsync_ComUsuarioIdValido_DeveRetornarProjetos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var projetos = new List<Projeto> { new Projeto("Projeto Teste", "Descrição", usuarioId) };
        var projetosDto = new List<ProjetoDto> { new ProjetoDto { Nome = "Projeto Teste" } };

        _mockUnitOfWork.Setup(u => u.Projetos.ObterTodosPorUsuarioIdAsync(usuarioId)).ReturnsAsync(projetos);
        _mockMapper.Setup(m => m.Map<IEnumerable<ProjetoDto>>(projetos)).Returns(projetosDto);

        // Act
        var resultado = await _servicoProjeto.ObterTodosPorUsuarioIdAsync(usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal("Projeto Teste", resultado.First().Nome);
    }

    [Fact]
    public async Task ObterTodosPorUsuarioIdAsync_ComUsuarioIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var usuarioId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicoProjeto.ObterTodosPorUsuarioIdAsync(usuarioId));

        Assert.Equal("O ID do usuário não pode ser vazio. (Parameter 'usuarioId')", exception.Message);
    }

    [Fact]
    public async Task ObterTodosPorUsuarioIdAsync_ComProjetosNulos_DeveRetornarEnumerableVazio()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Projetos.ObterTodosPorUsuarioIdAsync(usuarioId))
            .ReturnsAsync((IEnumerable<Projeto>)null);

        // Act
        var resultado = await _servicoProjeto.ObterTodosPorUsuarioIdAsync(usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObterTodosPorUsuarioIdAsync_ComListaVazia_DeveRetornarEnumerableVazio()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var projetos = new List<Projeto>();
        _mockUnitOfWork.Setup(u => u.Projetos.ObterTodosPorUsuarioIdAsync(usuarioId)).ReturnsAsync(projetos);

        // Act
        var resultado = await _servicoProjeto.ObterTodosPorUsuarioIdAsync(usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    #endregion

    #region Testes ObterPorIdAsync

    [Fact]
    public async Task ObterPorIdAsync_ComIdValido_DeveRetornarProjeto()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var projeto = new Projeto("Projeto Teste", "Descrição", Guid.NewGuid());
        var projetoDto = new ProjetoDto { Nome = "Projeto Teste" };

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);
        _mockMapper.Setup(m => m.Map<ProjetoDto>(projeto)).Returns(projetoDto);

        // Act
        var resultado = await _servicoProjeto.ObterPorIdAsync(projetoId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Projeto Teste", resultado.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var projetoId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicoProjeto.ObterPorIdAsync(projetoId));

        Assert.Equal("O ID do projeto não pode ser vazio. (Parameter 'id')", exception.Message);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComProjetoNaoEncontrado_DeveRetornarNull()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync((Projeto)null);

        // Act
        var resultado = await _servicoProjeto.ObterPorIdAsync(projetoId);

        // Assert
        Assert.Null(resultado);
    }

    #endregion

    #region Testes CriarAsync

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveRetornarProjetoCriado()
    {
        // Arrange
        var criarProjetoDto = new CriarProjetoDto
        {
            Nome = "Novo Projeto",
            Descricao = "Descrição",
            UsuarioId = Guid.NewGuid()
        };
        var projeto = new Projeto(criarProjetoDto.Nome, criarProjetoDto.Descricao, criarProjetoDto.UsuarioId);
        var projetoDto = new ProjetoDto { Nome = "Novo Projeto" };

        _mockUnitOfWork.Setup(u => u.Projetos.CriarAsync(It.IsAny<Projeto>())).ReturnsAsync(projeto);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<ProjetoDto>(projeto)).Returns(projetoDto);

        // Act
        var resultado = await _servicoProjeto.CriarAsync(criarProjetoDto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Novo Projeto", resultado.Nome);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_ComDadosNulos_DeveLancarArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _servicoProjeto.CriarAsync(null));

        Assert.Equal("Os dados do projeto não podem ser nulos. (Parameter 'criarProjetoDto')", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_ComUsuarioIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var criarProjetoDto = new CriarProjetoDto
        {
            Nome = "Novo Projeto",
            Descricao = "Descrição",
            UsuarioId = Guid.Empty
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicoProjeto.CriarAsync(criarProjetoDto));

        Assert.Contains("O ID do usuário não pode ser vazio ou inválido", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_ComFalhaAoCriar_DeveLancarInvalidOperationException()
    {
        // Arrange
        var criarProjetoDto = new CriarProjetoDto
        {
            Nome = "Novo Projeto",
            Descricao = "Descrição",
            UsuarioId = Guid.NewGuid()
        };

        _mockUnitOfWork.Setup(u => u.Projetos.CriarAsync(It.IsAny<Projeto>())).ReturnsAsync((Projeto)null);
        _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicoProjeto.CriarAsync(criarProjetoDto));

        Assert.Equal("Não foi possível criar o projeto.", exception.Message);
        _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
    }

    #endregion

    #region Testes AtualizarAsync

    [Fact]
    public async Task AtualizarAsync_ComDadosValidos_DeveRetornarProjetoAtualizado()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var atualizarProjetoDto = new AtualizarProjetoDto
        {
            Nome = "Projeto Atualizado",
            Descricao = "Nova Descrição"
        };
        var projeto = new Projeto("Projeto Original", "Descrição Original", Guid.NewGuid());
        var projetoDto = new ProjetoDto { Nome = "Projeto Atualizado" };

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);
        _mockUnitOfWork.Setup(u => u.Projetos.AtualizarAsync(projeto)).ReturnsAsync(projeto);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<ProjetoDto>(projeto)).Returns(projetoDto);

        // Act
        var resultado = await _servicoProjeto.AtualizarAsync(projetoId, atualizarProjetoDto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Projeto Atualizado", resultado.Nome);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ComIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var projetoId = Guid.Empty;
        var atualizarProjetoDto = new AtualizarProjetoDto();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicoProjeto.AtualizarAsync(projetoId, atualizarProjetoDto));

        Assert.Equal("O ID do projeto não pode ser vazio. (Parameter 'id')", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_ComDadosNulos_DeveLancarArgumentNullException()
    {
        // Arrange
        var projetoId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _servicoProjeto.AtualizarAsync(projetoId, null));

        Assert.Equal("Os dados do projeto não podem ser nulos. (Parameter 'atualizarProjetoDto')", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_ComProjetoNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var atualizarProjetoDto = new AtualizarProjetoDto();

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync((Projeto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicoProjeto.AtualizarAsync(projetoId, atualizarProjetoDto));

        Assert.Equal($"Projeto com ID {projetoId} não encontrado.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_ComFalhaAoAtualizar_DeveLancarInvalidOperationException()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var atualizarProjetoDto = new AtualizarProjetoDto
        {
            Nome = "Projeto Atualizado",
            Descricao = "Nova Descrição"
        };
        var projeto = new Projeto("Projeto Original", "Descrição Original", Guid.NewGuid());

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);
        _mockUnitOfWork.Setup(u => u.Projetos.AtualizarAsync(projeto)).ReturnsAsync((Projeto)null);
        _mockUnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicoProjeto.AtualizarAsync(projetoId, atualizarProjetoDto));

        Assert.Equal($"Não foi possível atualizar o projeto com ID {projetoId}.", exception.Message);
        _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
    }

    #endregion

    #region Testes ExcluirAsync

    [Fact]
    public async Task ExcluirAsync_ComIdValido_DeveRetornarTrue()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var projeto = new Projeto("Projeto Teste", "Descrição", Guid.NewGuid());

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);
        _mockProjetoServicoDominio.Setup(s => s.PodeRemoverProjetoAsync(projetoId)).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.Projetos.ExcluirAsync(projetoId)).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _servicoProjeto.ExcluirAsync(projetoId);

        // Assert
        Assert.True(resultado);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_ComIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var projetoId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicoProjeto.ExcluirAsync(projetoId));

        Assert.Equal("O ID do projeto não pode ser vazio. (Parameter 'id')", exception.Message);
    }

    [Fact]
    public async Task ExcluirAsync_ComProjetoNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync((Projeto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicoProjeto.ExcluirAsync(projetoId));

        Assert.Equal($"Projeto com ID {projetoId} não encontrado para exclusão.", exception.Message);
    }

    [Fact]
    public async Task ExcluirAsync_ComProjetoComTarefasPendentes_DeveLancarExcecaoDominio()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var projeto = new Projeto("Projeto Teste", "Descrição", Guid.NewGuid());

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);
        _mockProjetoServicoDominio.Setup(s => s.PodeRemoverProjetoAsync(projetoId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExcecaoDominio>(() =>
            _servicoProjeto.ExcluirAsync(projetoId));

        Assert.Equal("Não é possível excluir o projeto porque ele possui tarefas pendentes. Conclua ou remova todas as tarefas primeiro.", exception.Message);
    }

    [Fact]
    public async Task ExcluirAsync_ComFalhaAoExcluir_DeveLancarInvalidOperationException()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var projeto = new Projeto("Projeto Teste", "Descrição", Guid.NewGuid());

        _mockUnitOfWork.Setup(u => u.Projetos.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);
        _mockProjetoServicoDominio.Setup(s => s.PodeRemoverProjetoAsync(projetoId)).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.Projetos.ExcluirAsync(projetoId)).ReturnsAsync(false);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicoProjeto.ExcluirAsync(projetoId));

        Assert.Equal($"Não foi possível excluir o projeto com ID {projetoId}. Ele pode já estar excluído ou não existir.", exception.Message);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    #endregion
}