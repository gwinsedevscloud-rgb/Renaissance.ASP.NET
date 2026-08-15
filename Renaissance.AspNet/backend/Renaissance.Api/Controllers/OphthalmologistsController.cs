using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/ophthalmologists")]
[RequireModule(AppModule.Ophthalmologists)]
public class OphthalmologistsController : ControllerBase
{
    private readonly IOphthalmologistService _ophthalmologists;

    public OphthalmologistsController(IOphthalmologistService ophthalmologists)
    {
        _ophthalmologists = ophthalmologists;
    }

    [HttpGet]
    public async Task<ActionResult<List<Ophthalmologist>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _ophthalmologists.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Ophthalmologist>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _ophthalmologists.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<ActionResult<List<Ophthalmologist>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _ophthalmologists.GetByPatientAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Ophthalmologist>> Create([FromBody] Ophthalmologist ophthalmologist, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _ophthalmologists.CreateAsync(ophthalmologist, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Ophthalmologist>> Update(Guid id, [FromBody] Ophthalmologist incoming, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _ophthalmologists.UpdateAsync(id, incoming, cancellationToken);
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
        var deleted = await _ophthalmologists.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
