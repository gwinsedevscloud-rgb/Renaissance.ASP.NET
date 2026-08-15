using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/consultations")]
[RequireModule(AppModule.Consultations)]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultations;

    public ConsultationsController(IConsultationService consultations)
    {
        _consultations = consultations;
    }

    [HttpGet]
    public async Task<ActionResult<List<Consultation>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _consultations.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Consultation>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _consultations.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<Consultation>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _consultations.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Consultation>> Create([FromBody] Consultation consultation, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _consultations.CreateAsync(consultation, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Consultation>> Update(Guid id, [FromBody] Consultation incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _consultations.UpdateAsync(id, incoming, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/itn-dispense")]
    public async Task<ActionResult<Consultation>> UpdateItnDispense(Guid id, [FromBody] bool itnDispense, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _consultations.UpdateItnDispenseAsync(id, itnDispense, cancellationToken);
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
        var deleted = await _consultations.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
