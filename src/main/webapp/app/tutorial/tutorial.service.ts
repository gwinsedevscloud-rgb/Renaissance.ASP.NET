import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {Observable, tap} from 'rxjs';

export interface Patient {
    id?: string;
    clientNumber: string;
    age: number;
    ageUnit: 'years' | 'months';
    sex: 'Male' | 'Female';
    maritalStatus: string;
    tribe: string;
    religion: string;
    occupation: string;
    education: string;
    address: string;
    createdBy?: string;
    createdDate?: string;
    updatedBy?: string;
    updatedDate?: string;
    archived?: boolean;
    phoneNumber?: string;
}


@Injectable()
export class TutorialService {
    private resourceUrl = '/api/ren/patients';
    private http = inject(HttpClient);

    getAll(): Observable<Patient[]> {
        return this.http.get<Patient[]>(this.resourceUrl).pipe();
    }

    getById(id: string): Observable<Patient> {
        return this.http.get<Patient>(`${this.resourceUrl}/${id}`);
    }

    getByClientNumber(clientNumber: string): Observable<Patient[]> {
        return this.http.get<Patient[]>(`${this.resourceUrl}/client-number/${clientNumber}`);
    }

    create(patient: Patient): Observable<Patient> {
        return this.http.post<Patient>(this.resourceUrl, patient);
    }

    update(id: string, patient: Patient): Observable<Patient> {
        return this.http.put<Patient>(`${this.resourceUrl}/${id}`, patient);
    }

    archive(id: string): Observable<void> {
        return this.http.delete<void>(`${this.resourceUrl}/${id}`);
    }

    generateClientNumber(): Observable<string> {
        // @ts-ignore
        return this.http.get<string>(`${this.resourceUrl}/generate-client-number`, { responseType: 'text' });
    }

}
