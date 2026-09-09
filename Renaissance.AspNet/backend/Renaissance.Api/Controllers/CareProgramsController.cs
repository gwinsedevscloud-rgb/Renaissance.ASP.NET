using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/care-programs")]
public class CareProgramsController : ControllerBase
{
    private readonly ICareProgramService _programs;
    private readonly IOutreachModeService _outreach;

    public CareProgramsController(ICareProgramService programs, IOutreachModeService outreach)
    {
        _programs = programs;
        _outreach = outreach;
    }

    [HttpGet("outreach-status")]
    [Authorize]
    public async Task<ActionResult<OutreachStatusDto>> GetOutreachStatus(CancellationToken cancellationToken)
        => Ok(await _outreach.GetStatusAsync(cancellationToken));

    [HttpGet]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<List<CareProgramDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _programs.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<CareProgramDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var program = await _programs.GetByIdAsync(id, cancellationToken);
        return program is null ? NotFound() : Ok(program);
    }

    [HttpPost]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<CareProgramDto>> Create([FromBody] SaveCareProgramRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _programs.CreateAsync(request, User.Identity?.Name ?? "admin", cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<CareProgramDto>> Update(Guid id, [FromBody] SaveCareProgramRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _programs.UpdateAsync(id, request, User.Identity?.Name ?? "admin", cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _programs.DeleteAsync(id, User.Identity?.Name ?? "admin", cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/activate")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<CareProgramDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var program = await _programs.ActivateAsync(id, User.Identity?.Name ?? "admin", cancellationToken);
            return program is null ? NotFound() : Ok(program);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/end")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<CareProgramDto>> End(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var program = await _programs.EndAsync(id, User.Identity?.Name ?? "admin", cancellationToken);
            return program is null ? NotFound() : Ok(program);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/generate-ids")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<GenerateProgramIdsResult>> GenerateIds(
        Guid id,
        [FromBody] GenerateProgramIdsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _programs.GeneratePatientIdsAsync(id, request, User.Identity?.Name ?? "admin", cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}/patient-ids")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<List<ProgramPatientIdDto>>> GetPatientIds(
        Guid id,
        [FromQuery] ProgramPatientIdStatus? status,
        CancellationToken cancellationToken)
        => Ok(await _programs.GetPatientIdsAsync(id, status, cancellationToken));

    [HttpPost("validate-id")]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<ValidateProgramIdResult>> ValidateId(
        [FromBody] ValidateProgramIdRequest request,
        CancellationToken cancellationToken)
        => Ok(await _programs.ValidatePatientIdAsync(request, cancellationToken));

    [HttpPost("register-patient")]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<Patient>> RegisterPatient(
        [FromBody] RegisterOutreachPatientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _programs.RegisterPatientAsync(request, User.Identity?.Name ?? "admin", cancellationToken);
            return Ok(patient);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
