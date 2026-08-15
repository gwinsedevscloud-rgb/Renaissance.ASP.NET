using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patients;

    public PatientsController(IPatientService patients)
    {
        _patients = patients;
    }

    [HttpGet]
    public async Task<ActionResult<List<Patient>>> GetAll([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var patients = await _patients.GetAllAsync(q, cancellationToken);
        return Ok(patients);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Patient>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("next-client-number")]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<string>> PeekNextClientNumber(CancellationToken cancellationToken)
    {
        var clientNumber = await _patients.PeekNextClientNumberAsync(cancellationToken);
        return Ok(clientNumber);
    }

    [HttpPost]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<Patient>> Create([FromBody] Patient patient, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _patients.CreateAsync(patient, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<Patient>> Update(Guid id, [FromBody] Patient incoming, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _patients.UpdateAsync(id, incoming, cancellationToken);
            return patient is null ? NotFound() : Ok(patient);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [RequireModule(AppModule.Clients)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _patients.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
