using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;
using Renaissance.Infrastructure.Persistence;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/referrals")]
public class ReferralsController : ControllerBase
{
    private readonly IReferralService _referrals;
    private readonly RenaissanceDbContext _db;

    public ReferralsController(IReferralService referrals, RenaissanceDbContext db)
    {
        _referrals = referrals;
        _db = db;
    }

    [HttpGet("queue/{module}")]
    public async Task<ActionResult<List<ReferralQueueItemDto>>> GetQueue(AppModule module, CancellationToken cancellationToken)
    {
        if (!await HasModuleAsync(module, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _referrals.GetQueueAsync(module, cancellationToken));
    }

    [HttpGet("pending-count/{module}")]
    public async Task<ActionResult<int>> GetPendingCount(AppModule module, CancellationToken cancellationToken)
    {
        if (!await HasModuleAsync(module, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _referrals.GetPendingCountAsync(module, cancellationToken));
    }

    [HttpGet("module-counts")]
    public async Task<ActionResult<List<ModuleReferralCountDto>>> GetModuleCounts(CancellationToken cancellationToken)
    {
        var modules = await GetAccessibleModulesAsync(cancellationToken);
        return Ok(await _referrals.GetModuleCountsAsync(modules, cancellationToken));
    }

    [HttpGet("inbox")]
    public async Task<ActionResult<List<ReferralInboxItemDto>>> GetInbox(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var modules = await GetAccessibleReferralModulesAsync(cancellationToken);
        return Ok(await _referrals.GetInboxAsync(userId.Value, modules, cancellationToken));
    }

    [HttpGet("journey/{patientId:guid}")]
    public async Task<ActionResult<List<PatientJourneyStepDto>>> GetPatientJourney(Guid patientId, CancellationToken cancellationToken)
    {
        return Ok(await _referrals.GetPatientJourneyAsync(patientId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CreateReferralsResultDto>> Create(
        [FromBody] CreateReferralsRequest request,
        CancellationToken cancellationToken)
    {
        if (!await HasModuleAsync(request.SourceModule, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var created = await _referrals.CreateAsync(request, CurrentUserName(), cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/claim")]
    public async Task<ActionResult<ReferralQueueItemDto>> Claim(Guid id, CancellationToken cancellationToken)
    {
        if (!await CanAccessReferralAsync(id, cancellationToken))
        {
            return Forbid();
        }

        var item = await _referrals.ClaimAsync(id, CurrentUserName(), cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ReferralQueueItemDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        if (!await CanAccessReferralAsync(id, cancellationToken))
        {
            return Forbid();
        }

        var item = await _referrals.CompleteAsync(id, CurrentUserName(), cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{id:guid}/return-to-queue")]
    public async Task<ActionResult<ReferralQueueItemDto>> ReturnToQueue(Guid id, CancellationToken cancellationToken)
    {
        if (!await CanAccessReferralAsync(id, cancellationToken))
        {
            return Forbid();
        }

        var item = await _referrals.ReturnToQueueAsync(id, CurrentUserName(), cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{id:guid}/reassign")]
    public async Task<ActionResult<ReferralQueueItemDto>> Reassign(
        Guid id,
        [FromBody] ReassignReferralRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessReferralAsync(id, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var item = await _referrals.ReassignAsync(id, request, CurrentUserName(), cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/mark-read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await _referrals.MarkReadAsync(userId.Value, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var modules = await GetAccessibleReferralModulesAsync(cancellationToken);
        await _referrals.MarkAllReadAsync(userId.Value, modules, cancellationToken);
        return NoContent();
    }

    private Guid? CurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private string CurrentUserName()
        => User.FindFirstValue("full_name")
           ?? User.FindFirstValue(ClaimTypes.Name)
           ?? User.Identity?.Name
           ?? "System";

    private async Task<bool> CanAccessReferralAsync(Guid id, CancellationToken cancellationToken)
    {
        var targetModule = await _db.Referrals.AsNoTracking()
            .Where(r => r.Id == id && !r.Archived)
            .Select(r => (AppModule?)r.TargetModule)
            .FirstOrDefaultAsync(cancellationToken);

        return targetModule is not null && await HasModuleAsync(targetModule.Value, cancellationToken);
    }

    private async Task<bool> HasModuleAsync(AppModule module, CancellationToken cancellationToken)
    {
        var modules = await GetAccessibleModulesAsync(cancellationToken);
        return modules.Contains(module);
    }

    private async Task<List<AppModule>> GetAccessibleModulesAsync(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return [];
        }

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive && !u.Archived)
            .Select(u => new
            {
                IsAdmin = u.Role != null && u.Role.IsSystem,
                Modules = u.Role!.ModuleAccess.Select(m => m.Module)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return [];
        }

        return user.IsAdmin ? Enum.GetValues<AppModule>().ToList() : user.Modules.ToList();
    }

    private async Task<List<AppModule>> GetAccessibleReferralModulesAsync(CancellationToken cancellationToken)
    {
        var modules = await GetAccessibleModulesAsync(cancellationToken);
        return modules
            .Where(m => m is AppModule.Consultations or AppModule.Pharmacy or AppModule.Laboratory
                or AppModule.Dental or AppModule.Ancillary or AppModule.Optometrists or AppModule.Ophthalmologists)
            .ToList();
    }
}
