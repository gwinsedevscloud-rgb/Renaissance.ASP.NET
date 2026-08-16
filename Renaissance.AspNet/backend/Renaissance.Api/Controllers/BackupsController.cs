using Microsoft.AspNetCore.Mvc;
using Renaissance.Api.Authorization;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Controllers;

[ApiController]
[Route("api/backups")]
[RequireModule(AppModule.Administration)]
public class BackupsController : ControllerBase
{
    private readonly IBackupService _backups;

    public BackupsController(IBackupService backups)
    {
        _backups = backups;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BackupInfoDto>>> List(CancellationToken cancellationToken)
        => Ok(await _backups.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BackupCreateResultDto>> Create(CancellationToken cancellationToken)
    {
        var createdBy = User.Identity?.Name ?? "admin";
        var result = await _backups.CreateAsync(createdBy, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{fileName}/download")]
    public async Task<IActionResult> Download(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _backups.OpenReadAsync(fileName, cancellationToken);
            if (file is null)
            {
                return NotFound();
            }

            return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Invalid backup file name.");
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> Delete(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            await _backups.DeleteAsync(fileName, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Invalid backup file name.");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("restore")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<ActionResult<BackupRestoreResultDto>> Restore(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Choose a backup file to restore.");
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Backup files must be .zip archives exported from Renaissance.");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var restoredBy = User.Identity?.Name ?? "admin";
            var result = await _backups.RestoreAsync(stream, restoredBy, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
