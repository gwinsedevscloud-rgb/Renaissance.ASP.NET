using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/ancillary-services")]
[RequireModule(AppModule.Ancillary)]
public class AncillaryController : ControllerBase
{
    private readonly IAncillaryService _ancillary;

    public AncillaryController(IAncillaryService ancillary)
    {
        _ancillary = ancillary;
    }

    [HttpGet]
    public async Task<ActionResult<List<Ancillary>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _ancillary.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Ancillary>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _ancillary.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<Ancillary>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _ancillary.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Ancillary>> Create([FromBody] Ancillary ancillary, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _ancillary.CreateAsync(ancillary, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Ancillary>> Update(Guid id, [FromBody] Ancillary incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _ancillary.UpdateAsync(id, incoming, cancellationToken);
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
        var deleted = await _ancillary.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
