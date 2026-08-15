using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Renaissance.Web.Models;
using Renaissance.Web.Models.Enums;
using Renaissance.Web.ViewModels;

namespace Renaissance.Web.Services;

public class RenaissanceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public RenaissanceApiClient(HttpClient http)
    {
        _http = http;
    }

    // ----- Auth -----

    public async Task<LoginResponse> LoginAsync(string userName, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            UserName = userName,
            Password = password
        }, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = (await response.Content.ReadAsStringAsync(ct)).Trim().Trim('"');
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body) ? "Invalid username or password." : body);
        }

        return (await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, ct))!;
    }

    public Task<CurrentUserDto?> GetMeAsync(CancellationToken ct = default)
        => GetOrDefaultAsync<CurrentUserDto>("api/auth/me", ct);

    public Task<List<UserDto>> GetUsersAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<UserDto>>("api/users", ct);

    public Task<UserDto?> GetUserAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<UserDto>($"api/users/{id}", ct);

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/users", request, JsonOptions, ct);
        await EnsureSuccessWithMessageAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions, ct))!;
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{id}", request, JsonOptions, ct);
        await EnsureSuccessWithMessageAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions, ct))!;
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/users/{id}", ct);
        await EnsureSuccessWithMessageAsync(response, ct);
    }

    public Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<RoleDto>>("api/roles", ct);

    public Task<RoleDto?> GetRoleAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<RoleDto>($"api/roles/{id}", ct);

    public Task<List<ModuleDescriptorDto>> GetModuleCatalogAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<ModuleDescriptorDto>>("api/roles/modules", ct);

    public async Task<RoleDto> CreateRoleAsync(SaveRoleRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/roles", request, JsonOptions, ct);
        await EnsureSuccessWithMessageAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<RoleDto>(JsonOptions, ct))!;
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid id, SaveRoleRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/roles/{id}", request, JsonOptions, ct);
        await EnsureSuccessWithMessageAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<RoleDto>(JsonOptions, ct))!;
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/roles/{id}", ct);
        await EnsureSuccessWithMessageAsync(response, ct);
    }

    // ----- Patients -----

    public async Task<List<Patient>> GetPatientsAsync(string? q = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(q)
            ? "api/patients"
            : $"api/patients?q={Uri.EscapeDataString(q.Trim())}";
        return await GetRequiredAsync<List<Patient>>(url, ct);
    }

    public Task<Patient?> GetPatientAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Patient>($"api/patients/{id}", ct);

    public async Task<string> PeekNextClientNumberAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/patients/next-client-number", ct);
        response.EnsureSuccessStatusCode();
        var value = (await response.Content.ReadAsStringAsync(ct)).Trim();
        // API may return plain text (ACH0001) or a JSON-encoded string ("ACH0001")
        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
        {
            value = value[1..^1];
        }

        return value;
    }

    public async Task<Patient> CreatePatientAsync(Patient patient, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/patients", patient, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Patient>(JsonOptions, ct))!;
    }

    public async Task<Patient> UpdatePatientAsync(Guid id, Patient patient, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/patients/{id}", patient, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Patient>(JsonOptions, ct))!;
    }

    public async Task DeletePatientAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/patients/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> PatientExistsAsync(Guid patientId, CancellationToken ct = default)
        => await GetPatientAsync(patientId, ct) is not null;

    // ----- Dashboard -----

    public async Task<StakeholdersDashboardViewModel?> GetStakeholdersDashboardAsync(CancellationToken ct = default)
        => await GetOrDefaultAsync<StakeholdersDashboardViewModel>("api/stakeholders-dashboard", ct);

    public async Task<ClientDashboardViewModel?> GetDashboardAsync(Guid patientId, CancellationToken ct = default)
    {
        var dto = await GetOrDefaultAsync<ClientDashboardApiDto>($"api/client-dashboard/{patientId}", ct);
        if (dto?.Patient is null)
        {
            return null;
        }

        return new ClientDashboardViewModel
        {
            Patient = dto.Patient,
            Triages = dto.Triages,
            Consultations = dto.Consultations,
            Prescriptions = dto.PharmacyPrescriptions,
            Laboratories = dto.Laboratories,
            DentalConsultations = dto.DentalConsultations,
            Ancillaries = dto.Ancillaries,
            Optometrists = dto.Optometrists,
            Ophthalmologists = dto.Ophthalmologists,
            HasPendingPharmacy = dto.HasPendingPharmacy
        };
    }

    // ----- Referrals -----

    public Task<List<ReferralQueueItem>> GetReferralQueueAsync(AppModule module, CancellationToken ct = default)
        => GetRequiredAsync<List<ReferralQueueItem>>($"api/referrals/queue/{module}", ct);

    public Task<int> GetReferralPendingCountAsync(AppModule module, CancellationToken ct = default)
        => GetRequiredAsync<int>($"api/referrals/pending-count/{module}", ct);

    public async Task<CreateReferralsResult> CreateReferralsAsync(CreateReferralsRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/referrals", request, JsonOptions, ct);
        await EnsureSuccessWithMessageAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<CreateReferralsResult>(JsonOptions, ct))!;
    }

    public async Task<ReferralQueueItem> ReturnReferralToQueueAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/referrals/{id}/return-to-queue", null, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReferralQueueItem>(JsonOptions, ct))!;
    }

    public async Task<ReferralQueueItem> ReassignReferralAsync(Guid id, ReassignReferralRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/referrals/{id}/reassign", request, JsonOptions, ct);
        await EnsureSuccessWithMessageAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ReferralQueueItem>(JsonOptions, ct))!;
    }

    public Task<List<ModuleReferralCount>> GetReferralModuleCountsAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<ModuleReferralCount>>("api/referrals/module-counts", ct);

    public Task<List<ReferralInboxItem>> GetReferralInboxAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<ReferralInboxItem>>("api/referrals/inbox", ct);

    public Task<List<PatientJourneyStep>> GetPatientJourneyAsync(Guid patientId, CancellationToken ct = default)
        => GetRequiredAsync<List<PatientJourneyStep>>($"api/referrals/journey/{patientId}", ct);

    public async Task MarkReferralReadAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/referrals/{id}/mark-read", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllReferralsReadAsync(CancellationToken ct = default)
    {
        var response = await _http.PostAsync("api/referrals/mark-all-read", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ReferralQueueItem> ClaimReferralAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/referrals/{id}/claim", null, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReferralQueueItem>(JsonOptions, ct))!;
    }

    public async Task<ReferralQueueItem> CompleteReferralAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/referrals/{id}/complete", null, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReferralQueueItem>(JsonOptions, ct))!;
    }

    // ----- Triage -----

    public Task<List<Triage>> GetTriagesAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<Triage>>("api/triage", ct);

    public Task<Triage?> GetTriageAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Triage>($"api/triage/{id}", ct);

    public Task<List<Triage>> GetTriagesByPatientAsync(Guid patientId, CancellationToken ct = default)
        => GetRequiredAsync<List<Triage>>($"api/triage/by-patient/{patientId}", ct);

    public async Task<Triage> CreateTriageAsync(Triage triage, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/triage", triage, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Triage>(JsonOptions, ct))!;
    }

    public async Task<Triage> UpdateTriageAsync(Guid id, Triage triage, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/triage/{id}", triage, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Triage>(JsonOptions, ct))!;
    }

    public async Task DeleteTriageAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/triage/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Consultations -----

    public Task<List<Consultation>> GetConsultationsAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<Consultation>>("api/consultations", ct);

    public Task<Consultation?> GetConsultationAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Consultation>($"api/consultations/{id}", ct);

    public async Task<Consultation> CreateConsultationAsync(Consultation consultation, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/consultations", consultation, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Consultation>(JsonOptions, ct))!;
    }

    public async Task<Consultation> UpdateConsultationAsync(Guid id, Consultation consultation, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/consultations/{id}", consultation, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Consultation>(JsonOptions, ct))!;
    }

    public async Task UpdateItnDispenseAsync(Guid id, bool itnDispense, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(itnDispense, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/consultations/{id}/itn-dispense")
        {
            Content = content
        };
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteConsultationAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/consultations/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Pharmacy -----

    public Task<List<PharmacyPrescription>> GetPharmacyPrescriptionsAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<PharmacyPrescription>>("api/pharmacy", ct);

    public Task<PharmacyPrescription?> GetPharmacyAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<PharmacyPrescription>($"api/pharmacy/{id}", ct);

    public Task<List<PharmacyPrescription>> GetPharmacyByPatientAsync(Guid patientId, CancellationToken ct = default)
        => GetRequiredAsync<List<PharmacyPrescription>>($"api/pharmacy/by-patient/{patientId}", ct);

    public async Task<PharmacyPrescription> CreatePharmacyAsync(PharmacyPrescription prescription, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/pharmacy", prescription, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PharmacyPrescription>(JsonOptions, ct))!;
    }

    public async Task DispensePharmacyAsync(IEnumerable<PharmacyDispenseUpdate> updates, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync("api/pharmacy/dispense", updates, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePharmacyAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/pharmacy/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Laboratory -----

    public Task<List<Laboratory>> GetLaboratoriesAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<Laboratory>>("api/laboratory", ct);

    public Task<Laboratory?> GetLaboratoryAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Laboratory>($"api/laboratory/{id}", ct);

    public async Task<Laboratory> CreateLaboratoryAsync(Laboratory laboratory, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/laboratory", laboratory, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Laboratory>(JsonOptions, ct))!;
    }

    public async Task<Laboratory> UpdateLaboratoryAsync(Guid id, Laboratory laboratory, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/laboratory/{id}", laboratory, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Laboratory>(JsonOptions, ct))!;
    }

    public async Task DeleteLaboratoryAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/laboratory/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Dental -----

    public Task<List<DentalConsultation>> GetDentalsAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<DentalConsultation>>("api/dental-consultations", ct);

    public Task<DentalConsultation?> GetDentalAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<DentalConsultation>($"api/dental-consultations/{id}", ct);

    public async Task<DentalConsultation> CreateDentalAsync(DentalConsultation dental, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/dental-consultations", dental, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DentalConsultation>(JsonOptions, ct))!;
    }

    public async Task<DentalConsultation> UpdateDentalAsync(Guid id, DentalConsultation dental, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/dental-consultations/{id}", dental, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DentalConsultation>(JsonOptions, ct))!;
    }

    public async Task DeleteDentalAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/dental-consultations/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Ancillary -----

    public Task<List<Ancillary>> GetAncillariesAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<Ancillary>>("api/ancillary-services", ct);

    public Task<Ancillary?> GetAncillaryAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Ancillary>($"api/ancillary-services/{id}", ct);

    public async Task<Ancillary> CreateAncillaryAsync(Ancillary ancillary, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/ancillary-services", ancillary, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Ancillary>(JsonOptions, ct))!;
    }

    public async Task<Ancillary> UpdateAncillaryAsync(Guid id, Ancillary ancillary, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/ancillary-services/{id}", ancillary, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Ancillary>(JsonOptions, ct))!;
    }

    public async Task DeleteAncillaryAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/ancillary-services/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Optometrists -----

    public Task<List<Optometrist>> GetOptometristsAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<Optometrist>>("api/optometrists", ct);

    public Task<Optometrist?> GetOptometristAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Optometrist>($"api/optometrists/{id}", ct);

    public async Task<Optometrist> CreateOptometristAsync(Optometrist optometrist, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/optometrists", optometrist, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Optometrist>(JsonOptions, ct))!;
    }

    public async Task<Optometrist> UpdateOptometristAsync(Guid id, Optometrist optometrist, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/optometrists/{id}", optometrist, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Optometrist>(JsonOptions, ct))!;
    }

    public async Task DeleteOptometristAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/optometrists/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Ophthalmologists -----

    public Task<List<Ophthalmologist>> GetOphthalmologistsAsync(CancellationToken ct = default)
        => GetRequiredAsync<List<Ophthalmologist>>("api/ophthalmologists", ct);

    public Task<Ophthalmologist?> GetOphthalmologistAsync(Guid id, CancellationToken ct = default)
        => GetOrDefaultAsync<Ophthalmologist>($"api/ophthalmologists/{id}", ct);

    public async Task<Ophthalmologist> CreateOphthalmologistAsync(Ophthalmologist ophthalmologist, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/ophthalmologists", ophthalmologist, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Ophthalmologist>(JsonOptions, ct))!;
    }

    public async Task<Ophthalmologist> UpdateOphthalmologistAsync(Guid id, Ophthalmologist ophthalmologist, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/ophthalmologists/{id}", ophthalmologist, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Ophthalmologist>(JsonOptions, ct))!;
    }

    public async Task DeleteOphthalmologistAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/ophthalmologists/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ----- Helpers -----

    private static async Task EnsureSuccessWithMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = (await response.Content.ReadAsStringAsync(ct)).Trim().Trim('"');
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
            ? $"Request failed ({(int)response.StatusCode})."
            : body);
    }

    private async Task<T> GetRequiredAsync<T>(string url, CancellationToken ct)
    {
        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        return value ?? throw new InvalidOperationException($"Empty response from {url}");
    }

    private async Task<T?> GetOrDefaultAsync<T>(string url, CancellationToken ct) where T : class
    {
        var response = await _http.GetAsync(url, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private sealed class ClientDashboardApiDto
    {
        public Patient? Patient { get; set; }
        public List<Triage> Triages { get; set; } = [];
        public List<Consultation> Consultations { get; set; } = [];
        public List<DentalConsultation> DentalConsultations { get; set; } = [];
        public List<Laboratory> Laboratories { get; set; } = [];
        public List<PharmacyPrescription> PharmacyPrescriptions { get; set; } = [];
        public List<Ancillary> Ancillaries { get; set; } = [];
        public List<Optometrist> Optometrists { get; set; } = [];
        public List<Ophthalmologist> Ophthalmologists { get; set; } = [];
        public bool HasPendingPharmacy { get; set; }
    }
}

public class PharmacyDispenseUpdate
{
    public Guid Id { get; set; }
    public DispensationStatus Status { get; set; }
    public int? QuantityDispensed { get; set; }
    public string? DispensationNote { get; set; }
    public bool Dispensed { get; set; }
}
