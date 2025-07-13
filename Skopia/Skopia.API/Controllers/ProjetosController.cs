using Microsoft.AspNetCore.Mvc;
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos;

namespace Skopia.API.Controllers;

[ApiController]
[Route("api/[controller]")] // A rota será /api/Projetos
public class ProjetosController : ControllerBase // Nome do controlador traduzido
{
    private readonly IServicoProjeto _servicoProjeto; // Nome da interface de serviço traduzido

    public ProjetosController(IServicoProjeto servicoProjeto)
    {
        _servicoProjeto = servicoProjeto;
    }

    /// <summary>
    /// Obtém todos os projetos de um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <returns>Lista de projetos.</returns>
    [HttpGet("usuario/{usuarioId}")] // Rota ajustada para português
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] // Adicionado tipo para mensagem de erro
    public async Task<ActionResult<IEnumerable<ProjetoDto>>> ObterTodosPorUsuarioId(Guid usuarioId) // Nome do método e tipo ajustados
    {
        if (usuarioId == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID do usuário não pode ser vazio."); // Mensagem traduzida

        var projetos = await _servicoProjeto.ObterTodosPorUsuarioIdAsync(usuarioId);
        return Ok(projetos);
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
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] // Adicionado tipo para mensagem de erro
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] // Adicionado tipo para mensagem de erro
    public async Task<ActionResult<ProjetoDto>> ObterPorId(Guid id) // Nome do método e tipo ajustados
    {
        if (id == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID do projeto não pode ser vazio."); // Mensagem traduzida

        try
        {
            var projeto = await _servicoProjeto.ObterPorIdAsync(id);
            if (projeto == null) // Verificação se o serviço retornou null (não encontrou)
            {
                return NotFound($"Projeto com ID {id} não encontrado."); // Mensagem traduzida
            }
            return Ok(projeto);
        }
        catch (KeyNotFoundException ex) // Captura exceções de "não encontrado" que vêm do serviço
        {
            return NotFound(ex.Message); // Mensagem traduzida
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] // Tipo para erros de validação
    public async Task<ActionResult<ProjetoDto>> Criar([FromBody] CriarProjetoDto criarProjetoDto) // Nome do método e DTO ajustados
    {
        // O ModelState.IsValid já lida com as validações de DataAnnotations do DTO
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var projetoCriado = await _servicoProjeto.CriarAsync(criarProjetoDto);
        // Retorna 201 Created com a localização do novo recurso
        return CreatedAtAction(nameof(ObterPorId), new { id = projetoCriado.Id }, projetoCriado);
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
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] // Adicionado tipo para mensagem de erro
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] // Adicionado tipo para mensagem de erro
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] // Tipo para erros de validação
    public async Task<ActionResult<ProjetoDto>> Atualizar(Guid id, [FromBody] AtualizarProjetoDto atualizarProjetoDto) // Nome do método e DTO ajustados
    {
        if (id == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID do projeto não pode ser vazio."); // Mensagem traduzida

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var projetoAtualizado = await _servicoProjeto.AtualizarAsync(id, atualizarProjetoDto);
            return Ok(projetoAtualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // Mensagem traduzida
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
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] // Adicionado tipo para mensagem de erro
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] // Adicionado tipo para mensagem de erro
    public async Task<ActionResult> Excluir(Guid id) // Nome do método e tipo ajustados
    {
        if (id == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID do projeto não pode ser vazio."); // Mensagem traduzida

        try
        {
            var resultado = await _servicoProjeto.ExcluirAsync(id);
            if (!resultado)
                return NotFound($"Projeto com ID {id} não encontrado."); // Mensagem traduzida

            return NoContent(); // Retorno 204 No Content para exclusão bem-sucedida
        }
        catch (InvalidOperationException ex) // Captura exceções de regras de negócio (ex: projeto com tarefas pendentes)
        {
            // Retorna BadRequest com a mensagem da exceção para o cliente
            return BadRequest(new { mensagem = ex.Message }); // Objeto anônimo para JSON
        }
    }
}