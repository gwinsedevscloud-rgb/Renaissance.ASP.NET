using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Services;

public sealed class RecordsExportService : IRecordsExportService
{
    private const string HeaderColor = "#0D6E6E";
    private const string AccentColor = "#E6F4F4";

    private static readonly IReadOnlyList<ExportModuleInfoDto> ModuleCatalog =
    [
        new() { Module = ExportRecordModule.Clients, Name = "Patients", Description = "Registered outpatient patients" },
        new() { Module = ExportRecordModule.Triage, Name = "Triage", Description = "Vitals and medical history" },
        new() { Module = ExportRecordModule.Consultations, Name = "Consultations", Description = "Diagnosis, treatment, referrals, ITN" },
        new() { Module = ExportRecordModule.Pharmacy, Name = "Pharmacy", Description = "Prescriptions and dispensation" },
        new() { Module = ExportRecordModule.Laboratory, Name = "Laboratory", Description = "Tests and results" },
        new() { Module = ExportRecordModule.Dental, Name = "Dental", Description = "Dental consultations" },
        new() { Module = ExportRecordModule.Ancillary, Name = "Ancillary", Description = "Ancillary services" },
        new() { Module = ExportRecordModule.Optometrists, Name = "Optometrists", Description = "Optometry exams" },
        new() { Module = ExportRecordModule.Ophthalmologists, Name = "Ophthalmologists", Description = "Ophthalmology care" },
        new() { Module = ExportRecordModule.Referrals, Name = "Referrals", Description = "Cross-module referral queue history" }
    ];

    private readonly IApplicationDbContext _db;

