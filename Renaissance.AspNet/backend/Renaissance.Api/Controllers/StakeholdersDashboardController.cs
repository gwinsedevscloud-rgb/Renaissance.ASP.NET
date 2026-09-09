using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/stakeholders-dashboard")]
[RequireModule(AppModule.Stakeholders)]
public class StakeholdersDashboardController : ControllerBase
{
    private readonly IStakeholdersDashboardService _dashboard;
    private readonly ISecondaryOutreachService _outreach;

    public StakeholdersDashboardController(
        IStakeholdersDashboardService dashboard,
        ISecondaryOutreachService outreach)
    {
        _dashboard = dashboard;
        _outreach = outreach;
    }

    [HttpGet]
    public async Task<ActionResult<StakeholdersDashboardDto>> Get(CancellationToken cancellationToken)
    {
        var dto = await _dashboard.GetAsync(cancellationToken);
        return Ok(dto);
    }

    [HttpGet("outreach-overview")]
    public async Task<ActionResult<StakeholdersOutreachOverviewDto>> GetOutreachOverview(CancellationToken cancellationToken)
    {
        return Ok(new StakeholdersOutreachOverviewDto
        {
            GeneratedAt = DateTime.UtcNow,
            FacilityOverview = await _dashboard.GetAsync(cancellationToken),
            OutreachDashboards = await _outreach.GetAllOutreachDashboardsAsync(cancellationToken)
        });
    }

    [HttpGet("outreach/{programId:guid}")]
    public async Task<ActionResult<OutreachStakeholdersDashboardDto>> GetOutreachDashboard(
        Guid programId,
        CancellationToken cancellationToken)
    {
        var dto = await _outreach.GetStakeholdersDashboardAsync(programId, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
