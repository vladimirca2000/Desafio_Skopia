using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Skopia.API.Controllers;
using Skopia.Domain.Excecoes;
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Skopia.Test.Controller
{
    public class TarefasControllerTests
    {
        private readonly Mock<IServicoTarefa> _mockServicoTarefa;
        private readonly Mock<ILogger<TarefasController>> _mockLogger;
        private readonly TarefasController _controller;

        public TarefasControllerTests()
        {
            _mockServicoTarefa = new Mock<IServicoTarefa>();
            _mockLogger = new Mock<ILogger<TarefasController>>();
            _controller = new TarefasController(_mockServicoTarefa.Object, _mockLogger.Object);
        }

        #region ObterTodosPorProjetoId Tests

        [Fact]
        public async Task ObterTodosPorProjetoId_DeveRetornarOkComTarefas()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var tarefas = new List<TarefaDto>
            {
                new TarefaDto { Id = Guid.NewGuid(), Titulo = "Tarefa 1" },
                new TarefaDto { Id = Guid.NewGuid(), Titulo = "Tarefa 2" }
            };
            _mockServicoTarefa.Setup(s => s.ObterTodosPorProjetoIdAsync(projetoId)).ReturnsAsync(tarefas);

            // Act
            var resultado = await _controller.ObterTodosPorProjetoId(projetoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(tarefas, okResult.Value);
        }

        [Fact]
        public async Task ObterTodosPorProjetoId_DeveRetornarBadRequestParaProjetoIdVazio()
        {
            // Arrange
            var projetoId = Guid.Empty;

            // Act
            var resultado = await _controller.ObterTodosPorProjetoId(projetoId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID do projeto não pode ser vazio.", badRequestResult.Value);
        }

        [Fact]
        public async Task ObterTodosPorProjetoId_DeveRetornarNotFoundQuandoProjetoNaoExiste()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ObterTodosPorProjetoIdAsync(projetoId))
                .ThrowsAsync(new KeyNotFoundException("Projeto não encontrado"));

            // Act
            var resultado = await _controller.ObterTodosPorProjetoId(projetoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal("Projeto não encontrado", notFoundResult.Value);
        }

        [Fact]
        public async Task ObterTodosPorProjetoId_DeveRetornarInternalServerErrorParaExcecaoGenerica()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ObterTodosPorProjetoIdAsync(projetoId))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.ObterTodosPorProjetoId(projetoId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ObterPorId Tests

        [Fact]
        public async Task ObterPorId_DeveRetornarOkComTarefa()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            var tarefa = new TarefaDto { Id = tarefaId, Titulo = "Tarefa Teste" };
            _mockServicoTarefa.Setup(s => s.ObterPorIdAsync(tarefaId)).ReturnsAsync(tarefa);

            // Act
            var resultado = await _controller.ObterPorId(tarefaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(tarefa, okResult.Value);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarBadRequestParaIdVazio()
        {
            // Arrange
            var tarefaId = Guid.Empty;

            // Act
            var resultado = await _controller.ObterPorId(tarefaId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID da tarefa não pode ser vazio.", badRequestResult.Value);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFoundParaTarefaNaoEncontrada()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ObterPorIdAsync(tarefaId))
                .ThrowsAsync(new Exception("Tarefa não encontrada"));

            // Act
            var resultado = await _controller.ObterPorId(tarefaId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal($"Tarefa com ID {tarefaId} não encontrada.", notFoundResult.Value);
        }

        #endregion

        #region CriarTarefa Tests

        [Fact]
        public async Task CriarTarefa_DeveRetornarCreatedComTarefa()
        {
            // Arrange
            var criarTarefaDto = new CriarTarefaDto
            {
                Titulo = "Nova Tarefa",
                ProjetoId = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid()
            };
            var tarefaCriada = new TarefaDto
            {
                Id = Guid.NewGuid(),
                Titulo = criarTarefaDto.Titulo,
                ProjetoId = criarTarefaDto.ProjetoId
            };
            _mockServicoTarefa.Setup(s => s.CriarAsync(criarTarefaDto)).ReturnsAsync(tarefaCriada);

            // Act
            var resultado = await _controller.CriarTarefa(criarTarefaDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(tarefaCriada, createdResult.Value);
            Assert.Equal(nameof(_controller.ObterPorId), createdResult.ActionName);
        }

        [Fact]
        public async Task CriarTarefa_DeveRetornarBadRequestParaModeloInvalido()
        {
            // Arrange
            _controller.ModelState.AddModelError("Titulo", "O título é obrigatório");
            var criarTarefaDto = new CriarTarefaDto();

            // Act
            var resultado = await _controller.CriarTarefa(criarTarefaDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CriarTarefa_DeveRetornarNotFoundQuandoProjetoNaoExiste()
        {
            // Arrange
            var criarTarefaDto = new CriarTarefaDto
            {
                Titulo = "Nova Tarefa",
                ProjetoId = Guid.NewGuid()
            };
            _mockServicoTarefa.Setup(s => s.CriarAsync(criarTarefaDto))
                .ThrowsAsync(new KeyNotFoundException("Projeto não encontrado"));

            // Act
            var resultado = await _controller.CriarTarefa(criarTarefaDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal("Projeto não encontrado", notFoundResult.Value);
        }

        [Fact]
        public async Task CriarTarefa_DeveRetornarBadRequestParaInvalidOperationException()
        {
            // Arrange
            var criarTarefaDto = new CriarTarefaDto
            {
                Titulo = "Nova Tarefa",
                ProjetoId = Guid.NewGuid()
            };
            _mockServicoTarefa.Setup(s => s.CriarAsync(criarTarefaDto))
                .ThrowsAsync(new InvalidOperationException("Regra de negócio violada"));

            // Act
            var resultado = await _controller.CriarTarefa(criarTarefaDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CriarTarefa_DeveRetornarUnprocessableEntityParaExcecaoDominio()
        {
            // Arrange
            var criarTarefaDto = new CriarTarefaDto
            {
                Titulo = "Nova Tarefa",
                ProjetoId = Guid.NewGuid()
            };
            _mockServicoTarefa.Setup(s => s.CriarAsync(criarTarefaDto))
                .ThrowsAsync(new ExcecaoDominio("Erro de domínio"));

            // Act
            var resultado = await _controller.CriarTarefa(criarTarefaDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CriarTarefa_DeveRetornarInternalServerErrorParaExcecaoGenerica()
        {
            // Arrange
            var criarTarefaDto = new CriarTarefaDto
            {
                Titulo = "Nova Tarefa",
                ProjetoId = Guid.NewGuid()
            };
            _mockServicoTarefa.Setup(s => s.CriarAsync(criarTarefaDto))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.CriarTarefa(criarTarefaDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region Atualizar Tests

        [Fact]
        public async Task Atualizar_DeveRetornarOkComTarefaAtualizada()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            var atualizarTarefaDto = new AtualizarTarefaDto
            {
                Titulo = "Tarefa Atualizada",
                Status = "Concluida",
                UsuarioExecutorId = Guid.NewGuid()
            };
            var tarefaAtualizada = new TarefaDto
            {
                Id = tarefaId,
                Titulo = atualizarTarefaDto.Titulo,
                Status = atualizarTarefaDto.Status
            };
            _mockServicoTarefa.Setup(s => s.AtualizarAsync(tarefaId, atualizarTarefaDto))
                .ReturnsAsync(tarefaAtualizada);

            // Act
            var resultado = await _controller.Atualizar(tarefaId, atualizarTarefaDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(tarefaAtualizada, okResult.Value);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequestParaIdVazio()
        {
            // Arrange
            var tarefaId = Guid.Empty;
            var atualizarTarefaDto = new AtualizarTarefaDto();

            // Act
            var resultado = await _controller.Atualizar(tarefaId, atualizarTarefaDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID da tarefa não pode ser vazio.", badRequestResult.Value);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequestParaModeloInvalido()
        {
            // Arrange
            _controller.ModelState.AddModelError("Titulo", "O título é obrigatório");
            var tarefaId = Guid.NewGuid();
            var atualizarTarefaDto = new AtualizarTarefaDto();

            // Act
            var resultado = await _controller.Atualizar(tarefaId, atualizarTarefaDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNotFoundQuandoTarefaNaoExiste()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            var atualizarTarefaDto = new AtualizarTarefaDto { Titulo = "Teste" };
            _mockServicoTarefa.Setup(s => s.AtualizarAsync(tarefaId, atualizarTarefaDto))
                .ThrowsAsync(new KeyNotFoundException("Tarefa não encontrada"));

            // Act
            var resultado = await _controller.Atualizar(tarefaId, atualizarTarefaDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal("Tarefa não encontrada", notFoundResult.Value);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequestParaInvalidOperationException()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            var atualizarTarefaDto = new AtualizarTarefaDto { Titulo = "Teste" };
            _mockServicoTarefa.Setup(s => s.AtualizarAsync(tarefaId, atualizarTarefaDto))
                .ThrowsAsync(new InvalidOperationException("Regra de negócio violada"));

            // Act
            var resultado = await _controller.Atualizar(tarefaId, atualizarTarefaDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarInternalServerErrorParaExcecaoGenerica()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            var atualizarTarefaDto = new AtualizarTarefaDto { Titulo = "Teste" };
            _mockServicoTarefa.Setup(s => s.AtualizarAsync(tarefaId, atualizarTarefaDto))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.Atualizar(tarefaId, atualizarTarefaDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region Excluir Tests

        [Fact]
        public async Task Excluir_DeveRetornarNoContent()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ExcluirAsync(tarefaId)).ReturnsAsync(true);

            // Act
            var resultado = await _controller.Excluir(tarefaId);

            // Assert
            Assert.IsType<NoContentResult>(resultado);
        }

        [Fact]
        public async Task Excluir_DeveRetornarBadRequestParaIdVazio()
        {
            // Arrange
            var tarefaId = Guid.Empty;

            // Act
            var resultado = await _controller.Excluir(tarefaId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal("O ID da tarefa não pode ser vazio.", badRequestResult.Value);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFoundParaTarefaNaoEncontrada()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ExcluirAsync(tarefaId)).ReturnsAsync(false);

            // Act
            var resultado = await _controller.Excluir(tarefaId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
            Assert.Equal($"Tarefa com ID {tarefaId} não encontrada.", notFoundResult.Value);
        }

        [Fact]
        public async Task Excluir_DeveRetornarUnprocessableEntityParaExcecaoDominio()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ExcluirAsync(tarefaId))
                .ThrowsAsync(new ExcecaoDominio("Erro de domínio"));

            // Act
            var resultado = await _controller.Excluir(tarefaId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Excluir_DeveRetornarInternalServerErrorParaExcecaoGenerica()
        {
            // Arrange
            var tarefaId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ExcluirAsync(tarefaId))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.Excluir(tarefaId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region AdicionarComentario Tests

        [Fact]
        public async Task AdicionarComentario_DeveRetornarOkComTarefa()
        {
            // Arrange
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.NewGuid(),
                Conteudo = "Novo comentário",
                UsuarioId = Guid.NewGuid()
            };
            var tarefaComComentario = new TarefaDto
            {
                Id = criarComentarioDto.TarefaId,
                Titulo = "Tarefa com comentário"
            };
            _mockServicoTarefa.Setup(s => s.AdicionarComentarioAsync(criarComentarioDto))
                .ReturnsAsync(tarefaComComentario);

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(tarefaComComentario, okResult.Value);
        }

        [Fact]
        public async Task AdicionarComentario_DeveRetornarBadRequestParaModeloInvalido()
        {
            // Arrange
            _controller.ModelState.AddModelError("Conteudo", "O conteúdo do comentário é obrigatório");
            var criarComentarioDto = new CriarComentarioTarefaDto();

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task AdicionarComentario_DeveRetornarNotFoundQuandoTarefaNaoExiste()
        {
            // Arrange
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.NewGuid(),
                Conteudo = "Comentário",
                UsuarioId = Guid.NewGuid()
            };
            _mockServicoTarefa.Setup(s => s.AdicionarComentarioAsync(criarComentarioDto))
                .ThrowsAsync(new KeyNotFoundException("Tarefa não encontrada"));

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal("Tarefa não encontrada", notFoundResult.Value);
        }

        [Fact]
        public async Task AdicionarComentario_DeveRetornarInternalServerErrorParaExcecaoGenerica()
        {
            // Arrange
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.NewGuid(),
                Conteudo = "Comentário",
                UsuarioId = Guid.NewGuid()
            };
            _mockServicoTarefa.Setup(s => s.AdicionarComentarioAsync(criarComentarioDto))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ObterRelatorioDesempenho Tests

        [Fact]
        public async Task ObterRelatorioDesempenho_DeveRetornarOkComRelatorio()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var relatorios = new List<RelatorioDesempenhoDto>
    {
        new RelatorioDesempenhoDto
        {
            UsuarioId = usuarioId,
            NomeUsuario = "João Silva",
            ContagemTarefasConcluidas = 10,
            MediaTarefasConcluidasPorDia = 2.5
        },
        new RelatorioDesempenhoDto
        {
            UsuarioId = usuarioId,
            NomeUsuario = "João Silva",
            ContagemTarefasConcluidas = 15,
            MediaTarefasConcluidasPorDia = 3.0
        }
    };
            _mockServicoTarefa.Setup(s => s.ObterRelatorioDesempenhoUsuarioAsync(usuarioId))
                .ReturnsAsync(relatorios);

            // Act
            var resultado = await _controller.ObterRelatorioDesempenho(usuarioId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var relatoriosRetornados = Assert.IsType<List<RelatorioDesempenhoDto>>(okResult.Value);

            Assert.Equal(2, relatoriosRetornados.Count);
            Assert.Equal(relatorios[0].UsuarioId, relatoriosRetornados[0].UsuarioId);
            Assert.Equal(relatorios[0].NomeUsuario, relatoriosRetornados[0].NomeUsuario);
            Assert.Equal(relatorios[0].ContagemTarefasConcluidas, relatoriosRetornados[0].ContagemTarefasConcluidas);
            Assert.Equal(relatorios[0].MediaTarefasConcluidasPorDia, relatoriosRetornados[0].MediaTarefasConcluidasPorDia);
        }

        [Fact]
        public async Task ObterRelatorioDesempenho_DeveRetornarBadRequestParaUsuarioIdVazio()
        {
            // Arrange
            var usuarioId = Guid.Empty;

            // Act
            var resultado = await _controller.ObterRelatorioDesempenho(usuarioId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID do usuário não pode ser vazio.", badRequestResult.Value);
        }

        [Fact]
        public async Task ObterRelatorioDesempenho_DeveRetornarNotFoundQuandoUsuarioNaoExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ObterRelatorioDesempenhoUsuarioAsync(usuarioId))
                .ThrowsAsync(new KeyNotFoundException("Usuário não encontrado"));

            // Act
            var resultado = await _controller.ObterRelatorioDesempenho(usuarioId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal("Usuário não encontrado", notFoundResult.Value);
        }

        [Fact]
        public async Task ObterRelatorioDesempenho_DeveRetornarForbiddenParaUnauthorizedAccessException()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ObterRelatorioDesempenhoUsuarioAsync(usuarioId))
                .ThrowsAsync(new UnauthorizedAccessException("Acesso negado"));

            // Act
            var resultado = await _controller.ObterRelatorioDesempenho(usuarioId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ObterRelatorioDesempenho_DeveRetornarInternalServerErrorParaExcecaoGenerica()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            _mockServicoTarefa.Setup(s => s.ObterRelatorioDesempenhoUsuarioAsync(usuarioId))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.ObterRelatorioDesempenho(usuarioId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_DeveInicializarCorretamente()
        {
            // Arrange & Act
            var controller = new TarefasController(_mockServicoTarefa.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region Testes de Validação de Modelos

        [Fact]
        public async Task AdicionarComentario_DeveRetornarBadRequestParaConteudoVazio()
        {
            // Arrange
            _controller.ModelState.AddModelError("Conteudo", "O conteúdo do comentário é obrigatório");
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.NewGuid(),
                Conteudo = "", // Conteúdo vazio
                UsuarioId = Guid.NewGuid()
            };

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task AdicionarComentario_DeveRetornarBadRequestParaTarefaIdVazio()
        {
            // Arrange
            _controller.ModelState.AddModelError("TarefaId", "O ID da tarefa é obrigatório");
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.Empty, // ID vazio
                Conteudo = "Comentário válido",
                UsuarioId = Guid.NewGuid()
            };

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task AdicionarComentario_DeveRetornarBadRequestParaUsuarioIdVazio()
        {
            // Arrange
            _controller.ModelState.AddModelError("UsuarioId", "O ID do usuário é obrigatório");
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.NewGuid(),
                Conteudo = "Comentário válido",
                UsuarioId = Guid.Empty // ID vazio
            };

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task AdicionarComentario_DeveRetornarBadRequestParaConteudoMuitoLongo()
        {
            // Arrange
            _controller.ModelState.AddModelError("Conteudo", "O comentário não pode exceder 1000 caracteres");
            var conteudoLongo = new string('A', 1001); // 1001 caracteres
            var criarComentarioDto = new CriarComentarioTarefaDto
            {
                TarefaId = Guid.NewGuid(),
                Conteudo = conteudoLongo,
                UsuarioId = Guid.NewGuid()
            };

            // Act
            var resultado = await _controller.AdicionarComentario(criarComentarioDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        #endregion
    }
}