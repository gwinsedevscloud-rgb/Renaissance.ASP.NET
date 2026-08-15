using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/optometrists")]
[RequireModule(AppModule.Optometrists)]
public class OptometristsController : ControllerBase
{
    private readonly IOptometristService _optometrists;

    public OptometristsController(IOptometristService optometrists)
    {
        _optometrists = optometrists;
    }

    [HttpGet]
    public async Task<ActionResult<List<Optometrist>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _optometrists.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Optometrist>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _optometrists.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<Optometrist>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _optometrists.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Optometrist>> Create([FromBody] Optometrist optometrist, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _optometrists.CreateAsync(optometrist, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Optometrist>> Update(Guid id, [FromBody] Optometrist incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _optometrists.UpdateAsync(id, incoming, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _optometrists.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
