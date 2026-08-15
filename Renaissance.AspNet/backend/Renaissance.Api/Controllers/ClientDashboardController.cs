using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/client-dashboard")]
[RequireModule(AppModule.ClientDashboard)]
public class ClientDashboardController : ControllerBase
{
    private readonly IClientDashboardService _dashboard;

    public ClientDashboardController(IClientDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("{patientId:guid}")]
    public async Task<ActionResult<ClientDashboardDto>> Get(Guid patientId, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _dashboard.GetAsync(patientId, cancellationToken);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
