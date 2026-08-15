using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/laboratory")]
[RequireModule(AppModule.Laboratory)]
public class LaboratoryController : ControllerBase
{
    private readonly ILaboratoryService _laboratory;

    public LaboratoryController(ILaboratoryService laboratory)
    {
        _laboratory = laboratory;
    }

    [HttpGet]
    public async Task<ActionResult<List<Laboratory>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _laboratory.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Laboratory>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _laboratory.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<Laboratory>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _laboratory.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Laboratory>> Create([FromBody] Laboratory laboratory, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _laboratory.CreateAsync(laboratory, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Laboratory>> Update(Guid id, [FromBody] Laboratory incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _laboratory.UpdateAsync(id, incoming, cancellationToken);
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
        var deleted = await _laboratory.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
