import {inject, Injectable, signal} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";

export interface OphthalmologistDto {
    id?: string;
    patientId: string;
    referred: boolean;
    diagnoses: string[];
    treatments: string[];
    surgeries: string[];
    othersTreatment: string[],
    othersDiagnosis: string[],
    otherSurgery: string[],
    visualAcuityRight: string,
    visualAcuityLeft: string
    glassesDispensed: boolean
}

@Injectable({
  providedIn: 'root'
})
export class OphthalmologistService {

    private readonly baseUrl = '/api/ophthalmologists';

    private http = inject(HttpClient)

    save(dto: OphthalmologistDto): Observable<OphthalmologistDto> {
        return this.http.post<OphthalmologistDto>(this.baseUrl, dto);
    }

    update(id: string, dto: OphthalmologistDto): Observable<OphthalmologistDto> {
        return this.http.put<OphthalmologistDto>(`${this.baseUrl}/${id}`, dto);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    findById(id: string): Observable<OphthalmologistDto> {
        return this.http.get<OphthalmologistDto>(`${this.baseUrl}/${id}`);
    }

    findByPatient(patientId: string): Observable<OphthalmologistDto[]> {
        return this.http.get<OphthalmologistDto[]>(`${this.baseUrl}/by-patient/${patientId}`);
    }

    findAll(): Observable<OphthalmologistDto[]> {
        return this.http.get<OphthalmologistDto[]>(this.baseUrl);
    }
}
