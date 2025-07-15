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
using System.Threading.Tasks;
using Xunit;

namespace Skopia.Test.Controller
{
    public class ProjetosControllerTests
    {
        private readonly Mock<IServicoProjeto> _mockServicoProjeto;
        private readonly Mock<ILogger<ProjetosController>> _mockLogger;
        private readonly ProjetosController _controller;

        public ProjetosControllerTests()
        {
            _mockServicoProjeto = new Mock<IServicoProjeto>();
            _mockLogger = new Mock<ILogger<ProjetosController>>();
            _controller = new ProjetosController(_mockServicoProjeto.Object, _mockLogger.Object);
        }

        #region ObterTodosPorUsuarioId Tests

        [Fact]
        public async Task ObterTodosPorUsuarioId_DeveRetornarOkComProjetos()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var projetos = new List<ProjetoDto>
            {
                new ProjetoDto { Id = Guid.NewGuid(), Nome = "Projeto Teste 1" },
                new ProjetoDto { Id = Guid.NewGuid(), Nome = "Projeto Teste 2" }
            };
            _mockServicoProjeto.Setup(s => s.ObterTodosPorUsuarioIdAsync(usuarioId)).ReturnsAsync(projetos);

            // Act
            var resultado = await _controller.ObterTodosPorUsuarioId(usuarioId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(projetos, okResult.Value);
            _mockServicoProjeto.Verify(s => s.ObterTodosPorUsuarioIdAsync(usuarioId), Times.Once);
        }

