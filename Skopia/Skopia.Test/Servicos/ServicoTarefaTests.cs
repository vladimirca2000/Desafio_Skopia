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

    //[Fact]
    //public async Task ObterTodosPorProjetoIdAsync_ComProjetoIdValido_DeveRetornarTarefas()
    //{
    //    // Arrange
    //    var projetoId = Guid.NewGuid();
    //    var projeto = new Projeto("Projeto Teste", "Descrição", Guid.NewGuid());
    //    var tarefas = new List<Tarefa>
    //    {
    //        new Tarefa(projetoId, Guid.NewGuid(), "Tarefa 1", "Descrição 1", PrioridadeTarefa.Alta, DateTime.UtcNow.AddDays(7))
    //    };
    //    projeto.GetType().GetProperty("Tarefas")?.SetValue(projeto, tarefas);

    //    var tarefasDto = new List<TarefaDto>
    //    {
    //        new TarefaDto { Id = tarefas[0].Id, Titulo = "Tarefa 1" }
    //    };

    //    _mockRepositorioProjeto.Setup(x => x.ObterPorIdAsync(projetoId))
    //        .ReturnsAsync(projeto);
    //    _mockMapper.Setup(x => x.Map<IEnumerable<TarefaDto>>(It.IsAny<IEnumerable<Tarefa>>()))
    //        .Returns(tarefasDto);

    //    // Act
    //    var resultado = await _servico.ObterTodosPorProjetoIdAsync(projetoId);

    //    // Assert
    //    Assert.NotNull(resultado);
    //    Assert.Single(resultado);
    //    Assert.Equal("Tarefa 1", resultado.First().Titulo);
    //}

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

    //[Fact]
    //public async Task CriarAsync_ComDadosValidos_DeveCriarTarefa()
    //{
    //    // Arrange
    //    var projetoId = Guid.NewGuid();
    //    var usuarioId = Guid.NewGuid();
    //    var projeto = new Projeto("Projeto Teste", "Descrição", Guid.NewGuid());

    //    var criarTarefaDto = new CriarTarefaDto
    //    {
    //        ProjetoId = projetoId,
    //        UsuarioId = usuarioId,
    //        Titulo = "Nova Tarefa",
    //        Descricao = "Descrição da tarefa",
    //        Prioridade = "alta",
    //        DataVencimento = DateTime.UtcNow.AddDays(7)
    //    };

    //    var tarefaDto = new TarefaDto { Titulo = "Nova Tarefa" };

    //    _mockRepositorioProjeto.Setup(x => x.ObterPorIdAsync(projetoId))
    //        .ReturnsAsync(projeto);
    //    _mockRepositorioTarefa.Setup(x => x.CriarAsync(It.IsAny<Tarefa>()))
    //        .Returns((Task<Tarefa>)Task.CompletedTask);
    //    _mockUnitOfWork.Setup(x => x.CommitAsync())
    //        .Returns((Task<int>)Task.CompletedTask);
    //    _mockMapper.Setup(x => x.Map<TarefaDto>(It.IsAny<Tarefa>()))
    //        .Returns(tarefaDto);

    //    // Act
    //    var resultado = await _servico.CriarAsync(criarTarefaDto);

    //    // Assert
    //    Assert.NotNull(resultado);
    //    Assert.Equal("Nova Tarefa", resultado.Titulo);
    //    _mockRepositorioTarefa.Verify(x => x.CriarAsync(It.IsAny<Tarefa>()), Times.Once);
    //    _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    //}

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

    //[Fact]
    //public async Task AtualizarAsync_ComDadosValidos_DeveAtualizarTarefa()
    //{
    //    // Arrange
    //    var tarefaId = Guid.NewGuid();
    //    var tarefa = new Tarefa(Guid.NewGuid(), Guid.NewGuid(), "Tarefa Original", "Descrição Original", PrioridadeTarefa.Media, DateTime.UtcNow.AddDays(7));

    //    var atualizarDto = new AtualizarTarefaDto
    //    {
    //        Titulo = "Tarefa Atualizada",
    //        Descricao = "Descrição Atualizada",
    //        Status = "emandamento",
    //        UsuarioExecutorId = Guid.NewGuid(),
    //        DataConclusao = DateTime.UtcNow
    //    };

    //    var tarefaDto = new TarefaDto { Titulo = "Tarefa Atualizada" };

    //    _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
    //        .ReturnsAsync(tarefa);
    //    _mockRepositorioTarefa.Setup(x => x.AtualizarAsync(It.IsAny<Tarefa>()))
    //        .Returns((Task<Tarefa>)Task.CompletedTask);
    //    _mockUnitOfWork.Setup(x => x.CommitAsync())
    //        .Returns((Task<int>)Task.CompletedTask);
    //    _mockMapper.Setup(x => x.Map<TarefaDto>(It.IsAny<Tarefa>()))
    //        .Returns(tarefaDto);

    //    // Act
    //    var resultado = await _servico.AtualizarAsync(tarefaId, atualizarDto);

    //    // Assert
    //    Assert.NotNull(resultado);
    //    _mockRepositorioTarefa.Verify(x => x.AtualizarAsync(It.IsAny<Tarefa>()), Times.Once);
    //    _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    //}

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

    //[Fact]
    //public async Task ExcluirAsync_ComTarefaValida_DeveExcluirComSucesso()
    //{
    //    // Arrange
    //    var tarefaId = Guid.NewGuid();
    //    var tarefa = new Tarefa(Guid.NewGuid(), Guid.NewGuid(), "Tarefa", "Descrição", PrioridadeTarefa.Media, DateTime.UtcNow.AddDays(7));

    //    _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
    //        .ReturnsAsync(tarefa);
    //    _mockRepositorioTarefa.Setup(x => x.ExcluirAsync(tarefaId))
    //        .ReturnsAsync(true);
    //    _mockUnitOfWork.Setup(x => x.CommitAsync())
    //        .Returns((Task<int>)Task.CompletedTask);

    //    // Act
    //    var resultado = await _servico.ExcluirAsync(tarefaId);

    //    // Assert
    //    Assert.True(resultado);
    //    _mockRepositorioTarefa.Verify(x => x.ExcluirAsync(tarefaId), Times.Once);
    //    _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    //}

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

    //[Fact]
    //public async Task AdicionarComentarioAsync_ComDadosValidos_DeveAdicionarComentario()
    //{
    //    // Arrange
    //    var tarefaId = Guid.NewGuid();
    //    var tarefa = new Tarefa(Guid.NewGuid(), Guid.NewGuid(), "Tarefa", "Descrição", PrioridadeTarefa.Media, DateTime.UtcNow.AddDays(7));

    //    var criarComentarioDto = new CriarComentarioTarefaDto
    //    {
    //        TarefaId = tarefaId,
    //        UsuarioId = Guid.NewGuid(),
    //        Conteudo = "Comentário teste"
    //    };

    //    var tarefaDto = new TarefaDto { Id = tarefaId };

    //    _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
    //        .ReturnsAsync(tarefa);
    //    _mockRepositorioComentario.Setup(x => x.CriarAsync(It.IsAny<ComentarioTarefa>()))
    //        .Returns((Task<ComentarioTarefa>)Task.CompletedTask);
    //    _mockRepositorioHistorico.Setup(x => x.CriarAsync(It.IsAny<HistoricoAlteracaoTarefa>()))
    //        .Returns((Task<HistoricoAlteracaoTarefa>)Task.CompletedTask);
    //    _mockUnitOfWork.Setup(x => x.CommitAsync())
    //        .Returns(Task.CompletedTask);
    //    _mockMapper.Setup(x => x.Map<TarefaDto>(It.IsAny<Tarefa>()))
    //        .Returns(tarefaDto);

    //    // Act
    //    var resultado = await _servico.AdicionarComentarioAsync(criarComentarioDto);

    //    // Assert
    //    Assert.NotNull(resultado);
    //    _mockRepositorioComentario.Verify(x => x.CriarAsync(It.IsAny<ComentarioTarefa>()), Times.Once);
    //    _mockRepositorioHistorico.Verify(x => x.CriarAsync(It.IsAny<HistoricoAlteracaoTarefa>()), Times.Once);
    //    _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    //}

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

    //[Fact]
    //public async Task ObterRelatorioDesempenhoUsuarioAsync_ComUsuarioGerente_DeveRetornarRelatorio()
    //{
    //    // Arrange
    //    var usuarioId = Guid.NewGuid();
    //    var usuario = new Usuario("Gerente", "gerente@teste.com");

    //    var contagemTarefas = new List<dynamic>
    //    {
    //        new { Usuario = new Usuario("João", "joao@teste.com") { Id = Guid.NewGuid() }, Contagem = 10 },
    //        new { Usuario = new Usuario("Maria", "maria@teste.com") { Id = Guid.NewGuid() }, Contagem = 15 }
    //    };

    //    _mockRepositorioUsuario.Setup(x => x.ObterPorIdAsync(usuarioId))
    //        .ReturnsAsync(usuario);
    //    _mockRepositorioUsuario.Setup(x => x.VerificarSeGerenteAsync(usuarioId))
    //        .ReturnsAsync(true);
    //    _mockRepositorioTarefa.Setup(x => x.ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(It.IsAny<DateTime>()))
    //        .ReturnsAsync(contagemTarefas);

    //    // Act
    //    var resultado = await _servico.ObterRelatorioDesempenhoUsuarioAsync(usuarioId);

    //    // Assert
    //    Assert.NotNull(resultado);
    //    Assert.Equal(2, resultado.Count());
    //}

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

    //[Theory]
    //[InlineData("baixa", PrioridadeTarefa.Baixa)]
    //[InlineData("media", PrioridadeTarefa.Media)]
    //[InlineData("alta", PrioridadeTarefa.Alta)]
    //[InlineData("invalida", PrioridadeTarefa.Media)]
    //public async Task CriarAsync_ComDiferentesPrioridades_DeveConverterCorretamente(string prioridadeString, PrioridadeTarefa prioridadeEsperada)
    //{
    //    // Arrange
    //    var projetoId = Guid.NewGuid();
    //    var projeto = new Projeto("Projeto", "Descrição", Guid.NewGuid());

    //    var criarTarefaDto = new CriarTarefaDto
    //    {
    //        ProjetoId = projetoId,
    //        UsuarioId = Guid.NewGuid(),
    //        Titulo = "Tarefa",
    //        Descricao = "Descrição",
    //        Prioridade = prioridadeString,
    //        DataVencimento = DateTime.UtcNow.AddDays(7)
    //    };

    //    _mockRepositorioProjeto.Setup(x => x.ObterPorIdAsync(projetoId))
    //        .ReturnsAsync(projeto);
    //    _mockMapper.Setup(x => x.Map<TarefaDto>(It.IsAny<Tarefa>()))
    //        .Returns(new TarefaDto());

    //    // Act
    //    await _servico.CriarAsync(criarTarefaDto);

    //    // Assert
    //    _mockRepositorioTarefa.Verify(x => x.CriarAsync(It.Is<Tarefa>(t => t.Prioridade == prioridadeEsperada)), Times.Once);
    //}

    //[Theory]
    //[InlineData("pendente")]
    //[InlineData("emandamento")]
    //[InlineData("concluida")]
    //[InlineData("cancelada")]
    //[InlineData("invalido")]
    //public async Task AtualizarAsync_ComDiferentesStatus_DeveConverterCorretamente(string statusString)
    //{
    //    // Arrange
    //    var tarefaId = Guid.NewGuid();
    //    var tarefa = new Tarefa(Guid.NewGuid(), Guid.NewGuid(), "Tarefa", "Descrição", PrioridadeTarefa.Media, DateTime.UtcNow.AddDays(7));

    //    var atualizarDto = new AtualizarTarefaDto
    //    {
    //        Status = statusString,
    //        UsuarioExecutorId = Guid.NewGuid()
    //    };

    //    _mockRepositorioTarefa.Setup(x => x.ObterPorIdAsync(tarefaId))
    //        .ReturnsAsync(tarefa);
    //    _mockMapper.Setup(x => x.Map<TarefaDto>(It.IsAny<Tarefa>()))
    //        .Returns(new TarefaDto());

    //    // Act
    //    var resultado = await _servico.AtualizarAsync(tarefaId, atualizarDto);

    //    // Assert
    //    Assert.NotNull(resultado);
    //    _mockRepositorioTarefa.Verify(x => x.AtualizarAsync(It.IsAny<Tarefa>()), Times.Once);
    //}

    #endregion
}