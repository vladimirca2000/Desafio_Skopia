using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Skopia.Domain.Entidades;
using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;
using Skopia.Domain.Interfaces.UnitOfWork;
using Skopia.Domain.Repositorios.Interfaces;
using Skopia.Services.Modelos;
using Skopia.Services.Servicos;
using Xunit;

namespace Skopia.Test.Servicos;

public class ServicoTarefaTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ServicoTarefa>> _mockLogger;
    private readonly Mock<IRepositorioTarefa> _mockRepositorioTarefa;
    private readonly Mock<IRepositorioProjeto> _mockRepositorioProjeto;
    private readonly Mock<IRepositorioUsuario> _mockRepositorioUsuario;
    private readonly Mock<IRepositorioComentarioTarefa> _mockRepositorioComentario;
    private readonly Mock<IRepositorioHistoricoAlteracaoTarefa> _mockRepositorioHistorico;
    private readonly ServicoTarefa _servico;

    public ServicoTarefaTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ServicoTarefa>>();
        _mockRepositorioTarefa = new Mock<IRepositorioTarefa>();
        _mockRepositorioProjeto = new Mock<IRepositorioProjeto>();
        _mockRepositorioUsuario = new Mock<IRepositorioUsuario>();
        _mockRepositorioComentario = new Mock<IRepositorioComentarioTarefa>();
        _mockRepositorioHistorico = new Mock<IRepositorioHistoricoAlteracaoTarefa>();

        _mockUnitOfWork.Setup(x => x.Tarefas).Returns(_mockRepositorioTarefa.Object);
        _mockUnitOfWork.Setup(x => x.Projetos).Returns(_mockRepositorioProjeto.Object);
        _mockUnitOfWork.Setup(x => x.Usuarios).Returns(_mockRepositorioUsuario.Object);
        _mockUnitOfWork.Setup(x => x.ComentariosTarefas).Returns(_mockRepositorioComentario.Object);
        _mockUnitOfWork.Setup(x => x.HistoricosAlteracaoTarefa).Returns(_mockRepositorioHistorico.Object);

        _servico = new ServicoTarefa(_mockUnitOfWork.Object, _mockMapper.Object, _mockLogger.Object);
    }

    #region Testes do Construtor

    [Fact]
    public void Construtor_ComParametrosValidos_DeveInicializarCorretamente()
    {
        // Act & Assert
        Assert.NotNull(_servico);
    }

    [Fact]
    public void Construtor_ComUnitOfWorkNulo_DeveLancarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ServicoTarefa(null, _mockMapper.Object, _mockLogger.Object));
    }

    [Fact]
    public void Construtor_ComMapperNulo_DeveLancarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ServicoTarefa(_mockUnitOfWork.Object, null, _mockLogger.Object));
    }

    [Fact]
    public void Construtor_ComLoggerNulo_DeveLancarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ServicoTarefa(_mockUnitOfWork.Object, _mockMapper.Object, null));
    }

    #endregion

    #region Testes ObterTodosPorProjetoIdAsync

    

    [Fact]
    public async Task ObterTodosPorProjetoIdAsync_ComProjetoIdVazio_DeveLancarArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _servico.ObterTodosPorProjetoIdAsync(Guid.Empty));

        Assert.Contains("O ID do projeto não pode ser vazio", exception.Message);
    }

    [Fact]
    public async Task ObterTodosPorProjetoIdAsync_ComProjetoInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        _mockRepositorioProjeto.Setup(x => x.ObterPorIdAsync(projetoId))
            .ReturnsAsync((Projeto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _servico.ObterTodosPorProjetoIdAsync(projetoId));

        Assert.Contains($"Projeto com ID {projetoId} não encontrado", exception.Message);
    }

    #endregion

    #region Testes ObterPorIdAsync

    [Fact]
    public async Task ObterPorIdAsync_ComIdValido_DeveRetornarTarefa()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();
        var tarefa = new Tarefa(Guid.NewGuid(), Guid.NewGuid(), "Tarefa Teste", "Descrição", PrioridadeTarefa.Media, DateTime.UtcNow.AddDays(7));
        var tarefaDto = new TarefaDto { Id = tarefaId, Titulo = "Tarefa Teste" };

        _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
            .ReturnsAsync(tarefa);
        _mockMapper.Setup(x => x.Map<TarefaDto>(tarefa))
            .Returns(tarefaDto);

        // Act
        var resultado = await _servico.ObterPorIdAsync(tarefaId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Tarefa Teste", resultado.Titulo);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdVazio_DeveLancarArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _servico.ObterPorIdAsync(Guid.Empty));

        Assert.Contains("O ID da tarefa não pode ser vazio", exception.Message);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComTarefaInexistente_DeveRetornarNull()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();
        _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
            .ReturnsAsync((Tarefa)null);

        // Act
        var resultado = await _servico.ObterPorIdAsync(tarefaId);

        // Assert
        Assert.Null(resultado);
    }

    #endregion

    #region Testes CriarAsync

    

    [Fact]
    public async Task CriarAsync_ComProjetoInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var criarTarefaDto = new CriarTarefaDto
        {
            ProjetoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Titulo = "Nova Tarefa",
            Descricao = "Descrição",
            Prioridade = "media",
            DataVencimento = DateTime.UtcNow.AddDays(7)
        };

        _mockRepositorioProjeto.Setup(x => x.ObterPorIdAsync(criarTarefaDto.ProjetoId))
            .ReturnsAsync((Projeto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _servico.CriarAsync(criarTarefaDto));

        Assert.Contains($"Projeto com ID {criarTarefaDto.ProjetoId} não encontrado", exception.Message);
    }

    #endregion

    #region Testes AtualizarAsync

    

    [Fact]
    public async Task AtualizarAsync_ComIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var atualizarDto = new AtualizarTarefaDto();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _servico.AtualizarAsync(Guid.Empty, atualizarDto));

        Assert.Contains("O ID da tarefa não pode ser vazio", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_ComTarefaInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();
        var atualizarDto = new AtualizarTarefaDto();

        _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
            .ReturnsAsync((Tarefa)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _servico.AtualizarAsync(tarefaId, atualizarDto));

        Assert.Contains($"Tarefa com ID {tarefaId} não encontrada", exception.Message);
    }

    #endregion

    #region Testes ExcluirAsync

    

    [Fact]
    public async Task ExcluirAsync_ComTarefaInexistente_DeveRetornarFalse()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();
        _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
            .ReturnsAsync((Tarefa)null);

        // Act
        var resultado = await _servico.ExcluirAsync(tarefaId);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task ExcluirAsync_ComTarefaConcluida_DeveLancarExcecaoDominio()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();
        var tarefa = new Tarefa(Guid.NewGuid(), Guid.NewGuid(), "Tarefa", "Descrição", PrioridadeTarefa.Media, DateTime.UtcNow.AddDays(7));

        // Simular tarefa concluída
        tarefa.GetType().GetProperty("Status")?.SetValue(tarefa, StatusTarefa.Concluida);

        _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
            .ReturnsAsync(tarefa);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExcecaoDominio>(
            () => _servico.ExcluirAsync(tarefaId));

        Assert.Contains("Não é possível excluir uma tarefa que já foi concluída", exception.Message);
    }

    #endregion

    #region Testes AdicionarComentarioAsync

    

    [Fact]
    public async Task AdicionarComentarioAsync_ComTarefaInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var criarComentarioDto = new CriarComentarioTarefaDto
        {
            TarefaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Conteudo = "Comentário"
        };

        _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(criarComentarioDto.TarefaId))
            .ReturnsAsync((Tarefa)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _servico.AdicionarComentarioAsync(criarComentarioDto));

        Assert.Contains($"Tarefa com ID {criarComentarioDto.TarefaId} não encontrada", exception.Message);
    }

    #endregion

    #region Testes ObterRelatorioDesempenhoUsuarioAsync

    

    [Fact]
    public async Task ObterRelatorioDesempenhoUsuarioAsync_ComUsuarioInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _mockRepositorioUsuario.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync((Usuario)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _servico.ObterRelatorioDesempenhoUsuarioAsync(usuarioId));

        Assert.Contains($"Usuário com ID {usuarioId} não encontrado", exception.Message);
    }

    [Fact]
    public async Task ObterRelatorioDesempenhoUsuarioAsync_ComUsuarioNaoGerente_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario(usuarioId, "Usuário", "usuario@teste.com", FuncaoUsuario.Regular);

        _mockRepositorioUsuario.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);
        _mockRepositorioUsuario.Setup(x => x.VerificarSeGerenteAsync(usuarioId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _servico.ObterRelatorioDesempenhoUsuarioAsync(usuarioId));

        Assert.Contains("Somente gerentes podem acessar relatórios de desempenho", exception.Message);
    }

    #endregion

    #region Testes dos Métodos Privados (através de reflexão ou métodos públicos que os utilizam)

    

    #endregion
}