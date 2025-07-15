using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Excecoes;
using Skopia.Services.Interfaces;
using Skopia.Services.Modelos;

namespace Skopia.API.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class TarefasController : ControllerBase 
{
    private readonly IServicoTarefa _servicoTarefa; 
    private readonly ILogger<TarefasController> _logger;

    public TarefasController(IServicoTarefa servicoTarefa, ILogger<TarefasController> logger)
    {
        _servicoTarefa = servicoTarefa;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as tarefas de um projeto específico.
    /// </summary>
    /// <param name="projetoId">ID do projeto.</param>
    /// <returns>Lista de tarefas.</returns>
    [HttpGet("projeto/{projetoId}")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TarefaDto>>> ObterTodosPorProjetoId(Guid projetoId) 
    {
        _logger.LogInformation("Requisição para obter tarefas do projeto ID: {ProjetoId}", projetoId);
        if (projetoId == Guid.Empty)
        {
            _logger.LogWarning("Tentativa de obter tarefas com ID de projeto vazio.");
            return BadRequest("O ID do projeto não pode ser vazio.");
        }
        
        try
        {
            var tarefas = await _servicoTarefa.ObterTodosPorProjetoIdAsync(projetoId);
            _logger.LogInformation("Tarefas obtidas com sucesso para o projeto ID: {ProjetoId}. Quantidade: {Count}", projetoId, tarefas.Count());
            return Ok(tarefas);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Projeto com ID {ProjetoId} não encontrado ao tentar obter tarefas.", projetoId);
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
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
    public async Task<ActionResult<TarefaDto>> ObterPorId(Guid id) 
    {
        _logger.LogInformation("Requisição para obter tarefa com ID: {Id}", id);
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Tentativa de obter tarefa com ID vazio.");
            return BadRequest("O ID da tarefa não pode ser vazio."); 
        }
        try
        {
            var tarefa = await _servicoTarefa.ObterPorIdAsync(id);
            _logger.LogInformation("Tarefa obtida com sucesso: {TarefaId}", id);
            return Ok(tarefa);
        }
        catch (Exception)
        {
            _logger.LogWarning("Tarefa com ID {Id} não encontrada.", id);
            return NotFound($"Tarefa com ID {id} não encontrada."); 
        }
        
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <param name="criarTarefaDto">Dados da tarefa.</param>
    /// <returns>Tarefa criada.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)] 
    public async Task<ActionResult<TarefaDto>> CriarTarefa([FromBody] CriarTarefaDto criarTarefaDto) 
    {
        _logger.LogInformation("Tentativa de criar nova tarefa: {Titulo} para o projeto ID: {ProjetoId}", criarTarefaDto.Titulo, criarTarefaDto.ProjetoId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Falha na validação do modelo ao criar tarefa. Erros: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);
        }
            

        try
        {
            var tarefaCriada = await _servicoTarefa.CriarAsync(criarTarefaDto);
            _logger.LogInformation("Tarefa '{Titulo}' (ID: {TarefaId}) criada com sucesso para o projeto ID: {ProjetoId}.", tarefaCriada.Titulo, tarefaCriada.Id, tarefaCriada.ProjetoId);
            return CreatedAtAction(nameof(ObterPorId), new { id = tarefaCriada.Id }, tarefaCriada);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Projeto com ID {ProjetoId} não encontrado ao tentar criar tarefa.", criarTarefaDto.ProjetoId);
            return NotFound(ex.Message); 
        }
        catch (InvalidOperationException ex) 
        {
            _logger.LogWarning(ex, "Regra de negócio violada ao criar tarefa '{Titulo}' para o projeto ID: {ProjetoId}. Mensagem: {Message}", criarTarefaDto.Titulo, criarTarefaDto.ProjetoId, ex.Message);
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
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TarefaDto>> Atualizar(Guid id, [FromBody] AtualizarTarefaDto atualizarTarefaDto)
    {
        _logger.LogInformation("Tentativa de atualizar tarefa com ID: {Id}", id);

        if (id == Guid.Empty)
        { 
            _logger.LogWarning("Tentativa de atualizar tarefa com ID vazio.");
            return BadRequest("O ID da tarefa não pode ser vazio.");
        }
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Falha na validação do modelo ao atualizar tarefa. Erros: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);
        }
            

        try
        {
            var tarefaAtualizada = await _servicoTarefa.AtualizarAsync(id, atualizarTarefaDto);
            _logger.LogInformation("Tarefa com ID {Id} atualizada com sucesso.", id);
            return Ok(tarefaAtualizada);
        }
        catch (KeyNotFoundException ex) 
        {
            _logger.LogWarning(ex, "Tarefa com ID {Id} não encontrada ao tentar atualizar.", id);
            return NotFound(ex.Message); 
        }
        catch (InvalidOperationException ex) 
        {
            _logger.LogWarning(ex, "Regra de negócio violada ao atualizar tarefa com ID {Id}. Mensagem: {Message}", id, ex.Message);
            return BadRequest(new { mensagem = ex.Message }); 
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma tarefa.
    /// </summary>
    /// <param name="id">ID da tarefa.</param>
    /// <returns>Nenhum conteúdo (204 No Content).</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Excluir(Guid id) 
    {
        _logger.LogInformation("Tentativa de excluir tarefa com ID: {Id}", id);

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Tentativa de excluir tarefa com ID vazio.");
            return BadRequest("O ID da tarefa não pode ser vazio."); 
        }

        try
        {
            var resultado = await _servicoTarefa.ExcluirAsync(id);
            if (!resultado)
            {
                _logger.LogWarning("Tarefa com ID {Id} não encontrada para exclusão.", id);
                return NotFound($"Tarefa com ID {id} não encontrada.");
            }
            _logger.LogInformation("Tarefa com ID {Id} excluída com sucesso (soft delete).", id);
            return NoContent();
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

        //var resultado = await _servicoTarefa.ExcluirAsync(id);
        //if (!resultado)
        //{
        //    _logger.LogWarning("Tarefa com ID {Id} não encontrada para exclusão.", id);
        //    return NotFound($"Tarefa com ID {id} não encontrada."); 
        //}

        //_logger.LogInformation("Tarefa com ID {Id} excluída com sucesso (soft delete).", id);
        //return NoContent(); 
    }

    /// <summary>
    /// Adiciona um comentário a uma tarefa.
    /// </summary>
    /// <param name="criarComentarioTarefaDto">Dados do comentário.</param>
    /// <returns>Tarefa com o novo comentário.</returns>
    [HttpPost("comentarios")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TarefaDto>> AdicionarComentario([FromBody] CriarComentarioTarefaDto criarComentarioTarefaDto) 
    {
        _logger.LogInformation("Tentativa de adicionar comentário à tarefa ID: {TarefaId}", criarComentarioTarefaDto.TarefaId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Falha na validação do modelo ao adicionar comentário. Erros: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);
        }
            

        try
        {
            var tarefa = await _servicoTarefa.AdicionarComentarioAsync(criarComentarioTarefaDto);
            _logger.LogInformation("Comentário adicionado com sucesso à tarefa ID: {TarefaId}", criarComentarioTarefaDto.TarefaId);
            return Ok(tarefa);
        }
        catch (KeyNotFoundException ex) 
        {
            _logger.LogWarning(ex, "Tarefa com ID {TarefaId} não encontrada ao tentar adicionar comentário.", criarComentarioTarefaDto.TarefaId);
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Obtém o relatório de desempenho para um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <returns>Relatório de desempenho.</returns>
    [HttpGet("desempenho/{usuarioId}")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RelatorioDesempenhoDto>> ObterRelatorioDesempenho(Guid usuarioId) 
    {
        _logger.LogInformation("Requisição para obter relatório de desempenho do usuário ID: {UsuarioId}", usuarioId);

        if (usuarioId == Guid.Empty) 
        { 
            _logger.LogWarning("Tentativa de obter relatório de desempenho com ID de usuário vazio.");
            return BadRequest("O ID do usuário não pode ser vazio.");
        }

        try
        {
            var relatorios = await _servicoTarefa.ObterRelatorioDesempenhoUsuarioAsync(usuarioId);
            _logger.LogInformation("Relatório de desempenho obtido com sucesso");
            return Ok(relatorios);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Usuário com ID {usuarioId} não encontrado ao tentar obter relatório de desempenho.");
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, $"Usuário ID: {usuarioId} tentou acessar relatório de desempenho sem ser gerente.");
            return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = ex.Message });
        }
    }
}