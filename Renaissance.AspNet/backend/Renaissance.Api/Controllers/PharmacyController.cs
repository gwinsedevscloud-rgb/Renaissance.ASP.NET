using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/pharmacy")]
[RequireModule(AppModule.Pharmacy)]
public class PharmacyController : ControllerBase
{
    private readonly IPharmacyService _pharmacy;

    public PharmacyController(IPharmacyService pharmacy)
    {
        _pharmacy = pharmacy;
    }

    [HttpGet]
    public async Task<ActionResult<List<PharmacyPrescription>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _pharmacy.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PharmacyPrescription>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _pharmacy.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<PharmacyPrescription>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _pharmacy.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpGet("has-pending/{patientId:guid}")]
    public async Task<ActionResult<bool>> HasPending(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _pharmacy.HasPendingAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PharmacyPrescription>> Create([FromBody] PharmacyPrescription prescription, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _pharmacy.CreateAsync(prescription, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] List<PharmacyPrescription> prescriptions, CancellationToken cancellationToken)
    {
        try
        {
            await _pharmacy.CreateBulkAsync(prescriptions, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PharmacyPrescription>> Update(Guid id, [FromBody] PharmacyPrescription incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _pharmacy.UpdateAsync(id, incoming, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("dispense")]
    public async Task<IActionResult> DispenseBulk([FromBody] List<PharmacyDispenseUpdateDto> updates, CancellationToken cancellationToken)
    {
        try
        {
            await _pharmacy.DispenseBulkAsync(updates, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _pharmacy.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
