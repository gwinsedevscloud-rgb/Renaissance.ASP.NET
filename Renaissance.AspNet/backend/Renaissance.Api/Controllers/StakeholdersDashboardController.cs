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

    public StakeholdersDashboardController(IStakeholdersDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    public async Task<ActionResult<StakeholdersDashboardDto>> Get(CancellationToken cancellationToken)
    {
        var dto = await _dashboard.GetAsync(cancellationToken);
        return Ok(dto);
    }
}
