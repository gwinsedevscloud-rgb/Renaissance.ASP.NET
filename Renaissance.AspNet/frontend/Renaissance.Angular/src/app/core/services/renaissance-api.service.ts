import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams, HttpResponse } from '@angular/common/http';
import { map, Observable, of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    Ancillary,
    ClientDashboardDto,
    Consultation,
    DentalConsultation,
    Laboratory,
    Ophthalmologist,
    Optometrist,
    Patient,
    PharmacyDispenseUpdate,
    PharmacyPrescription,
    Triage
} from '../models/clinical.models';
import { StakeholdersDashboardDto } from '../models/stakeholders.models';
import {
    AppModule,
    CreateUserRequest,
    CurrentUserDto,
    LoginRequest,
    LoginResponse,
    ModuleDescriptorDto,
    RoleDto,
    SaveRoleRequest,
    UpdateUserRequest,
    UserDto
} from '../models/auth.models';
import {
    ChangeLanAccessPasswordRequest,
    DeploymentInfoDto,
    HospitalModulesConfigDto,
    HospitalModulesStateDto,
    HospitalSettingsDto,
    LanAccessStatusDto,
    LanAccessUnlockRequest,
    LanAccessUnlockResponse,
    LanSettingsDto,
    PublicHospitalSettingsDto,
    UpdateHospitalModulesRequest,
    UpdateHospitalSettingsRequest,
    UpdateLanSettingsRequest
} from '../models/settings.models';
import {
    CreateReferralsRequest,
    CreateReferralsResult,
    ModuleReferralCount,
    PatientJourneyStep,
    ReassignReferralRequest,
    ReferralInboxItem,
    ReferralQueueItem
} from '../models/referral.models';
import { BackupCreateResultDto, BackupInfoDto, BackupRestoreResultDto } from '../models/backup.models';
import {
    ExportDownloadResult,
    ExportModuleInfoDto,
    ExportPreviewDto,
    ExportRecordsRequest
} from '../models/export.models';

