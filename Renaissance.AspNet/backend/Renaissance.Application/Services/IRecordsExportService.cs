using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface IRecordsExportService
{
    IReadOnlyList<ExportModuleInfoDto> GetModules();

    Task<ExportPreviewDto> PreviewAsync(ExportRecordsRequest request, CancellationToken cancellationToken = default);

    Task<ExportFileResult> ExportAsync(ExportRecordsRequest request, string exportedBy, CancellationToken cancellationToken = default);
}
