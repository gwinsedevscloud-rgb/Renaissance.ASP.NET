using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/exports")]
[RequireModule(AppModule.Administration)]
public class ExportsController : ControllerBase
{
    private readonly IRecordsExportService _exports;

    public ExportsController(IRecordsExportService exports)
    {
        _exports = exports;
    }

    [HttpGet("modules")]
    public ActionResult<IReadOnlyList<ExportModuleInfoDto>> Modules()
        => Ok(_exports.GetModules());

    [HttpPost("preview")]
    public async Task<ActionResult<ExportPreviewDto>> Preview([FromBody] ExportRecordsRequest request, CancellationToken cancellationToken)
        => Ok(await _exports.PreviewAsync(request, cancellationToken));

    [HttpPost("download")]
    public async Task<IActionResult> Download([FromBody] ExportRecordsRequest request, CancellationToken cancellationToken)
    {
        var exportedBy = User.Identity?.Name ?? "admin";
        var file = await _exports.ExportAsync(request, exportedBy, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
