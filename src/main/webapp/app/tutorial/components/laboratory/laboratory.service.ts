import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";

export interface LaboratoryDto {
    id?: string;
    patientId: string;
    testName: string;
    testType: string;
    result?: string;
    note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LaboratoryService {

    private http = inject(HttpClient);
    private baseUrl = '/api/laboratory';

    getAll(): Observable<LaboratoryDto[]> {
        return this.http.get<LaboratoryDto[]>(this.baseUrl);
    }

    saveAll(dtos: LaboratoryDto[]): Observable<LaboratoryDto[]> {
        return this.http.post<LaboratoryDto[]>(`${this.baseUrl}/bulk`, dtos);
    }

    getById(id: string): Observable<LaboratoryDto> {
        return this.http.get<LaboratoryDto>(`${this.baseUrl}/${id}`);
    }

    getByPatientId(patientId: string): Observable<LaboratoryDto[]> {
        return this.http.get<LaboratoryDto[]>(`${this.baseUrl}/by-patient/${patientId}`);
    }

    save(dto: LaboratoryDto): Observable<LaboratoryDto> {
        return this.http.post<LaboratoryDto>(this.baseUrl, dto);
    }

    update(id: string, dto: LaboratoryDto): Observable<LaboratoryDto> {
        return this.http.put<LaboratoryDto>(`${this.baseUrl}/${id}`, dto);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
}
