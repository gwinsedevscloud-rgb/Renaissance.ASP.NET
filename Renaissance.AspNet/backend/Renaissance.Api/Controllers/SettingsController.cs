using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;
using System.Security.Claims;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settings;
    private readonly IHospitalModuleService _modules;

    public SettingsController(ISettingsService settings, IHospitalModuleService modules)
    {
        _settings = settings;
        _modules = modules;
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicHospitalSettingsDto>> GetPublic(CancellationToken cancellationToken)
        => Ok(await _settings.GetPublicSettingsAsync(cancellationToken));

    [HttpGet]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<HospitalSettingsDto>> Get(CancellationToken cancellationToken)
        => Ok(await _settings.GetHospitalSettingsAsync(cancellationToken));

    [HttpGet("lan/status")]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<LanAccessStatusDto>> GetLanStatus(CancellationToken cancellationToken)
        => Ok(await _settings.GetLanAccessStatusAsync(cancellationToken));

    [HttpPost("lan/unlock")]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<LanAccessUnlockResponse>> UnlockLan(
        [FromBody] LanAccessUnlockRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = User.Identity?.Name ?? "admin";
            return Ok(await _settings.UnlockLanAccessAsync(request, userId, userName, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("lan")]
    [RequireModule(AppModule.Administration)]
    [RequireLanAccess]
    public async Task<ActionResult<LanSettingsDto>> GetLan(CancellationToken cancellationToken)
        => Ok(await _settings.GetLanSettingsAsync(cancellationToken));

    [HttpGet("deployment")]
    [RequireModule(AppModule.Administration)]
    [RequireLanAccess]
    public async Task<ActionResult<DeploymentInfoDto>> GetDeployment(CancellationToken cancellationToken)
        => Ok(await _settings.GetDeploymentInfoAsync(cancellationToken));

    [HttpPut("lan")]
    [RequireModule(AppModule.Administration)]
    [RequireLanAccess]
    public async Task<ActionResult<HospitalSettingsDto>> UpdateLan(
        [FromBody] UpdateLanSettingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = User.Identity?.Name ?? "admin";
            await _settings.UpdateLanSettingsAsync(request, updatedBy, cancellationToken);
            return Ok(await _settings.GetLanSettingsAsync(cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("lan/password")]
    [RequireModule(AppModule.Administration)]
    [RequireLanAccess]
    public async Task<IActionResult> ChangeLanPassword(
        [FromBody] ChangeLanAccessPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = User.Identity?.Name ?? "admin";
            await _settings.ChangeLanAccessPasswordAsync(request, updatedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<HospitalSettingsDto>> Update(
        [FromBody] UpdateHospitalSettingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = User.Identity?.Name ?? "admin";
            var result = await _settings.UpdateHospitalSettingsAsync(request, updatedBy, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("modules")]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<HospitalModulesConfigDto>> GetModules(CancellationToken cancellationToken)
        => Ok(await _modules.GetConfigurationAsync(cancellationToken));

    [HttpPut("modules")]
    [RequireModule(AppModule.Administration)]
    public async Task<ActionResult<HospitalModulesConfigDto>> UpdateModules(
        [FromBody] UpdateHospitalModulesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = User.Identity?.Name ?? "admin";
            return Ok(await _modules.UpdateConfigurationAsync(request, updatedBy, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("modules/active")]
    [Authorize]
    public async Task<ActionResult<HospitalModulesStateDto>> GetActiveModules(CancellationToken cancellationToken)
        => Ok(await _modules.GetActiveStateAsync(cancellationToken));

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("Authenticated user id is missing.");
        }

        return userId;
    }
}
