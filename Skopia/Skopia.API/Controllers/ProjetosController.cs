using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Excecoes;
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos;

namespace Skopia.API.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class ProjetosController : ControllerBase 
{
    private readonly IServicoProjeto _servicoProjeto;
    private readonly ILogger<ProjetosController> _logger;

    public ProjetosController(IServicoProjeto servicoProjeto, ILogger<ProjetosController> logger)
    {
        _servicoProjeto = servicoProjeto;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os projetos de um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <returns>Lista de projetos.</returns>
    [HttpGet("usuario/{usuarioId}")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] 
    public async Task<ActionResult<IEnumerable<ProjetoDto>>> ObterTodosPorUsuarioId(Guid usuarioId) 
    {
        _logger.LogInformation("Obtendo todos os projetos para o usuário ID: {UsuarioId}", usuarioId);

        if (usuarioId == Guid.Empty)
        {
            _logger.LogWarning("ID do usuário vazio ao tentar obter projetos.");
            return BadRequest("O ID do usuário não pode ser vazio.");
        }

        try
        {
            var projetos = await _servicoProjeto.ObterTodosPorUsuarioIdAsync(usuarioId);
            _logger.LogInformation("Projetos obtidos com sucesso para o usuário ID: {UsuarioId}. Total de projetos: {TotalProjetos}", usuarioId, projetos.Count());
            return Ok(projetos);
        }
        catch (Exception)
        {
            _logger.LogError("Erro ao obter projetos para o usuário ID: {UsuarioId}", usuarioId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno.");
        }

        
    }

    /// <summary>
    /// Obtém um projeto pelo seu ID.
    /// </summary>
    /// <param name="id">ID do projeto.</param>
    /// <returns>Detalhes do projeto.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjetoDto>> ObterPorId(Guid id)
    {
        _logger.LogInformation("Obtendo projeto com ID: {Id}", id);

        if (id == Guid.Empty)
        { 
            _logger.LogWarning("ID do projeto vazio ao tentar obter projeto.");
            return BadRequest("O ID do projeto não pode ser vazio."); // Mensagem traduzida
        }

        try
        {
            var projeto = await _servicoProjeto.ObterPorIdAsync(id);
            if (projeto == null) 
            {
                _logger.LogWarning("Projeto com ID {Id} não encontrado.", id);
                return NotFound($"Projeto com ID {id} não encontrado."); // Mensagem traduzida
            }
            _logger.LogInformation("Projeto com ID {Id} obtido com sucesso.", id);
            return Ok(projeto);
        }
        catch (KeyNotFoundException ex) // Captura exceções de "não encontrado" que vêm do serviço
        {
            _logger.LogWarning("Projeto com ID {Id} não encontrado: {Message}", id, ex.Message);
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo projeto.
    /// </summary>
    /// <param name="criarProjetoDto">Dados do projeto.</param>
    /// <returns>Projeto criado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] 
    public async Task<ActionResult<ProjetoDto>> Criar([FromBody] CriarProjetoDto criarProjetoDto) 
    {
        _logger.LogInformation("Criando novo projeto com dados: {@CriarProjetoDto}", criarProjetoDto);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Dados inválidos para criação de projeto: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var projetoCriado = await _servicoProjeto.CriarAsync(criarProjetoDto);
            _logger.LogInformation("Projeto criado com sucesso: {Id}", projetoCriado.Id);
            return CreatedAtAction(nameof(ObterPorId), new { id = projetoCriado.Id }, projetoCriado);
        }
        catch (Exception)
        {
            _logger.LogError("Erro ao criar projeto com dados: {@CriarProjetoDto}", criarProjetoDto);   
            return BadRequest(ModelState);
        }
        
    }

    /// <summary>
    /// Atualiza um projeto existente.
    /// </summary>
    /// <param name="id">ID do projeto.</param>
    /// <param name="atualizarProjetoDto">Dados do projeto para atualização.</param>
    /// <returns>Projeto atualizado.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] 
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] 
    public async Task<ActionResult<ProjetoDto>> Atualizar(Guid id, [FromBody] AtualizarProjetoDto atualizarProjetoDto) 
    {
        _logger.LogInformation($"Atualizando projeto com ID: {id} e dados: {atualizarProjetoDto}");
        if (id == Guid.Empty)
        {
            _logger.LogWarning("ID do projeto vazio ao tentar atualizar projeto.");
            return BadRequest("O ID do projeto não pode ser vazio.");
        }
            

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Dados inválidos para atualização de projeto.");
            return BadRequest(ModelState);
        }
            

        try
        {
            var projetoAtualizado = await _servicoProjeto.AtualizarAsync(id, atualizarProjetoDto);
            _logger.LogInformation($"Projeto com ID {id} atualizado com sucesso.");
            return Ok(projetoAtualizado);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Projeto com ID {id}: {ex.Message}.");
            return NotFound(ex.Message); // Mensagem traduzida
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Exclui um projeto.
    /// </summary>
    /// <param name="id">ID do projeto.</param>
    /// <returns>Nenhum conteúdo (204 No Content).</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] 
    public async Task<ActionResult> Excluir(Guid id) 
    {
        _logger.LogInformation("Excluindo projeto com ID: {Id}", id);

        if (id == Guid.Empty)
        {
            _logger.LogWarning("ID do projeto vazio ao tentar excluir projeto.");
            return BadRequest("O ID do projeto não pode ser vazio.");
        }
             

        try
        {
            var resultado = await _servicoProjeto.ExcluirAsync(id);
           
            if (!resultado)
            {
                _logger.LogWarning("Projeto com ID {Id} não encontrado para exclusão.", id);
                return NotFound($"Projeto com ID {id} não encontrado."); 
            }
                
            _logger.LogInformation("Projeto com ID {Id} excluído com sucesso.", id);
            return NoContent(); 
        }
        catch (InvalidOperationException ex) 
        {
            _logger.LogError("Erro ao excluir projeto com ID {Id}: {Message}", id, ex.Message);
            return BadRequest(new { mensagem = ex.Message }); 
        }
        catch (ExcecaoDominio ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
        }

    }
}