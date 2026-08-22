import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { DentalConsultation, Patient } from '../../core/models/clinical.models';
import { AppModule } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';
import { PatientQuickSearchComponent } from '../../shared/patient-quick-search/patient-quick-search.component';
import { PatientIdentityComponent } from '../../shared/patient-identity/patient-identity.component';
import { ReferralQueuePanelComponent } from '../../shared/referral-queue-panel/referral-queue-panel.component';

@Component({
    selector: 'app-dental-list',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent,
        PatientQuickSearchComponent, PatientIdentityComponent, ReferralQueuePanelComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Dental" subtitle="Dental consultations" icon="heroicons_outline:face-smile" />
            <app-referral-queue-panel [module]="AppModule.Dental" />
            <app-patient-quick-search createHrefPrefix="/dental/create" actionLabel="New consultation" />
            @if (loading()) {
                <app-loading-state message="Loading dental records..." />
            } @else if (items().length === 0) {
                <app-empty-state icon="heroicons_outline:face-smile" title="No dental records yet"
                    message="Search for a patient above to record a dental consultation." />
            } @else {
                <div class="ren-results-bar"><span class="ren-results-count">{{ items().length }} records</span></div>
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="items()" class="ren-table w-full">
                        <ng-container matColumnDef="patient">
                            <th mat-header-cell *matHeaderCellDef>Patient</th>
                            <td mat-cell *matCellDef="let d"><app-patient-identity [patient]="patient(d.patientId)" /></td>
                        </ng-container>
                        <ng-container matColumnDef="diagnoses">
                            <th mat-header-cell *matHeaderCellDef>Diagnoses</th>
                            <td mat-cell *matCellDef="let d">{{ d.diagnoses?.[0] ?? '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let d">
                                <a mat-icon-button [routerLink]="['/dental/edit', d.id]"><mat-icon svgIcon="heroicons_outline:pencil"></mat-icon></a>
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
export class DentalListComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    readonly AppModule = AppModule;

    readonly loading = signal(true);
    readonly items = signal<DentalConsultation[]>([]);
    readonly patients = signal<Map<string, Patient>>(new Map());
    readonly columns = ['patient', 'diagnoses', 'actions'];

    ngOnInit(): void {
        forkJoin({ patients: this.api.getPatients(), items: this.api.getDentals() }).subscribe({
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
}
