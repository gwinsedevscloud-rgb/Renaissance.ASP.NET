import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { Patient, Triage } from '../../core/models/clinical.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';
import { PatientQuickSearchComponent } from '../../shared/patient-quick-search/patient-quick-search.component';
import { PatientIdentityComponent } from '../../shared/patient-identity/patient-identity.component';

@Component({
    selector: 'app-triage-list',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent,
        PatientQuickSearchComponent, PatientIdentityComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Triage" subtitle="Record vitals and open a new encounter" icon="heroicons_outline:heart" />
            <app-patient-quick-search createHrefPrefix="/triage/create" actionLabel="New triage" />
            @if (loading()) {
                <app-loading-state message="Loading triage records..." />
            } @else if (items().length === 0) {
                <app-empty-state icon="heroicons_outline:heart" title="No triage records yet"
                    message="Search for a patient above to start a new encounter." />
            } @else {
                <div class="ren-results-bar"><span class="ren-results-count">{{ items().length }} recent records</span></div>
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="items()" class="ren-table w-full">
                        <ng-container matColumnDef="patient">
                            <th mat-header-cell *matHeaderCellDef>Patient</th>
                            <td mat-cell *matCellDef="let t"><app-patient-identity [patient]="patient(t.patientId)" /></td>
                        </ng-container>
                        <ng-container matColumnDef="bp">
                            <th mat-header-cell *matHeaderCellDef>BP</th>
                            <td mat-cell *matCellDef="let t">{{ t.systolicBp ?? '—' }}/{{ t.diastolicBp ?? '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="temp">
                            <th mat-header-cell *matHeaderCellDef class="ren-col-hide-sm">Temp</th>
                            <td mat-cell *matCellDef="let t" class="ren-col-hide-sm">{{ t.temperature != null ? t.temperature + '°C' : '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="pulse">
                            <th mat-header-cell *matHeaderCellDef class="ren-col-hide-md">Pulse</th>
                            <td mat-cell *matCellDef="let t" class="ren-col-hide-md">{{ t.pulseRate ?? '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="date">
                            <th mat-header-cell *matHeaderCellDef class="ren-col-hide-sm">Recorded</th>
                            <td mat-cell *matCellDef="let t" class="ren-col-hide-sm">{{ formatDate(t.createdDate) }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let t">
                                <a mat-icon-button [routerLink]="['/triage/edit', t.id]" title="Edit"><mat-icon svgIcon="heroicons_outline:pencil"></mat-icon></a>
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
export class TriageListComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);

    readonly loading = signal(true);
    readonly items = signal<Triage[]>([]);
    readonly patients = signal<Map<string, Patient>>(new Map());
    readonly columns = ['patient', 'bp', 'temp', 'pulse', 'date', 'actions'];

    ngOnInit(): void {
        forkJoin({ patients: this.api.getPatients(), triages: this.api.getTriages() }).subscribe({
            next: ({ patients, triages }) => {
                this.patients.set(new Map(patients.map(p => [p.id, p])));
                this.items.set(triages.slice(0, 50));
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
