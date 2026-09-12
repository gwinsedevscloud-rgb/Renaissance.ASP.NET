using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;
using Renaissance.Infrastructure.Persistence;
using System.Security.Claims;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/secondary-outreach")]
public class SecondaryOutreachController : ControllerBase
{
    private readonly ISecondaryOutreachService _secondary;
    private readonly RenaissanceDbContext _db;

    public SecondaryOutreachController(ISecondaryOutreachService secondary, RenaissanceDbContext db)
    {
        _secondary = secondary;
        _db = db;
    }

    [HttpGet("accessible")]
    [Authorize]
    public async Task<ActionResult<List<CareProgramSummaryDto>>> GetAccessible(CancellationToken cancellationToken)
    {
        var (userId, isAdmin) = await GetUserContextAsync(cancellationToken);
        return Ok(await _secondary.GetAccessibleProgramsAsync(userId, isAdmin, cancellationToken));
    }

    [HttpGet("{programId:guid}/registrations")]
    [Authorize]
    public async Task<ActionResult<List<SecondaryOutreachRegistrationDto>>> GetRegistrations(
        Guid programId,
        CancellationToken cancellationToken)
        => Ok(await _secondary.GetRegistrationsAsync(programId, cancellationToken));

    [HttpGet("{programId:guid}/enrolled")]
    [Authorize]
    public async Task<ActionResult<List<SecondaryOutreachEnrollmentDto>>> GetEnrolled(
        Guid programId,
        CancellationToken cancellationToken)
    {
        try
        {
            var (userId, isAdmin) = await GetUserContextAsync(cancellationToken);
            return Ok(await _secondary.GetEnrolledAsync(programId, userId, isAdmin, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult<SecondaryOutreachRegistrationDto>> Register(
        [FromBody] RegisterSecondaryOutreachRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var (userId, isAdmin) = await GetUserContextAsync(cancellationToken);
            var result = await _secondary.RegisterAsync(
                request,
                userId,
                isAdmin,
                User.Identity?.Name ?? "user",
                cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{programId:guid}/staff")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<ActionResult<List<CareProgramStaffMemberDto>>> GetStaff(
        Guid programId,
        CancellationToken cancellationToken)
        => Ok(await _secondary.GetStaffAsync(programId, cancellationToken));

    [HttpPut("{programId:guid}/staff")]
    [RequireModule(AppModule.CarePrograms)]
    public async Task<IActionResult> UpdateStaff(
        Guid programId,
        [FromBody] UpdateCareProgramStaffRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _secondary.UpdateStaffAsync(programId, request, User.Identity?.Name ?? "admin", cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<(Guid UserId, bool IsAdmin)> GetUserContextAsync(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User id missing.");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new InvalidOperationException("User id invalid.");
        }

        var isAdmin = await _db.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId && !u.Archived && u.IsActive && u.Role != null && u.Role.IsSystem, cancellationToken);

        return (userId, isAdmin);
    }
}
