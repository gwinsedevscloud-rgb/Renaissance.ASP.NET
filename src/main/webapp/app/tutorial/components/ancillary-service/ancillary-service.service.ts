import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";

export interface AncillaryServiceDto {
    id?: string;
    patientId: string;
    services: string[];
    pregnancyStatus: string;
}

@Injectable({
  providedIn: 'root'
})
export class AncillaryServiceService {

    private baseUrl = '/api/ancillary-services';
    private http = inject(HttpClient);

    getAll(): Observable<AncillaryServiceDto[]> {
        return this.http.get<AncillaryServiceDto[]>(this.baseUrl);
    }

    getById(id: string): Observable<AncillaryServiceDto> {
        return this.http.get<AncillaryServiceDto>(`${this.baseUrl}/${id}`);
    }

    getByPatientId(patientId: string): Observable<AncillaryServiceDto[]> {
        return this.http.get<AncillaryServiceDto[]>(`${this.baseUrl}/by-patient/${patientId}`);
    }

    create(service: AncillaryServiceDto): Observable<AncillaryServiceDto> {
        return this.http.post<AncillaryServiceDto>(this.baseUrl, service);
    }

    update(id: string, service: AncillaryServiceDto): Observable<AncillaryServiceDto> {
        return this.http.put<AncillaryServiceDto>(`${this.baseUrl}/${id}`, service);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
}