        [Fact]
        public async Task ObterTodosPorUsuarioId_DeveRetornarBadRequestParaUsuarioIdVazio()
        {
            // Arrange
            var usuarioId = Guid.Empty;

            // Act
            var resultado = await _controller.ObterTodosPorUsuarioId(usuarioId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID do usuário não pode ser vazio.", badRequestResult.Value);
            _mockServicoProjeto.Verify(s => s.ObterTodosPorUsuarioIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ObterTodosPorUsuarioId_DeveRetornarInternalServerErrorQuandoOcorreExcecao()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            _mockServicoProjeto.Setup(s => s.ObterTodosPorUsuarioIdAsync(usuarioId))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act
            var resultado = await _controller.ObterTodosPorUsuarioId(usuarioId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Ocorreu um erro interno.", statusCodeResult.Value);
        }

        #endregion

        #region ObterPorId Tests

        [Fact]
        public async Task ObterPorId_DeveRetornarOkComProjeto()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var projeto = new ProjetoDto { Id = projetoId, Nome = "Projeto Teste" };
            _mockServicoProjeto.Setup(s => s.ObterPorIdAsync(projetoId)).ReturnsAsync(projeto);

            // Act
            var resultado = await _controller.ObterPorId(projetoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(projeto, okResult.Value);
            _mockServicoProjeto.Verify(s => s.ObterPorIdAsync(projetoId), Times.Once);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarBadRequestParaIdVazio()
        {
            // Arrange
            var projetoId = Guid.Empty;

            // Act
            var resultado = await _controller.ObterPorId(projetoId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID do projeto não pode ser vazio.", badRequestResult.Value);
            _mockServicoProjeto.Verify(s => s.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFoundParaProjetoNaoEncontrado()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            _mockServicoProjeto.Setup(s => s.ObterPorIdAsync(projetoId)).ReturnsAsync((ProjetoDto)null);

            // Act
            var resultado = await _controller.ObterPorId(projetoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal($"Projeto com ID {projetoId} não encontrado.", notFoundResult.Value);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFoundQuandoKeyNotFoundExceptionOcorre()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var mensagemErro = "Projeto não encontrado";
            _mockServicoProjeto.Setup(s => s.ObterPorIdAsync(projetoId))
                .ThrowsAsync(new KeyNotFoundException(mensagemErro));

            // Act
            var resultado = await _controller.ObterPorId(projetoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal(mensagemErro, notFoundResult.Value);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarInternalServerErrorQuandoExcecaoGenerica()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var mensagemErro = "Erro interno do servidor";
            _mockServicoProjeto.Setup(s => s.ObterPorIdAsync(projetoId))
                .ThrowsAsync(new Exception(mensagemErro));

            // Act
            var resultado = await _controller.ObterPorId(projetoId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            // Verificação usando JSON serialization
            Assert.NotNull(statusCodeResult.Value);
            var jsonString = System.Text.Json.JsonSerializer.Serialize(statusCodeResult.Value);
            Assert.Contains("mensagem", jsonString);
            Assert.Contains(mensagemErro, jsonString);
        }

        #endregion

        #region Criar Tests

        [Fact]
        public async Task Criar_DeveRetornarCreatedComProjeto()
        {
            // Arrange
            var criarProjetoDto = new CriarProjetoDto { Nome = "Novo Projeto" };
            var projetoCriado = new ProjetoDto { Id = Guid.NewGuid(), Nome = "Novo Projeto" };
            _mockServicoProjeto.Setup(s => s.CriarAsync(criarProjetoDto)).ReturnsAsync(projetoCriado);

            // Act
            var resultado = await _controller.Criar(criarProjetoDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal("ObterPorId", createdResult.ActionName);
            Assert.Equal(projetoCriado.Id, createdResult.RouteValues["id"]);
            Assert.Equal(projetoCriado, createdResult.Value);
            _mockServicoProjeto.Verify(s => s.CriarAsync(criarProjetoDto), Times.Once);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequestQuandoModelStateInvalido()
        {
            // Arrange
            var criarProjetoDto = new CriarProjetoDto();
            _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

            // Act
            var resultado = await _controller.Criar(criarProjetoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
            _mockServicoProjeto.Verify(s => s.CriarAsync(It.IsAny<CriarProjetoDto>()), Times.Never);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequestQuandoExcecaoOcorre()
        {
            // Arrange
            var criarProjetoDto = new CriarProjetoDto { Nome = "Projeto Teste" };
            _mockServicoProjeto.Setup(s => s.CriarAsync(criarProjetoDto))
                .ThrowsAsync(new Exception("Erro ao criar"));

            // Act
            var resultado = await _controller.Criar(criarProjetoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        #endregion

        #region Atualizar Tests

        [Fact]
        public async Task Atualizar_DeveRetornarOkComProjetoAtualizado()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var atualizarProjetoDto = new AtualizarProjetoDto { Nome = "Projeto Atualizado" };
            var projetoAtualizado = new ProjetoDto { Id = projetoId, Nome = "Projeto Atualizado" };
            _mockServicoProjeto.Setup(s => s.AtualizarAsync(projetoId, atualizarProjetoDto))
                .ReturnsAsync(projetoAtualizado);

            // Act
            var resultado = await _controller.Atualizar(projetoId, atualizarProjetoDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(projetoAtualizado, okResult.Value);
            _mockServicoProjeto.Verify(s => s.AtualizarAsync(projetoId, atualizarProjetoDto), Times.Once);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequestParaIdVazio()
        {
            // Arrange
            var projetoId = Guid.Empty;
            var atualizarProjetoDto = new AtualizarProjetoDto { Nome = "Projeto Atualizado" };

            // Act
            var resultado = await _controller.Atualizar(projetoId, atualizarProjetoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal("O ID do projeto não pode ser vazio.", badRequestResult.Value);
            _mockServicoProjeto.Verify(s => s.AtualizarAsync(It.IsAny<Guid>(), It.IsAny<AtualizarProjetoDto>()), Times.Never);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequestQuandoModelStateInvalido()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var atualizarProjetoDto = new AtualizarProjetoDto();
            _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

            // Act
            var resultado = await _controller.Atualizar(projetoId, atualizarProjetoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
            _mockServicoProjeto.Verify(s => s.AtualizarAsync(It.IsAny<Guid>(), It.IsAny<AtualizarProjetoDto>()), Times.Never);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNotFoundQuandoKeyNotFoundExceptionOcorre()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var atualizarProjetoDto = new AtualizarProjetoDto { Nome = "Projeto Atualizado" };
            var mensagemErro = "Projeto não encontrado";
            _mockServicoProjeto.Setup(s => s.AtualizarAsync(projetoId, atualizarProjetoDto))
                .ThrowsAsync(new KeyNotFoundException(mensagemErro));

            // Act
            var resultado = await _controller.Atualizar(projetoId, atualizarProjetoDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
            Assert.Equal(mensagemErro, notFoundResult.Value);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarInternalServerErrorQuandoExcecaoGenerica()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var atualizarProjetoDto = new AtualizarProjetoDto { Nome = "Projeto Atualizado" };
            var mensagemErro = "Erro interno";
            _mockServicoProjeto.Setup(s => s.AtualizarAsync(projetoId, atualizarProjetoDto))
                .ThrowsAsync(new Exception(mensagemErro));

            // Act
            var resultado = await _controller.Atualizar(projetoId, atualizarProjetoDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            // Verificação usando JSON serialization
            Assert.NotNull(statusCodeResult.Value);
            var jsonString = System.Text.Json.JsonSerializer.Serialize(statusCodeResult.Value);
            Assert.Contains("mensagem", jsonString);
            Assert.Contains(mensagemErro, jsonString);
        }

        #endregion

        #region Excluir Tests

        [Fact]
        public async Task Excluir_DeveRetornarNoContentQuandoExclusaoSucesso()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            _mockServicoProjeto.Setup(s => s.ExcluirAsync(projetoId)).ReturnsAsync(true);

            // Act
            var resultado = await _controller.Excluir(projetoId);

            // Assert
            Assert.IsType<NoContentResult>(resultado);
            _mockServicoProjeto.Verify(s => s.ExcluirAsync(projetoId), Times.Once);
        }

        [Fact]
        public async Task Excluir_DeveRetornarBadRequestParaIdVazio()
        {
            // Arrange
            var projetoId = Guid.Empty;

            // Act
            var resultado = await _controller.Excluir(projetoId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal("O ID do projeto não pode ser vazio.", badRequestResult.Value);
            _mockServicoProjeto.Verify(s => s.ExcluirAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFoundQuandoProjetoNaoEncontrado()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            _mockServicoProjeto.Setup(s => s.ExcluirAsync(projetoId)).ReturnsAsync(false);

            // Act
            var resultado = await _controller.Excluir(projetoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
            Assert.Equal($"Projeto com ID {projetoId} não encontrado.", notFoundResult.Value);
        }

        [Fact]
        public async Task Excluir_DeveRetornarBadRequestQuandoInvalidOperationExceptionOcorre()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var mensagemErro = "Operação inválida";
            _mockServicoProjeto.Setup(s => s.ExcluirAsync(projetoId))
                .ThrowsAsync(new InvalidOperationException(mensagemErro));

            // Act
            var resultado = await _controller.Excluir(projetoId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);

            // Verificação mais robusta
            Assert.NotNull(badRequestResult.Value);

            var valueType = badRequestResult.Value.GetType();
            var mensagemProperty = valueType.GetProperty("mensagem");

            Assert.NotNull(mensagemProperty);
            var actualMensagem = mensagemProperty.GetValue(badRequestResult.Value)?.ToString();
            Assert.Equal(mensagemErro, actualMensagem);
        }

        [Fact]
        public async Task Excluir_DeveRetornarUnprocessableEntityQuandoExcecaoDominioOcorre()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var mensagemErro = "Erro de domínio";
            _mockServicoProjeto.Setup(s => s.ExcluirAsync(projetoId))
                .ThrowsAsync(new ExcecaoDominio(mensagemErro));

            // Act
            var resultado = await _controller.Excluir(projetoId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, statusCodeResult.StatusCode);

            // Verificação mais robusta
            Assert.NotNull(statusCodeResult.Value);

            var valueType = statusCodeResult.Value.GetType();
            var mensagemProperty = valueType.GetProperty("mensagem");

            Assert.NotNull(mensagemProperty);
            var actualMensagem = mensagemProperty.GetValue(statusCodeResult.Value)?.ToString();
            Assert.Equal(mensagemErro, actualMensagem);
        }

        [Fact]
        public async Task Excluir_DeveRetornarInternalServerErrorQuandoExcecaoGenerica()
        {
            // Arrange
            var projetoId = Guid.NewGuid();
            var mensagemErro = "Erro interno";
            _mockServicoProjeto.Setup(s => s.ExcluirAsync(projetoId))
                .ThrowsAsync(new Exception(mensagemErro));

            // Act
            var resultado = await _controller.Excluir(projetoId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            // Verificação usando JSON serialization
            Assert.NotNull(statusCodeResult.Value);
            var jsonString = System.Text.Json.JsonSerializer.Serialize(statusCodeResult.Value);
            Assert.Contains("mensagem", jsonString);
            Assert.Contains(mensagemErro, jsonString);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_DeveInicializarCorretamente()
        {
            // Arrange & Act
            var controller = new ProjetosController(_mockServicoProjeto.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion
    }
}