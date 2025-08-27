import {inject, Injectable} from '@angular/core';
import {Observable} from "rxjs";
import {HttpClient} from "@angular/common/http";

export type DispensationStatus = 'PENDING' | 'DISPENSED' | 'DECLINED';

export interface PharmacyPrescription {
    id?: string;
    patientId: string;
    drugCategory: string;
    drugName: string;
    dosage: string;
    frequency: string;
    duration: string;
    dispensed: boolean,
    dispensationNote: string,
    createdDate?: string;
    updatedDate?: string;
    createdBy?: string;
    updatedBy?: string;
    status?: DispensationStatus
    quantityDispensed?: number;
}


@Injectable({
  providedIn: 'root'
})
export class PharmacyService {

    private resourceUrl = '/api/pharmacy';
    private http = inject(HttpClient);

    getAll(): Observable<PharmacyPrescription[]> {
        return this.http.get<PharmacyPrescription[]>(this.resourceUrl);
    }

    getById(id: string): Observable<PharmacyPrescription> {
        return this.http.get<PharmacyPrescription>(`${this.resourceUrl}/${id}`);
    }

    getByPatientId(patientId: string): Observable<PharmacyPrescription[]> {
        return this.http.get<PharmacyPrescription[]>(`${this.resourceUrl}/patient/${patientId}`);
    }

    create(prescription: PharmacyPrescription): Observable<PharmacyPrescription> {
        return this.http.post<PharmacyPrescription>(this.resourceUrl, prescription);
    }

    update(id: string, prescription: PharmacyPrescription): Observable<PharmacyPrescription> {
        return this.http.put<PharmacyPrescription>(`${this.resourceUrl}/${id}`, prescription);
    }

    archive(id: string): Observable<void> {
        return this.http.delete<void>(`${this.resourceUrl}/${id}`);
    }

    savePrescriptions(prescriptions: PharmacyPrescription[]): Observable<void> {
        return this.http.post<void>(`${this.resourceUrl}/bulk`, prescriptions);
    }

    updatePrescriptions(prescriptions: PharmacyPrescription[]): Observable<void> {
        return this.http.put<void>(`${this.resourceUrl}/bulk`, prescriptions);
    }


    hasPendingPrescriptions(patientId: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.resourceUrl}/has-pending/${patientId}`);
    }
}
