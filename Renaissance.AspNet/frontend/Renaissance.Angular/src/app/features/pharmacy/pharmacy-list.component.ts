import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { Patient, PharmacyPrescription } from '../../core/models/clinical.models';
import { AppModule } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';
import { PatientQuickSearchComponent } from '../../shared/patient-quick-search/patient-quick-search.component';
import { PatientIdentityComponent } from '../../shared/patient-identity/patient-identity.component';
import { ReferralQueuePanelComponent } from '../../shared/referral-queue-panel/referral-queue-panel.component';

@Component({
    selector: 'app-pharmacy-list',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent,
        PatientQuickSearchComponent, PatientIdentityComponent, ReferralQueuePanelComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Pharmacy" subtitle="Prescribe and dispense medications" icon="heroicons_outline:beaker" />
            <app-referral-queue-panel [module]="AppModule.Pharmacy" />
            <app-patient-quick-search createHrefPrefix="/pharmacy/create" actionLabel="New prescription" />
            @if (loading()) {
                <app-loading-state message="Loading prescriptions..." />
            } @else if (items().length === 0) {
                <app-empty-state icon="heroicons_outline:beaker" title="No prescriptions yet"
                    message="Search for a patient above to add a prescription." />
            } @else {
                <div class="ren-results-bar"><span class="ren-results-count">{{ items().length }} prescriptions</span></div>
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="items()" class="ren-table w-full">
                        <ng-container matColumnDef="patient">
                            <th mat-header-cell *matHeaderCellDef>Patient</th>
                            <td mat-cell *matCellDef="let p"><app-patient-identity [patient]="patient(p.patientId)" /></td>
                        </ng-container>
                        <ng-container matColumnDef="drug">
                            <th mat-header-cell *matHeaderCellDef>Drug</th>
                            <td mat-cell *matCellDef="let p">{{ p.drugName ?? '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="status">
                            <th mat-header-cell *matHeaderCellDef>Status</th>
                            <td mat-cell *matCellDef="let p">{{ p.status ?? '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let p">
                                <a mat-stroked-button color="primary" [routerLink]="['/pharmacy/dispense', p.patientId]">Dispense</a>
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
export class PharmacyListComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    readonly AppModule = AppModule;

    readonly loading = signal(true);
    readonly items = signal<PharmacyPrescription[]>([]);
    readonly patients = signal<Map<string, Patient>>(new Map());
    readonly columns = ['patient', 'drug', 'status', 'actions'];

    ngOnInit(): void {
        forkJoin({ patients: this.api.getPatients(), items: this.api.getPharmacyPrescriptions() }).subscribe({
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
