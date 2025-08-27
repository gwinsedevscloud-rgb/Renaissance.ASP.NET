import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";

export interface OptometristDto {
    id?: string;
    patientId: string;
    visualAcuityRight: string;
    visualAcuityLeft: string;
    glassesDispensed: boolean;
    referred: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class OptometristService {

    private readonly baseUrl = '/api/optometrists';
    private http = inject(HttpClient)

    save(dto: OptometristDto): Observable<OptometristDto> {
        return this.http.post<OptometristDto>(this.baseUrl, dto);
    }

    update(id: string, dto: OptometristDto): Observable<OptometristDto> {
        return this.http.put<OptometristDto>(`${this.baseUrl}/${id}`, dto);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    findById(id: string): Observable<OptometristDto> {
        return this.http.get<OptometristDto>(`${this.baseUrl}/${id}`);
    }

    findByPatient(patientId: string): Observable<OptometristDto[]> {
        return this.http.get<OptometristDto[]>(`${this.baseUrl}/by-patient/${patientId}`);
    }

    findAll(): Observable<OptometristDto[]> {
        return this.http.get<OptometristDto[]>(this.baseUrl);
    }

}
