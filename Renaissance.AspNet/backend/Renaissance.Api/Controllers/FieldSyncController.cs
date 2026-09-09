using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;
using System.Security.Claims;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/field-sync")]
public class FieldSyncController : ControllerBase
{
    private readonly IFieldSyncService _sync;

    public FieldSyncController(IFieldSyncService sync)
    {
        _sync = sync;
    }

    /// <summary>Download outreach config + clinical catalogs for offline caching.</summary>
    [HttpGet("pull")]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<FieldSyncPullDto>> Pull(
        [FromQuery] string? deviceId = null,
        [FromQuery] string? deviceLabel = null,
        CancellationToken cancellationToken = default)
    {
        var actor = User.Identity?.Name ?? "field";
        return Ok(await _sync.PullAsync(deviceId, deviceLabel, actor, cancellationToken));
    }

    /// <summary>Upload queued offline clinical records from a field tablet.</summary>
    [HttpPost("push")]
    [RequireModule(AppModule.Clients)]
    public async Task<ActionResult<FieldSyncPushResultDto>> Push(
        [FromBody] FieldSyncPushRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var actor = User.Identity?.Name ?? "field";
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var isAdmin = User.IsInRole("Administrator")
                          || string.Equals(User.FindFirstValue(ClaimTypes.Role), "Administrator", StringComparison.OrdinalIgnoreCase);
            return Ok(await _sync.PushAsync(request, actor, userId, isAdmin, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>List field tablets that have synced (admin visibility).</summary>
    [HttpGet("devices")]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<List<FieldDeviceDto>>> Devices(CancellationToken cancellationToken)
        => Ok(await _sync.ListDevicesAsync(cancellationToken));
}
