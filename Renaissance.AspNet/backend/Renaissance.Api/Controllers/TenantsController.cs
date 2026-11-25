using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[RequireModule(AppModule.Tenants)]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenants;

    public TenantsController(ITenantService tenants)
    {
        _tenants = tenants;
    }

    [HttpGet]
    public async Task<ActionResult<List<TenantDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _tenants.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actor = User.FindFirstValue(ClaimTypes.Name) ?? "platform";
            var created = await _tenants.CreateAsync(request, actor, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/active")]
    public async Task<ActionResult<TenantDto>> SetActive(Guid id, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            var actor = User.FindFirstValue(ClaimTypes.Name) ?? "platform";
            var updated = await _tenants.SetActiveAsync(id, isActive, actor, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
