namespace Renaissance.Web.Models;

public enum ExportRecordModule
{
    Clients = 1,
    Triage = 2,
    Consultations = 3,
    Pharmacy = 4,
    Laboratory = 5,
    Dental = 6,
    Ancillary = 7,
    Optometrists = 8,
    Ophthalmologists = 9,
    Referrals = 10
}

public class ExportModuleInfoDto
{
    public ExportRecordModule Module { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ExportRecordsRequest
{
    public List<ExportRecordModule> Modules { get; set; } = [];
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeArchived { get; set; }
    public string? ClientSearch { get; set; }
}

public class ExportModuleCountDto
{
    public ExportRecordModule Module { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ExportPreviewDto
{
    public List<ExportModuleCountDto> Modules { get; set; } = [];
    public int TotalRecords { get; set; }
}
