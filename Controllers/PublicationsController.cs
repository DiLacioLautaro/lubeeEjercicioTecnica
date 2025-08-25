using Microsoft.AspNetCore.Mvc;
using PruebaTecnica2025.Common;
using PruebaTecnica2025.Models.Dtos;
using PruebaTecnica2025.Services.Abstractions;

namespace PruebaTecnica2025.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicationsController : ControllerBase
{
    private readonly IPublicationService _svc;
    private readonly ILogger<PublicationsController> _logger;

    public PublicationsController(IPublicationService svc, ILogger<PublicationsController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    // GET api/publications
    [HttpGet]
    [ProducesResponseType(typeof(List<PublicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _svc.GetAllAsync(ct);
        return Ok(items);
    }

    // GET api/publications/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PublicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOne(int id, CancellationToken ct)
    {
        try
        {
            var item = await _svc.GetOneAsync(id, ct);
            return Ok(item);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "GetOne not found: {Id}", id);
            return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    // POST api/publications
    [HttpPost]
    [ProducesResponseType(typeof(PublicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PublicationCreateUpdateDto body, CancellationToken ct)
    {
        try
        {
            var created = await _svc.CreateAsync(body, ct);
            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogInformation(ex, "Validation error on create");
            return BadRequest(new ProblemDetails { Title = "Validación", Detail = ex.Message, Status = 400 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on create");
            return StatusCode(500, new ProblemDetails { Title = "Error inesperado", Detail = ex.Message, Status = 500 });
        }
    }

    // PUT api/publications/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PublicationCreateUpdateDto body, CancellationToken ct)
    {
        try
        {
            await _svc.UpdateAsync(id, body, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Update not found: {Id}", id);
            return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = 404 });
        }
        catch (DomainValidationException ex)
        {
            _logger.LogInformation(ex, "Validation error on update {Id}", id);
            return BadRequest(new ProblemDetails { Title = "Validación", Detail = ex.Message, Status = 400 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on update {Id}", id);
            return StatusCode(500, new ProblemDetails { Title = "Error inesperado", Detail = ex.Message, Status = 500 });
        }
    }

    // DELETE api/publications/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Delete not found: {Id}", id);
            return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = 404 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on delete {Id}", id);
            return StatusCode(500, new ProblemDetails { Title = "Error inesperado", Detail = ex.Message, Status = 500 });
        }
    }

    [HttpPost("bulk-delete")]
    [ProducesResponseType(typeof(BulkDeleteResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest body, CancellationToken ct)
    {
        try
        {
            if (body?.Ids == null || body.Ids.Count == 0)
                return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Detail = "Debes enviar IDs.", Status = 400 });

            var result = await _svc.DeleteManyAsync(body.Ids, ct);
            return Ok(result);
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validación", Detail = ex.Message, Status = 400 });
        }
        catch (Exception ex)
        {
            // Log en español
            // _logger.LogError(ex, "Error inesperado al borrar en lote");
            return StatusCode(500, new ProblemDetails { Title = "Error inesperado", Detail = ex.Message, Status = 500 });
        }
    }
}
