using Microsoft.AspNetCore.Mvc;
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos;

namespace Skopia.API.Controllers;

[ApiController]
[Route("api/[controller]")] // A rota será /api/Tarefas
public class TarefasController : ControllerBase // Nome do controlador traduzido
{
    private readonly IServicoTarefa _servicoTarefa; // Nome da interface de serviço traduzido

    public TarefasController(IServicoTarefa servicoTarefa)
    {
        _servicoTarefa = servicoTarefa;
    }

    /// <summary>
    /// Obtém todas as tarefas de um projeto específico.
    /// </summary>
    /// <param name="projetoId">ID do projeto.</param>
    /// <returns>Lista de tarefas.</returns>
    [HttpGet("projeto/{projetoId}")] // Rota ajustada para português
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Se o projeto não for encontrado pelo serviço
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TarefaDto>>> ObterTodosPorProjetoId(Guid projetoId) // Nome do método e tipo ajustados
    {
        if (projetoId == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID do projeto não pode ser vazio."); // Mensagem traduzida

        try
        {
            var tarefas = await _servicoTarefa.ObterTodosPorProjetoIdAsync(projetoId);
            return Ok(tarefas);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // Mensagem traduzida, ex: "Projeto com ID X não encontrado."
        }
    }

    /// <summary>
    /// Obtém uma tarefa pelo seu ID.
    /// </summary>
    /// <param name="id">ID da tarefa.</param>
    /// <returns>Detalhes da tarefa.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TarefaDto>> ObterPorId(Guid id) // Nome do método e tipo ajustados
    {
        if (id == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID da tarefa não pode ser vazio."); // Mensagem traduzida

        var tarefa = await _servicoTarefa.ObterPorIdAsync(id);
        if (tarefa == null) // Verifica se o serviço retornou null (não encontrou)
        {
            return NotFound($"Tarefa com ID {id} não encontrada."); // Mensagem traduzida
        }
        return Ok(tarefa);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <param name="criarTarefaDto">Dados da tarefa.</param>
    /// <returns>Tarefa criada.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Se o projeto associado não for encontrado
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] // Para KeyNotFoundException
    public async Task<ActionResult<TarefaDto>> Criar([FromBody] CriarTarefaDto criarTarefaDto) // Nome do método e DTO ajustados
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tarefaCriada = await _servicoTarefa.CriarAsync(criarTarefaDto);
            // Retorna 201 Created com a localização do novo recurso
            return CreatedAtAction(nameof(ObterPorId), new { id = tarefaCriada.Id }, tarefaCriada);
        }
        catch (KeyNotFoundException ex) // Para projeto não encontrado
        {
            return NotFound(ex.Message); // Mensagem traduzida
        }
        catch (InvalidOperationException ex) // Para limite de tarefas por projeto atingido ou outras regras de domínio
        {
            return BadRequest(new { mensagem = ex.Message }); // Objeto anônimo para JSON
        }
    }

    /// <summary>
    /// Atualiza uma tarefa existente.
    /// </summary>
    /// <param name="id">ID da tarefa.</param>
    /// <param name="atualizarTarefaDto">Dados da tarefa para atualização.</param>
    /// <returns>Tarefa atualizada.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)] // Para InvalidOperationException
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] // Para KeyNotFoundException
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] // Para ModelState.IsValid
    public async Task<ActionResult<TarefaDto>> Atualizar(Guid id, [FromBody] AtualizarTarefaDto atualizarTarefaDto) // Nome do método e DTO ajustados
    {
        if (id == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID da tarefa não pode ser vazio."); // Mensagem traduzida

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tarefaAtualizada = await _servicoTarefa.AtualizarAsync(id, atualizarTarefaDto);
            return Ok(tarefaAtualizada);
        }
        catch (KeyNotFoundException ex) // Tarefa não encontrada
        {
            return NotFound(ex.Message); // Mensagem traduzida
        }
        catch (InvalidOperationException ex) // Regras de negócio violadas (ex: status inválido)
        {
            return BadRequest(new { mensagem = ex.Message }); // Objeto anônimo para JSON
        }
    }

    /// <summary>
    /// Exclui uma tarefa.
    /// </summary>
    /// <param name="id">ID da tarefa.</param>
    /// <returns>Nenhum conteúdo (204 No Content).</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Se o ID for inválido
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Se a tarefa não for encontrada
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Excluir(Guid id) // Nome do método e tipo ajustados
    {
        if (id == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID da tarefa não pode ser vazio."); // Mensagem traduzida

        var resultado = await _servicoTarefa.ExcluirAsync(id);
        if (!resultado)
            return NotFound($"Tarefa com ID {id} não encontrada."); // Mensagem traduzida

        return NoContent(); // Retorno 204 No Content para exclusão bem-sucedida
    }

    /// <summary>
    /// Adiciona um comentário a uma tarefa.
    /// </summary>
    /// <param name="criarComentarioTarefaDto">Dados do comentário.</param>
    /// <returns>Tarefa com o novo comentário.</returns>
    [HttpPost("comentarios")] // Rota ajustada para português
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Se a tarefa não for encontrada
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Para ModelState.IsValid
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TarefaDto>> AdicionarComentario([FromBody] CriarComentarioTarefaDto criarComentarioTarefaDto) // Nome do método e DTO ajustados
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var tarefa = await _servicoTarefa.AdicionarComentarioAsync(criarComentarioTarefaDto);
            return Ok(tarefa);
        }
        catch (KeyNotFoundException ex) // Tarefa não encontrada
        {
            return NotFound(ex.Message); // Mensagem traduzida
        }
    }

    /// <summary>
    /// Obtém o relatório de desempenho para um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <returns>Relatório de desempenho.</returns>
    [HttpGet("desempenho/{usuarioId}")] // Rota ajustada para português
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // ID inválido
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Usuário não encontrado
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Sem permissão (não gerente)
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RelatorioDesempenhoDto>> ObterRelatorioDesempenho(Guid usuarioId) // Nome do método e DTO ajustados
    {
        if (usuarioId == Guid.Empty) // Validação para Guid.Empty
            return BadRequest("O ID do usuário não pode ser vazio."); // Mensagem traduzida

        try
        {
            var relatorio = await _servicoTarefa.ObterRelatorioDesempenhoUsuarioAsync(usuarioId);
            return Ok(relatorio);
        }
        catch (KeyNotFoundException ex) // Usuário não encontrado
        {
            return NotFound(ex.Message); // Mensagem traduzida
        }
        catch (UnauthorizedAccessException ex) // Não é gerente
        {
            // Retorna 403 Forbidden com a mensagem da exceção
            return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message }); // Objeto anônimo para JSON
        }
    }
}