import {inject, Injectable} from '@angular/core';
import {Observable} from "rxjs";
import {HttpClient} from "@angular/common/http";

export interface ConsultationDto {
    id?: string;
    patientId: string;
    diagnoses: string[];
    treatments: string[];
    servicesReferred: string[];
    referred: boolean;
    itnOrder: boolean;
    itnDispense: boolean;
    othersDiagnosis: string[];
    othersTreatment: string[]
}

@Injectable({
  providedIn: 'root'
})
export class ConsultationNoteService {

  constructor() { }

    private baseUrl = '/api/consultations';
    private http = inject(HttpClient);

    getAll(): Observable<ConsultationDto[]> {
        return this.http.get<ConsultationDto[]>(this.baseUrl);
    }

    getById(id: string): Observable<ConsultationDto> {
        return this.http.get<ConsultationDto>(`${this.baseUrl}/${id}`);
    }

    getByPatientId(patientId: string): Observable<ConsultationDto[]> {
        return this.http.get<ConsultationDto[]>(`${this.baseUrl}/by-patient/${patientId}`);
    }

    save(dto: ConsultationDto): Observable<ConsultationDto> {
        return this.http.post<ConsultationDto>(this.baseUrl, dto);
    }

    saveAll(dtos: ConsultationDto[]): Observable<ConsultationDto[]> {
        return this.http.post<ConsultationDto[]>(`${this.baseUrl}/bulk`, dtos);
    }

    update(id: string, dto: ConsultationDto): Observable<ConsultationDto> {
        return this.http.put<ConsultationDto>(`${this.baseUrl}/${id}`, dto);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    updateItnDispense(id: string, itnDispense: boolean): Observable<ConsultationDto> {
        return this.http.patch<ConsultationDto>(`${this.baseUrl}/${id}/itn-dispense`, itnDispense);
    }
}
