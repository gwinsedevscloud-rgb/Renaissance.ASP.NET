import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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

@Injectable({ providedIn: 'root' })
export class RenaissanceApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = environment.apiUrl;

    getPatients(q?: string): Observable<Patient[]> {
        let params = new HttpParams();
        if (q) params = params.set('q', q);
        return this.http.get<Patient[]>(`${this.baseUrl}/api/patients`, { params });
    }

    getPatient(id: string): Observable<Patient> {
        return this.http.get<Patient>(`${this.baseUrl}/api/patients/${id}`);
    }

    generateClientNumber(): Observable<string> {
        return this.http.get(`${this.baseUrl}/api/patients/next-client-number`, { responseType: 'text' });
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

    getDashboard(patientId: string): Observable<ClientDashboardDto> {
        return this.http.get<ClientDashboardDto>(`${this.baseUrl}/api/client-dashboard/${patientId}`);
    }

    getStakeholdersDashboard(): Observable<StakeholdersDashboardDto> {
        return this.http.get<StakeholdersDashboardDto>(`${this.baseUrl}/api/stakeholders-dashboard`);
    }

    getTriage(id: string): Observable<Triage> {
        return this.http.get<Triage>(`${this.baseUrl}/api/triage/${id}`);
    }

    createTriage(triage: Triage): Observable<Triage> {
        return this.http.post<Triage>(`${this.baseUrl}/api/triage`, triage);
    }

    updateTriage(id: string, triage: Triage): Observable<Triage> {
        return this.http.put<Triage>(`${this.baseUrl}/api/triage/${id}`, triage);
    }

    getConsultation(id: string): Observable<Consultation> {
        return this.http.get<Consultation>(`${this.baseUrl}/api/consultations/${id}`);
    }

    createConsultation(consultation: Consultation): Observable<Consultation> {
        return this.http.post<Consultation>(`${this.baseUrl}/api/consultations`, consultation);
    }

    updateConsultation(id: string, consultation: Consultation): Observable<Consultation> {
        return this.http.put<Consultation>(`${this.baseUrl}/api/consultations/${id}`, consultation);
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

    getLaboratory(id: string): Observable<Laboratory> {
        return this.http.get<Laboratory>(`${this.baseUrl}/api/laboratory/${id}`);
    }

    createLaboratory(lab: Laboratory): Observable<Laboratory> {
        return this.http.post<Laboratory>(`${this.baseUrl}/api/laboratory`, lab);
    }

    updateLaboratory(id: string, lab: Laboratory): Observable<Laboratory> {
        return this.http.put<Laboratory>(`${this.baseUrl}/api/laboratory/${id}`, lab);
    }

    getDental(id: string): Observable<DentalConsultation> {
        return this.http.get<DentalConsultation>(`${this.baseUrl}/api/dental-consultations/${id}`);
    }

    createDental(dental: DentalConsultation): Observable<DentalConsultation> {
        return this.http.post<DentalConsultation>(`${this.baseUrl}/api/dental-consultations`, dental);
    }

    updateDental(id: string, dental: DentalConsultation): Observable<DentalConsultation> {
        return this.http.put<DentalConsultation>(`${this.baseUrl}/api/dental-consultations/${id}`, dental);
    }

    createAncillary(ancillary: Ancillary): Observable<Ancillary> {
        return this.http.post<Ancillary>(`${this.baseUrl}/api/ancillary-services`, ancillary);
    }

    createOptometrist(optometrist: Optometrist): Observable<Optometrist> {
        return this.http.post<Optometrist>(`${this.baseUrl}/api/optometrists`, optometrist);
    }

    createOphthalmologist(ophthalmologist: Ophthalmologist): Observable<Ophthalmologist> {
        return this.http.post<Ophthalmologist>(`${this.baseUrl}/api/ophthalmologists`, ophthalmologist);
    }
}