@Injectable({ providedIn: 'root' })
export class RenaissanceApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = environment.apiUrl;

    // ----- Auth -----

    login(request: LoginRequest): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, request).pipe(catchError(this.handleError));
    }

    getMe(): Observable<CurrentUserDto | null> {
        return this.getOrNull<CurrentUserDto>(`${this.baseUrl}/api/auth/me`);
    }

    getUsers(): Observable<UserDto[]> {
        return this.http.get<UserDto[]>(`${this.baseUrl}/api/users`);
    }

    getUser(id: string): Observable<UserDto | null> {
        return this.getOrNull<UserDto>(`${this.baseUrl}/api/users/${id}`);
    }

    createUser(request: CreateUserRequest): Observable<UserDto> {
        return this.http.post<UserDto>(`${this.baseUrl}/api/users`, request).pipe(catchError(this.handleError));
    }

    updateUser(id: string, request: UpdateUserRequest): Observable<UserDto> {
        return this.http.put<UserDto>(`${this.baseUrl}/api/users/${id}`, request).pipe(catchError(this.handleError));
    }

    deleteUser(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/users/${id}`).pipe(catchError(this.handleError));
    }

    getRoles(): Observable<RoleDto[]> {
        return this.http.get<RoleDto[]>(`${this.baseUrl}/api/roles`);
    }

    getRole(id: string): Observable<RoleDto | null> {
        return this.getOrNull<RoleDto>(`${this.baseUrl}/api/roles/${id}`);
    }

    getModuleCatalog(): Observable<ModuleDescriptorDto[]> {
        return this.http.get<ModuleDescriptorDto[]>(`${this.baseUrl}/api/roles/modules`);
    }

    createRole(request: SaveRoleRequest): Observable<RoleDto> {
        return this.http.post<RoleDto>(`${this.baseUrl}/api/roles`, request).pipe(catchError(this.handleError));
    }

    updateRole(id: string, request: SaveRoleRequest): Observable<RoleDto> {
        return this.http.put<RoleDto>(`${this.baseUrl}/api/roles/${id}`, request).pipe(catchError(this.handleError));
    }

    deleteRole(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/roles/${id}`).pipe(catchError(this.handleError));
    }

    // ----- Patients -----

    getPatients(q?: string): Observable<Patient[]> {
        let params = new HttpParams();
        if (q?.trim()) {
            params = params.set('q', q.trim());
        }

        return this.http.get<Patient[]>(`${this.baseUrl}/api/patients`, { params });
    }

    getPatient(id: string): Observable<Patient | null> {
        return this.getOrNull<Patient>(`${this.baseUrl}/api/patients/${id}`);
    }

    peekNextClientNumber(): Observable<string> {
        return this.http.get(`${this.baseUrl}/api/patients/next-client-number`, { responseType: 'text' }).pipe(
            map(value => {
                const trimmed = value.trim();
                if (trimmed.length >= 2 && trimmed.startsWith('"') && trimmed.endsWith('"')) {
                    return trimmed.slice(1, -1);
                }

                return trimmed;
            })
        );
    }

    /** @deprecated Use peekNextClientNumber */
    generateClientNumber(): Observable<string> {
        return this.peekNextClientNumber();
    }

    createPatient(patient: Patient): Observable<Patient> {
        return this.http.post<Patient>(`${this.baseUrl}/api/patients`, patient);
    }

    updatePatient(id: string, patient: Patient): Observable<Patient> {
        return this.http.put<Patient>(`${this.baseUrl}/api/patients/${id}`, patient);
    }

    deletePatient(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/patients/${id}`);
    }

    patientExists(patientId: string): Observable<boolean> {
        return this.getPatient(patientId).pipe(map(patient => patient !== null));
    }

    // ----- Dashboard -----

    getStakeholdersDashboard(): Observable<StakeholdersDashboardDto | null> {
        return this.getOrNull<StakeholdersDashboardDto>(`${this.baseUrl}/api/stakeholders-dashboard`);
    }

    getDashboard(patientId: string): Observable<ClientDashboardDto | null> {
        return this.getOrNull<ClientDashboardDto>(`${this.baseUrl}/api/client-dashboard/${patientId}`);
    }

    // ----- Referrals -----

    getReferralQueue(module: AppModule): Observable<ReferralQueueItem[]> {
        return this.http.get<ReferralQueueItem[]>(`${this.baseUrl}/api/referrals/queue/${module}`);
    }

    getReferralPendingCount(module: AppModule): Observable<number> {
        return this.http.get<number>(`${this.baseUrl}/api/referrals/pending-count/${module}`);
    }

    createReferrals(request: CreateReferralsRequest): Observable<CreateReferralsResult> {
        return this.http.post<CreateReferralsResult>(`${this.baseUrl}/api/referrals`, request).pipe(catchError(this.handleError));
    }

    returnReferralToQueue(id: string): Observable<ReferralQueueItem> {
        return this.http.post<ReferralQueueItem>(`${this.baseUrl}/api/referrals/${id}/return-to-queue`, null);
    }

    reassignReferral(id: string, request: ReassignReferralRequest): Observable<ReferralQueueItem> {
        return this.http.post<ReferralQueueItem>(`${this.baseUrl}/api/referrals/${id}/reassign`, request).pipe(catchError(this.handleError));
    }

    getReferralModuleCounts(): Observable<ModuleReferralCount[]> {
        return this.http.get<ModuleReferralCount[]>(`${this.baseUrl}/api/referrals/module-counts`);
    }

    getReferralInbox(): Observable<ReferralInboxItem[]> {
        return this.http.get<ReferralInboxItem[]>(`${this.baseUrl}/api/referrals/inbox`);
    }

    getPatientJourney(patientId: string): Observable<PatientJourneyStep[]> {
        return this.http.get<PatientJourneyStep[]>(`${this.baseUrl}/api/referrals/journey/${patientId}`);
    }

    markReferralRead(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/api/referrals/${id}/mark-read`, null);
    }

    markAllReferralsRead(): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/api/referrals/mark-all-read`, null);
    }

    claimReferral(id: string): Observable<ReferralQueueItem> {
        return this.http.post<ReferralQueueItem>(`${this.baseUrl}/api/referrals/${id}/claim`, null);
    }

    completeReferral(id: string): Observable<ReferralQueueItem> {
        return this.http.post<ReferralQueueItem>(`${this.baseUrl}/api/referrals/${id}/complete`, null);
    }

    // ----- Triage -----

    getTriages(): Observable<Triage[]> {
        return this.http.get<Triage[]>(`${this.baseUrl}/api/triage`);
    }

    getTriage(id: string): Observable<Triage | null> {
        return this.getOrNull<Triage>(`${this.baseUrl}/api/triage/${id}`);
    }

    getTriagesByPatient(patientId: string): Observable<Triage[]> {
        return this.http.get<Triage[]>(`${this.baseUrl}/api/triage/by-patient/${patientId}`);
    }

    createTriage(triage: Triage): Observable<Triage> {
        return this.http.post<Triage>(`${this.baseUrl}/api/triage`, triage);
    }

    updateTriage(id: string, triage: Triage): Observable<Triage> {
        return this.http.put<Triage>(`${this.baseUrl}/api/triage/${id}`, triage);
    }

    deleteTriage(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/triage/${id}`);
    }

    // ----- Consultations -----

    getConsultations(): Observable<Consultation[]> {
        return this.http.get<Consultation[]>(`${this.baseUrl}/api/consultations`);
    }

    getConsultation(id: string): Observable<Consultation | null> {
        return this.getOrNull<Consultation>(`${this.baseUrl}/api/consultations/${id}`);
    }

    createConsultation(consultation: Consultation): Observable<Consultation> {
        return this.http.post<Consultation>(`${this.baseUrl}/api/consultations`, consultation);
    }

    updateConsultation(id: string, consultation: Consultation): Observable<Consultation> {
        return this.http.put<Consultation>(`${this.baseUrl}/api/consultations/${id}`, consultation);
    }

    updateItnDispense(id: string, itnDispense: boolean): Observable<void> {
        return this.http.patch<void>(`${this.baseUrl}/api/consultations/${id}/itn-dispense`, itnDispense);
    }

    deleteConsultation(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/consultations/${id}`);
    }

    // ----- Pharmacy -----

    getPharmacyPrescriptions(): Observable<PharmacyPrescription[]> {
        return this.http.get<PharmacyPrescription[]>(`${this.baseUrl}/api/pharmacy`);
    }

    getPharmacy(id: string): Observable<PharmacyPrescription | null> {
        return this.getOrNull<PharmacyPrescription>(`${this.baseUrl}/api/pharmacy/${id}`);
    }

    getPharmacyByPatient(patientId: string): Observable<PharmacyPrescription[]> {
        return this.http.get<PharmacyPrescription[]>(`${this.baseUrl}/api/pharmacy/by-patient/${patientId}`);
    }

    createPharmacy(prescription: PharmacyPrescription): Observable<PharmacyPrescription> {
        return this.http.post<PharmacyPrescription>(`${this.baseUrl}/api/pharmacy`, prescription);
    }

    dispensePharmacy(updates: PharmacyDispenseUpdate[]): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/api/pharmacy/dispense`, updates);
    }

    deletePharmacy(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/pharmacy/${id}`);
    }

    // ----- Laboratory -----

    getLaboratories(): Observable<Laboratory[]> {
        return this.http.get<Laboratory[]>(`${this.baseUrl}/api/laboratory`);
    }

    getLaboratory(id: string): Observable<Laboratory | null> {
        return this.getOrNull<Laboratory>(`${this.baseUrl}/api/laboratory/${id}`);
    }

    createLaboratory(laboratory: Laboratory): Observable<Laboratory> {
        return this.http.post<Laboratory>(`${this.baseUrl}/api/laboratory`, laboratory);
    }

    updateLaboratory(id: string, laboratory: Laboratory): Observable<Laboratory> {
        return this.http.put<Laboratory>(`${this.baseUrl}/api/laboratory/${id}`, laboratory);
    }

    deleteLaboratory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/laboratory/${id}`);
    }

    // ----- Dental -----

    getDentals(): Observable<DentalConsultation[]> {
        return this.http.get<DentalConsultation[]>(`${this.baseUrl}/api/dental-consultations`);
    }

    getDental(id: string): Observable<DentalConsultation | null> {
        return this.getOrNull<DentalConsultation>(`${this.baseUrl}/api/dental-consultations/${id}`);
    }

    createDental(dental: DentalConsultation): Observable<DentalConsultation> {
        return this.http.post<DentalConsultation>(`${this.baseUrl}/api/dental-consultations`, dental);
    }

    updateDental(id: string, dental: DentalConsultation): Observable<DentalConsultation> {
        return this.http.put<DentalConsultation>(`${this.baseUrl}/api/dental-consultations/${id}`, dental);
    }

    deleteDental(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/dental-consultations/${id}`);
    }

    // ----- Ancillary -----

    getAncillaries(): Observable<Ancillary[]> {
        return this.http.get<Ancillary[]>(`${this.baseUrl}/api/ancillary-services`);
    }

    getAncillary(id: string): Observable<Ancillary | null> {
        return this.getOrNull<Ancillary>(`${this.baseUrl}/api/ancillary-services/${id}`);
    }

    createAncillary(ancillary: Ancillary): Observable<Ancillary> {
        return this.http.post<Ancillary>(`${this.baseUrl}/api/ancillary-services`, ancillary);
    }

    updateAncillary(id: string, ancillary: Ancillary): Observable<Ancillary> {
        return this.http.put<Ancillary>(`${this.baseUrl}/api/ancillary-services/${id}`, ancillary);
    }

    deleteAncillary(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/ancillary-services/${id}`);
    }

    // ----- Optometrists -----

    getOptometrists(): Observable<Optometrist[]> {
        return this.http.get<Optometrist[]>(`${this.baseUrl}/api/optometrists`);
    }

    getOptometrist(id: string): Observable<Optometrist | null> {
        return this.getOrNull<Optometrist>(`${this.baseUrl}/api/optometrists/${id}`);
    }

    createOptometrist(optometrist: Optometrist): Observable<Optometrist> {
        return this.http.post<Optometrist>(`${this.baseUrl}/api/optometrists`, optometrist);
    }

    updateOptometrist(id: string, optometrist: Optometrist): Observable<Optometrist> {
        return this.http.put<Optometrist>(`${this.baseUrl}/api/optometrists/${id}`, optometrist);
    }

    deleteOptometrist(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/optometrists/${id}`);
    }

    // ----- Ophthalmologists -----

    getOphthalmologists(): Observable<Ophthalmologist[]> {
        return this.http.get<Ophthalmologist[]>(`${this.baseUrl}/api/ophthalmologists`);
    }

    getOphthalmologist(id: string): Observable<Ophthalmologist | null> {
        return this.getOrNull<Ophthalmologist>(`${this.baseUrl}/api/ophthalmologists/${id}`);
    }

    createOphthalmologist(ophthalmologist: Ophthalmologist): Observable<Ophthalmologist> {
        return this.http.post<Ophthalmologist>(`${this.baseUrl}/api/ophthalmologists`, ophthalmologist);
    }

    updateOphthalmologist(id: string, ophthalmologist: Ophthalmologist): Observable<Ophthalmologist> {
        return this.http.put<Ophthalmologist>(`${this.baseUrl}/api/ophthalmologists/${id}`, ophthalmologist);
    }

    deleteOphthalmologist(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/ophthalmologists/${id}`);
    }

    // ----- Backups -----

    getBackups(): Observable<BackupInfoDto[]> {
        return this.http.get<BackupInfoDto[]>(`${this.baseUrl}/api/backups`);
    }

    createBackup(): Observable<BackupCreateResultDto> {
        return this.http.post<BackupCreateResultDto>(`${this.baseUrl}/api/backups`, null).pipe(catchError(this.handleError));
    }

    downloadBackup(fileName: string): Observable<ArrayBuffer> {
        const encoded = encodeURIComponent(fileName);
        return this.http.get(`${this.baseUrl}/api/backups/${encoded}/download`, { responseType: 'arraybuffer' });
    }

    deleteBackup(fileName: string): Observable<void> {
        const encoded = encodeURIComponent(fileName);
        return this.http.delete<void>(`${this.baseUrl}/api/backups/${encoded}`).pipe(catchError(this.handleError));
    }

    restoreBackup(file: File): Observable<BackupRestoreResultDto> {
        const formData = new FormData();
        formData.append('file', file, file.name);

        return this.http.post<BackupRestoreResultDto>(`${this.baseUrl}/api/backups/restore`, formData).pipe(catchError(this.handleError));
    }

    // ----- Settings -----

    getPublicSettings(): Observable<PublicHospitalSettingsDto | null> {
        return this.getOrNull<PublicHospitalSettingsDto>(`${this.baseUrl}/api/settings/public`);
    }

    getHospitalSettings(): Observable<HospitalSettingsDto> {
        return this.http.get<HospitalSettingsDto>(`${this.baseUrl}/api/settings`);
    }

    updateHospitalSettings(request: UpdateHospitalSettingsRequest): Observable<HospitalSettingsDto> {
        return this.http.put<HospitalSettingsDto>(`${this.baseUrl}/api/settings`, request).pipe(catchError(this.handleError));
    }

    getLanAccessStatus(): Observable<LanAccessStatusDto> {
        return this.http.get<LanAccessStatusDto>(`${this.baseUrl}/api/settings/lan/status`);
    }

    unlockLanAccess(request: LanAccessUnlockRequest): Observable<LanAccessUnlockResponse> {
        return this.http.post<LanAccessUnlockResponse>(`${this.baseUrl}/api/settings/lan/unlock`, request).pipe(catchError(this.handleError));
    }

    getLanSettings(): Observable<LanSettingsDto> {
        return this.http.get<LanSettingsDto>(`${this.baseUrl}/api/settings/lan`);
    }

    getDeploymentInfo(): Observable<DeploymentInfoDto> {
        return this.http.get<DeploymentInfoDto>(`${this.baseUrl}/api/settings/deployment`);
    }

    updateLanSettings(request: UpdateLanSettingsRequest): Observable<LanSettingsDto> {
        return this.http.put<LanSettingsDto>(`${this.baseUrl}/api/settings/lan`, request).pipe(catchError(this.handleError));
    }

    changeLanAccessPassword(request: ChangeLanAccessPasswordRequest): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/api/settings/lan/password`, request).pipe(catchError(this.handleError));
    }

    getHospitalModulesConfig(): Observable<HospitalModulesConfigDto> {
        return this.http.get<HospitalModulesConfigDto>(`${this.baseUrl}/api/settings/modules`);
    }

    updateHospitalModulesConfig(request: UpdateHospitalModulesRequest): Observable<HospitalModulesConfigDto> {
        return this.http.put<HospitalModulesConfigDto>(`${this.baseUrl}/api/settings/modules`, request).pipe(catchError(this.handleError));
    }

    getActiveHospitalModules(): Observable<HospitalModulesStateDto> {
        return this.http.get<HospitalModulesStateDto>(`${this.baseUrl}/api/settings/modules/active`);
    }

    // ----- Exports -----

    getExportModules(): Observable<ExportModuleInfoDto[]> {
        return this.http.get<ExportModuleInfoDto[]>(`${this.baseUrl}/api/exports/modules`);
    }

    previewExport(request: ExportRecordsRequest): Observable<ExportPreviewDto> {
        return this.http.post<ExportPreviewDto>(`${this.baseUrl}/api/exports/preview`, request).pipe(catchError(this.handleError));
    }

    downloadExport(request: ExportRecordsRequest): Observable<ExportDownloadResult> {
        return this.http.post(`${this.baseUrl}/api/exports/download`, request, {
            observe: 'response',
            responseType: 'arraybuffer'
        }).pipe(
            catchError(this.handleError),
            map((response: HttpResponse<ArrayBuffer>) => ({
                content: response.body ?? new ArrayBuffer(0),
                fileName: this.extractDownloadFileName(response) ?? this.defaultExportFileName()
            }))
        );
    }

    // ----- Helpers -----

    private getOrNull<T>(url: string): Observable<T | null> {
        return this.http.get<T>(url).pipe(
            catchError((error: HttpErrorResponse) =>
                error.status === 404 ? of(null) : throwError(() => error)
            )
        );
    }

    private extractDownloadFileName(response: HttpResponse<ArrayBuffer>): string | null {
        const disposition = response.headers.get('content-disposition');
        if (!disposition) {
            return null;
        }

        const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
        if (utf8Match?.[1]) {
            return decodeURIComponent(utf8Match[1].trim());
        }

        const quotedMatch = /filename="([^"]+)"/i.exec(disposition);
        if (quotedMatch?.[1]) {
            return quotedMatch[1].trim();
        }

        const plainMatch = /filename=([^;]+)/i.exec(disposition);
        return plainMatch?.[1]?.trim().replace(/^"|"$/g, '') ?? null;
    }

    private defaultExportFileName(): string {
        const stamp = new Date().toISOString().replace(/[-:]/g, '').slice(0, 13);
        return `renaissance-export-${stamp}.xlsx`;
    }

    private handleError(error: HttpErrorResponse): Observable<never> {
        const body = typeof error.error === 'string'
            ? error.error.trim().replace(/^"|"$/g, '')
            : error.error?.title;
        return throwError(() => new Error(body || error.message || 'Request failed.'));
    }
}
