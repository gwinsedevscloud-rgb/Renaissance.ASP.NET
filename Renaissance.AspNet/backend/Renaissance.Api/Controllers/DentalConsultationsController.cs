using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/dental-consultations")]
[RequireModule(AppModule.Dental)]
public class DentalConsultationsController : ControllerBase
{
    private readonly IDentalConsultationService _dentalConsultations;

    public DentalConsultationsController(IDentalConsultationService dentalConsultations)
    {
        _dentalConsultations = dentalConsultations;
    }

    [HttpGet]
    public async Task<ActionResult<List<DentalConsultation>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _dentalConsultations.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DentalConsultation>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dentalConsultations.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<DentalConsultation>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _dentalConsultations.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<DentalConsultation>> Create([FromBody] DentalConsultation dental, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _dentalConsultations.CreateAsync(dental, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DentalConsultation>> Update(Guid id, [FromBody] DentalConsultation incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _dentalConsultations.UpdateAsync(id, incoming, cancellationToken);
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
        var deleted = await _dentalConsultations.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
