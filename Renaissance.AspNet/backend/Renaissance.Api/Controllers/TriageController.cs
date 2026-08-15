using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/triage")]
[RequireModule(AppModule.Triage)]
public class TriageController : ControllerBase
{
    private readonly ITriageService _triage;

    public TriageController(ITriageService triage)
    {
        _triage = triage;
    }

    [HttpGet]
    public async Task<ActionResult<List<Triage>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _triage.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Triage>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _triage.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<Triage>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _triage.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Triage>> Create([FromBody] Triage triage, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _triage.CreateAsync(triage, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Triage>> Update(Guid id, [FromBody] Triage incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _triage.UpdateAsync(id, incoming, cancellationToken);
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
        var deleted = await _triage.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