    public RecordsExportService(IApplicationDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<ExportModuleInfoDto> GetModules() => ModuleCatalog;

    public async Task<ExportPreviewDto> PreviewAsync(ExportRecordsRequest request, CancellationToken cancellationToken = default)
    {
        var modules = ResolveModules(request);
        var context = await BuildContextAsync(request, cancellationToken);
        var counts = new List<ExportModuleCountDto>();

        foreach (var module in modules)
        {
            var count = await CountModuleAsync(module, context, cancellationToken);
            counts.Add(new ExportModuleCountDto
            {
                Module = module,
                Name = ModuleCatalog.First(m => m.Module == module).Name,
                Count = count
            });
        }

        return new ExportPreviewDto
        {
            Modules = counts,
            TotalRecords = counts.Sum(c => c.Count)
        };
    }

    public async Task<ExportFileResult> ExportAsync(ExportRecordsRequest request, string exportedBy, CancellationToken cancellationToken = default)
    {
        var modules = ResolveModules(request);
        var context = await BuildContextAsync(request, cancellationToken);
        var preview = new List<ExportModuleCountDto>();

        using var workbook = new XLWorkbook();
        workbook.Properties.Author = exportedBy;
        workbook.Properties.Title = "Renaissance Clinical Export";
        workbook.Properties.Subject = "Filtered module records export";

        var summary = workbook.Worksheets.Add("Summary");
        WriteSummarySheet(summary, request, exportedBy, modules);

        foreach (var module in modules)
        {
            var count = await AddModuleSheetAsync(workbook, module, context, cancellationToken);
            preview.Add(new ExportModuleCountDto
            {
                Module = module,
                Name = ModuleCatalog.First(m => m.Module == module).Name,
                Count = count
            });
        }

        WriteSummaryCounts(summary, preview);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var facility = await _db.HospitalSettings.AsNoTracking()
            .Select(s => s.FacilityName)
            .FirstOrDefaultAsync(cancellationToken);

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");
        var prefix = string.IsNullOrWhiteSpace(facility) ? "renaissance" : Slugify(facility);
        var fileName = $"{prefix}-export-{stamp}.xlsx";

        return new ExportFileResult(
            stream.ToArray(),
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static List<ExportRecordModule> ResolveModules(ExportRecordsRequest request)
        => request.Modules.Count == 0
            ? ModuleCatalog.Select(m => m.Module).ToList()
            : request.Modules.Distinct().OrderBy(m => (int)m).ToList();

    private async Task<ExportContext> BuildContextAsync(ExportRecordsRequest request, CancellationToken cancellationToken)
    {
        var patientQuery = _db.Patients.AsNoTracking();
        if (!request.IncludeArchived)
        {
            patientQuery = patientQuery.Where(p => !p.Archived);
        }

        if (!string.IsNullOrWhiteSpace(request.ClientSearch))
        {
            var term = request.ClientSearch.Trim();
            patientQuery = patientQuery.Where(p =>
                p.ClientNumber.Contains(term) ||
                p.FullName.Contains(term));
        }

        var patients = await patientQuery.ToDictionaryAsync(p => p.Id, cancellationToken);
        var patientIds = patients.Keys.ToList();
        var toDate = request.ToDate?.Date.AddDays(1).AddTicks(-1);

        return new ExportContext(request, patients, patientIds, toDate);
    }

    private async Task<int> CountModuleAsync(ExportRecordModule module, ExportContext context, CancellationToken cancellationToken)
        => module switch
        {
            ExportRecordModule.Clients => await FilterPatientsQuery(context).CountAsync(cancellationToken),
            ExportRecordModule.Triage => await FilterClinical(_db.Triages.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Consultations => await FilterClinical(_db.Consultations.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Pharmacy => await FilterClinical(_db.PharmacyPrescriptions.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Laboratory => await FilterClinical(_db.Laboratories.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Dental => await FilterClinical(_db.DentalConsultations.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Ancillary => await FilterClinical(_db.Ancillaries.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Optometrists => await FilterClinical(_db.Optometrists.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Ophthalmologists => await FilterClinical(_db.Ophthalmologists.AsNoTracking(), context).CountAsync(cancellationToken),
            ExportRecordModule.Referrals => await FilterClinical(_db.Referrals.AsNoTracking(), context).CountAsync(cancellationToken),
            _ => 0
        };

    private async Task<int> AddModuleSheetAsync(XLWorkbook workbook, ExportRecordModule module, ExportContext context, CancellationToken cancellationToken)
        => module switch
        {
            ExportRecordModule.Clients => await WriteClientsSheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Triage => await WriteTriageSheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Consultations => await WriteConsultationsSheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Pharmacy => await WritePharmacySheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Laboratory => await WriteLaboratorySheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Dental => await WriteDentalSheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Ancillary => await WriteAncillarySheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Optometrists => await WriteOptometristsSheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Ophthalmologists => await WriteOphthalmologistsSheetAsync(workbook, context, cancellationToken),
            ExportRecordModule.Referrals => await WriteReferralsSheetAsync(workbook, context, cancellationToken),
            _ => 0
        };

    private IQueryable<Patient> FilterPatientsQuery(ExportContext context)
    {
        var query = _db.Patients.AsNoTracking();

        if (!context.Request.IncludeArchived)
        {
            query = query.Where(p => !p.Archived);
        }

        if (!string.IsNullOrWhiteSpace(context.Request.ClientSearch))
        {
            var term = context.Request.ClientSearch.Trim();
            query = query.Where(p => p.ClientNumber.Contains(term) || p.FullName.Contains(term));
        }

        if (context.Request.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedDate >= context.Request.FromDate);
        }

        if (context.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedDate <= context.ToDate);
        }

        return query.OrderBy(p => p.ClientNumber);
    }

    private IQueryable<T> FilterClinical<T>(IQueryable<T> query, ExportContext context) where T : AuditEntity
    {
        if (!context.Request.IncludeArchived)
        {
            query = query.Where(e => !e.Archived);
        }

        if (context.Request.FromDate.HasValue)
        {
            query = query.Where(e => e.CreatedDate >= context.Request.FromDate);
        }

        if (context.ToDate.HasValue)
        {
            query = query.Where(e => e.CreatedDate <= context.ToDate);
        }

        if (context.PatientIds.Count > 0)
        {
            query = query.Where(e => context.PatientIds.Contains(EF.Property<Guid>(e, "PatientId")));
        }
        else if (!string.IsNullOrWhiteSpace(context.Request.ClientSearch))
        {
            query = query.Where(e => false);
        }

        return query;
    }

    private async Task<int> WriteClientsSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Full Name", "Age", "Age Unit", "Sex", "Marital Status", "Tribe", "Religion",
            "Occupation", "Education", "Address", "Phone", "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Patients", headers);
        var rows = await FilterPatientsQuery(context).ToListAsync(cancellationToken);
        var row = 2;

        foreach (var p in rows)
        {
            var c = 1;
            ws.Cell(row, c++).Value = p.ClientNumber;
            ws.Cell(row, c++).Value = p.FullName;
            ws.Cell(row, c++).Value = p.Age;
            ws.Cell(row, c++).Value = p.AgeUnit;
            ws.Cell(row, c++).Value = p.Sex;
            ws.Cell(row, c++).Value = p.MaritalStatus ?? "";
            ws.Cell(row, c++).Value = p.Tribe ?? "";
            ws.Cell(row, c++).Value = p.Religion ?? "";
            ws.Cell(row, c++).Value = p.Occupation ?? "";
            ws.Cell(row, c++).Value = p.Education ?? "";
            ws.Cell(row, c++).Value = p.Address ?? "";
            ws.Cell(row, c++).Value = p.PhoneNumber ?? "";
            WriteAuditCells(ws, row, ref c, p);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteTriageSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Diabetes", "Asthma", "Sickle Cell", "Smoking", "Weight (kg)", "Height (cm)",
            "Temperature (°C)", "Systolic BP", "Diastolic BP", "Pulse", "Respiratory Rate",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Triage", headers);
        var rows = await FilterClinical(_db.Triages.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = FormatBool(item.Diabetes);
            ws.Cell(row, c++).Value = FormatBool(item.Asthma);
            ws.Cell(row, c++).Value = FormatBool(item.SickleCell);
            ws.Cell(row, c++).Value = FormatBool(item.Smoking);
            ws.Cell(row, c++).Value = item.Weight;
            ws.Cell(row, c++).Value = item.Height;
            ws.Cell(row, c++).Value = item.Temperature;
            ws.Cell(row, c++).Value = item.SystolicBp;
            ws.Cell(row, c++).Value = item.DiastolicBp;
            ws.Cell(row, c++).Value = item.PulseRate;
            ws.Cell(row, c++).Value = item.RespiratoryRate;
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteConsultationsSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Diagnoses", "Treatments", "Services Referred", "Other Diagnoses", "Other Treatments",
            "Referred", "ITN Ordered", "ITN Dispensed", "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Consultations", headers);
        var rows = await FilterClinical(_db.Consultations.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = JoinList(item.Diagnoses);
            ws.Cell(row, c++).Value = JoinList(item.Treatments);
            ws.Cell(row, c++).Value = JoinList(item.ServicesReferred);
            ws.Cell(row, c++).Value = JoinList(item.OthersDiagnosis);
            ws.Cell(row, c++).Value = JoinList(item.OthersTreatment);
            ws.Cell(row, c++).Value = FormatBool(item.Referred);
            ws.Cell(row, c++).Value = FormatBool(item.ItnOrder);
            ws.Cell(row, c++).Value = FormatBool(item.ItnDispense);
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WritePharmacySheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Drug Category", "Drug Name", "Dosage", "Frequency", "Duration",
            "Status", "Dispensed", "Quantity Dispensed", "Dispensation Note",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Pharmacy", headers);
        var rows = await FilterClinical(_db.PharmacyPrescriptions.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = item.DrugCategory ?? "";
            ws.Cell(row, c++).Value = item.DrugName ?? "";
            ws.Cell(row, c++).Value = item.Dosage ?? "";
            ws.Cell(row, c++).Value = item.Frequency ?? "";
            ws.Cell(row, c++).Value = item.Duration ?? "";
            ws.Cell(row, c++).Value = item.Status.ToString();
            ws.Cell(row, c++).Value = item.Dispensed ? "Yes" : "No";
            ws.Cell(row, c++).Value = item.QuantityDispensed;
            ws.Cell(row, c++).Value = item.DispensationNote ?? "";
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteLaboratorySheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Test Name", "Result", "Note",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Laboratory", headers);
        var rows = await FilterClinical(_db.Laboratories.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = item.TestName ?? "";
            ws.Cell(row, c++).Value = item.Result ?? "";
            ws.Cell(row, c++).Value = item.Note ?? "";
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteDentalSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Diagnoses", "Treatments", "Dispensed Items", "Services Referred",
            "Other Diagnoses", "Other Treatments", "Referred",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Dental", headers);
        var rows = await FilterClinical(_db.DentalConsultations.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = JoinList(item.Diagnoses);
            ws.Cell(row, c++).Value = JoinList(item.Treatments);
            ws.Cell(row, c++).Value = JoinList(item.DispensedItems);
            ws.Cell(row, c++).Value = JoinList(item.ServicesReferred);
            ws.Cell(row, c++).Value = JoinList(item.OthersDiagnosis);
            ws.Cell(row, c++).Value = JoinList(item.OthersTreatment);
            ws.Cell(row, c++).Value = FormatBool(item.Referred);
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteAncillarySheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Services", "Pregnancy Status",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Ancillary", headers);
        var rows = await FilterClinical(_db.Ancillaries.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = JoinList(item.Services);
            ws.Cell(row, c++).Value = item.PregnancyStatus ?? "";
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteOptometristsSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "VA Right", "VA Left", "Glasses Dispensed", "Referred",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Optometrists", headers);
        var rows = await FilterClinical(_db.Optometrists.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = item.VisualAcuityRight ?? "";
            ws.Cell(row, c++).Value = item.VisualAcuityLeft ?? "";
            ws.Cell(row, c++).Value = FormatBool(item.GlassesDispensed);
            ws.Cell(row, c++).Value = FormatBool(item.Referred);
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteOphthalmologistsSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "Diagnoses", "Treatments", "Surgeries", "Other Diagnoses", "Other Treatments", "Other Surgery",
            "VA Right", "VA Left", "Glasses Dispensed", "Referred",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Ophthalmologists", headers);
        var rows = await FilterClinical(_db.Ophthalmologists.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = JoinList(item.Diagnoses);
            ws.Cell(row, c++).Value = JoinList(item.Treatments);
            ws.Cell(row, c++).Value = JoinList(item.Surgeries);
            ws.Cell(row, c++).Value = JoinList(item.OthersDiagnosis);
            ws.Cell(row, c++).Value = JoinList(item.OthersTreatment);
            ws.Cell(row, c++).Value = JoinList(item.OtherSurgery);
            ws.Cell(row, c++).Value = item.VisualAcuityRight ?? "";
            ws.Cell(row, c++).Value = item.VisualAcuityLeft ?? "";
            ws.Cell(row, c++).Value = FormatBool(item.GlassesDispensed);
            ws.Cell(row, c++).Value = FormatBool(item.Referred);
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private async Task<int> WriteReferralsSheetAsync(XLWorkbook workbook, ExportContext context, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Patient Number", "Patient Name", "From Module", "To Module", "Status", "Priority", "Notes",
            "Attended By", "Attended Date", "Completed By", "Completed Date", "Cancelled Reason",
            "Created By", "Created Date", "Updated By", "Updated Date", "Archived"
        };

        var ws = CreateSheet(workbook, "Referrals", headers);
        var rows = await FilterClinical(_db.Referrals.AsNoTracking(), context)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var row = 2;
        foreach (var item in rows)
        {
            var patient = context.Patients.GetValueOrDefault(item.PatientId);
            var c = 1;
            ws.Cell(row, c++).Value = patient?.ClientNumber ?? "";
            ws.Cell(row, c++).Value = patient?.FullName ?? "";
            ws.Cell(row, c++).Value = AppModuleCatalog.DisplayName(item.SourceModule);
            ws.Cell(row, c++).Value = AppModuleCatalog.DisplayName(item.TargetModule);
            ws.Cell(row, c++).Value = item.Status.ToString();
            ws.Cell(row, c++).Value = item.Priority.ToString();
            ws.Cell(row, c++).Value = item.Notes ?? "";
            ws.Cell(row, c++).Value = item.AttendedBy ?? "";
            ws.Cell(row, c++).Value = item.AttendedDate;
            ws.Cell(row, c++).Value = item.CompletedBy ?? "";
            ws.Cell(row, c++).Value = item.CompletedDate;
            ws.Cell(row, c++).Value = item.CancelledReason ?? "";
            WriteAuditCells(ws, row, ref c, item);
            row++;
        }

        FinalizeSheet(ws, row - 1, headers.Length);
        return rows.Count;
    }

    private static IXLWorksheet CreateSheet(XLWorkbook workbook, string name, IReadOnlyList<string> headers)
    {
        var ws = workbook.Worksheets.Add(name);
        for (var i = 0; i < headers.Count; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        StyleHeaderRow(ws, 1, headers.Count);
        return ws;
    }

    private static void FinalizeSheet(IXLWorksheet ws, int lastDataRow, int columnCount)
    {
        if (lastDataRow >= 1)
        {
            var tableRange = ws.Range(1, 1, Math.Max(lastDataRow, 1), columnCount);
            tableRange.SetAutoFilter();
            ws.SheetView.FreezeRows(1);

            if (lastDataRow >= 2)
            {
                for (var row = 2; row <= lastDataRow; row++)
                {
                    if (row % 2 == 0)
                    {
                        ws.Range(row, 1, row, columnCount).Style.Fill.BackgroundColor = XLColor.FromHtml(AccentColor);
                    }
                }
            }
        }

        ws.Columns(1, columnCount).AdjustToContents(8, 48);
        ws.Row(1).Height = 22;
    }

    private static void StyleHeaderRow(IXLWorksheet ws, int row, int columnCount)
    {
        var range = ws.Range(row, 1, row, columnCount);
        range.Style.Fill.BackgroundColor = XLColor.FromHtml(HeaderColor);
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Font.Bold = true;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    }

    private static void WriteAuditCells(IXLWorksheet ws, int row, ref int column, AuditEntity entity)
    {
        ws.Cell(row, column++).Value = entity.CreatedBy ?? "";
        ws.Cell(row, column++).Value = entity.CreatedDate;
        ws.Cell(row, column++).Value = entity.UpdatedBy ?? "";
        ws.Cell(row, column++).Value = entity.UpdatedDate;
        ws.Cell(row, column++).Value = entity.Archived ? "Yes" : "No";
    }

    private static void WriteSummarySheet(IXLWorksheet ws, ExportRecordsRequest request, string exportedBy, IReadOnlyList<ExportRecordModule> modules)
    {
        ws.Cell(1, 1).Value = "Renaissance Clinical Records Export";
        ws.Range(1, 1, 1, 3).Merge();
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(HeaderColor);

        ws.Cell(3, 1).Value = "Exported by";
        ws.Cell(3, 2).Value = exportedBy;
        ws.Cell(4, 1).Value = "Generated (UTC)";
        ws.Cell(4, 2).Value = DateTime.UtcNow;
        ws.Cell(4, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";

        ws.Cell(6, 1).Value = "Date from";
        ws.Cell(6, 2).Value = request.FromDate;
        ws.Cell(7, 1).Value = "Date to";
        ws.Cell(7, 2).Value = request.ToDate;
        ws.Cell(8, 1).Value = "Include archived";
        ws.Cell(8, 2).Value = request.IncludeArchived ? "Yes" : "No";
        ws.Cell(9, 1).Value = "Patient filter";
        ws.Cell(9, 2).Value = string.IsNullOrWhiteSpace(request.ClientSearch) ? "All patients" : request.ClientSearch.Trim();

        ws.Cell(11, 1).Value = "Modules";
        ws.Cell(11, 2).Value = string.Join(", ", modules.Select(m => ModuleCatalog.First(c => c.Module == m).Name));

        ws.Cell(13, 1).Value = "Module";
        ws.Cell(13, 2).Value = "Rows exported";
        StyleHeaderRow(ws, 13, 2);

        ws.Column(1).Width = 28;
        ws.Column(2).Width = 40;
    }

    private static void WriteSummaryCounts(IXLWorksheet ws, IReadOnlyList<ExportModuleCountDto> counts)
    {
        var row = 14;
        foreach (var item in counts)
        {
            ws.Cell(row, 1).Value = item.Name;
            ws.Cell(row, 2).Value = item.Count;
            row++;
        }

        ws.Cell(row + 1, 1).Value = "Total records";
        ws.Cell(row + 1, 1).Style.Font.Bold = true;
        ws.Cell(row + 1, 2).Value = counts.Sum(c => c.Count);
        ws.Cell(row + 1, 2).Style.Font.Bold = true;
    }

    private static string JoinList(IEnumerable<string> values)
        => string.Join("; ", values.Where(v => !string.IsNullOrWhiteSpace(v)));

    private static string FormatBool(bool? value)
        => value switch
        {
            true => "Yes",
            false => "No",
            _ => ""
        };

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_')
            .ToArray();
        var slug = new string(chars);
        return string.IsNullOrWhiteSpace(slug) ? "renaissance" : slug[..Math.Min(slug.Length, 24)];
    }

    private sealed class ExportContext(
        ExportRecordsRequest request,
        Dictionary<Guid, Patient> patients,
        List<Guid> patientIds,
        DateTime? toDate)
    {
        public ExportRecordsRequest Request { get; } = request;
        public Dictionary<Guid, Patient> Patients { get; } = patients;
        public List<Guid> PatientIds { get; } = patientIds;
        public DateTime? ToDate { get; } = toDate;
    }
}
