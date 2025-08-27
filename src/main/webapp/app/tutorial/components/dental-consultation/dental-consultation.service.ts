import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";

export interface DentalConsultationDto {
    id?: string;
    patientId: string;
    diagnoses: string[];
    treatments: string[];
    dispensedItems: string[];
    servicesReferred: string[];
    referred: boolean;
    othersDiagnosis: string[];
    othersTreatment: string[]
}

@Injectable({
  providedIn: 'root'
})
export class DentalConsultationService {
    private readonly baseUrl = '/api/dental-consultations';
    private http = inject(HttpClient);

    create(dto: DentalConsultationDto): Observable<DentalConsultationDto> {
        return this.http.post<DentalConsultationDto>(this.baseUrl, dto);
    }

    createBulk(dtos: DentalConsultationDto[]): Observable<DentalConsultationDto[]> {
        return this.http.post<DentalConsultationDto[]>(`${this.baseUrl}/bulk`, dtos);
    }

    update(id: string, dto: DentalConsultationDto): Observable<DentalConsultationDto> {
        return this.http.put<DentalConsultationDto>(`${this.baseUrl}/${id}`, dto);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    getById(id: string): Observable<DentalConsultationDto> {
        return this.http.get<DentalConsultationDto>(`${this.baseUrl}/${id}`);
    }

    getByPatientId(patientId: string): Observable<DentalConsultationDto[]> {
        return this.http.get<DentalConsultationDto[]>(`${this.baseUrl}/by-patient/${patientId}`);
    }

    getAll(): Observable<DentalConsultationDto[]> {
        return this.http.get<DentalConsultationDto[]>(this.baseUrl);
    }

}
