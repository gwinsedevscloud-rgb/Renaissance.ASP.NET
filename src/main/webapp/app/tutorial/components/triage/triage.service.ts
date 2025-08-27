import {inject, Injectable, WritableSignal} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";

export interface Triage {
    id?: string;
    patientId: string;
    temperature?: number;
    systolicBp?: number;
    diastolicBp?: number;
    pulseRate?: number;
    respiratoryRate?: number;
    weight?: number;
    height?: number;
    diabetes?: boolean;
    asthma?: boolean;
    sickleCell?: boolean;
    smoking?: boolean;
    createdDate?: string;
    createdBy?: string;
    updatedDate?: string;
    updatedBy?: string;
}


@Injectable({
  providedIn: 'root'
})
export class TriageService {

    private http = inject(HttpClient);
    private baseUrl = '/api/ren/triage';

    getAllTriages(): Observable<Triage[]> {
        return this.http.get<Triage[]>(this.baseUrl);
    }

    getTriageById(id: string): Observable<Triage> {
        return this.http.get<Triage>(`${this.baseUrl}/${id}`);
    }

    createTriage(triage: Triage | null): Observable<Triage> {
        return this.http.post<Triage>(this.baseUrl, triage);
    }

    updateTriage(id: string, triage: Triage | null): Observable<Triage> {
        return this.http.put<Triage>(`${this.baseUrl}/${id}`, triage);
    }

    archiveTriage(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    getTriagesByPatientId(patientId: string): Observable<Triage[]> {
        return this.http.get<Triage[]>(`${this.baseUrl}/patient/${patientId}`);
    }
}
