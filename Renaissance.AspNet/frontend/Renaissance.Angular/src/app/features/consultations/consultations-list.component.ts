import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { Consultation, Patient } from '../../core/models/clinical.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';
import { PatientQuickSearchComponent } from '../../shared/patient-quick-search/patient-quick-search.component';
import { PatientIdentityComponent } from '../../shared/patient-identity/patient-identity.component';

@Component({
    selector: 'app-consultations-list',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent,
        PatientQuickSearchComponent, PatientIdentityComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Consultations" subtitle="Diagnosis and treatment records" icon="heroicons_outline:clipboard-document-list" />
            <app-patient-quick-search createHrefPrefix="/consultations/create" actionLabel="New consultation" />
            @if (loading()) {
                <app-loading-state message="Loading consultations..." />
            } @else if (items().length === 0) {
                <app-empty-state icon="heroicons_outline:clipboard-document-list" title="No consultations yet"
                    message="Search for a patient above to record a consultation." />
            } @else {
                <div class="ren-results-bar"><span class="ren-results-count">{{ items().length }} records</span></div>
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="items()" class="ren-table w-full">
                        <ng-container matColumnDef="patient">
                            <th mat-header-cell *matHeaderCellDef>Patient</th>
                            <td mat-cell *matCellDef="let c"><app-patient-identity [patient]="patient(c.patientId)" /></td>
                        </ng-container>
                        <ng-container matColumnDef="diagnoses">
                            <th mat-header-cell *matHeaderCellDef>Diagnoses</th>
                            <td mat-cell *matCellDef="let c">{{ (c.diagnoses?.[0] ?? '—') + (c.diagnoses?.length > 1 ? '…' : '') }}</td>
                        </ng-container>
                        <ng-container matColumnDef="date">
                            <th mat-header-cell *matHeaderCellDef class="ren-col-hide-sm">Recorded</th>
                            <td mat-cell *matCellDef="let c" class="ren-col-hide-sm">{{ formatDate(c.createdDate) }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let c">
                                <a mat-icon-button [routerLink]="['/consultations/edit', c.id]"><mat-icon svgIcon="heroicons_outline:pencil"></mat-icon></a>
                            </td>
                        </ng-container>
                        <tr mat-header-row *matHeaderRowDef="columns"></tr>
                        <tr mat-row *matRowDef="let row; columns: columns"></tr>
                    </table>
                </div>
            }
        </div>
    `
})
export class ConsultationsListComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);

    readonly loading = signal(true);
    readonly items = signal<Consultation[]>([]);
    readonly patients = signal<Map<string, Patient>>(new Map());
    readonly columns = ['patient', 'diagnoses', 'date', 'actions'];

    ngOnInit(): void {
        forkJoin({ patients: this.api.getPatients(), items: this.api.getConsultations() }).subscribe({
            next: ({ patients, items }) => {
                this.patients.set(new Map(patients.map(p => [p.id, p])));
                this.items.set(items.slice(0, 50));
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    patient(id: string): Patient | undefined {
        return this.patients().get(id);
    }

    formatDate(value?: string): string {
        return value ? new Date(value).toLocaleDateString() : '—';
    }
}
