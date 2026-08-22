import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { Optometrist, Patient } from '../../core/models/clinical.models';
import { AppModule } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';
import { PatientQuickSearchComponent } from '../../shared/patient-quick-search/patient-quick-search.component';
import { PatientIdentityComponent } from '../../shared/patient-identity/patient-identity.component';
import { ReferralQueuePanelComponent } from '../../shared/referral-queue-panel/referral-queue-panel.component';

@Component({
    selector: 'app-optometrists-list',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent,
        PatientQuickSearchComponent, PatientIdentityComponent, ReferralQueuePanelComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Optometrists" subtitle="Optometry exams" icon="heroicons_outline:eye" />
            <app-referral-queue-panel [module]="AppModule.Optometrists" />
            <app-patient-quick-search createHrefPrefix="/optometrists/create" actionLabel="New exam" />
            @if (loading()) {
                <app-loading-state message="Loading optometry records..." />
            } @else if (items().length === 0) {
                <app-empty-state icon="heroicons_outline:eye" title="No optometry records yet"
                    message="Search for a patient above to record an exam." />
            } @else {
                <div class="ren-results-bar"><span class="ren-results-count">{{ items().length }} records</span></div>
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="items()" class="ren-table w-full">
                        <ng-container matColumnDef="patient">
                            <th mat-header-cell *matHeaderCellDef>Patient</th>
                            <td mat-cell *matCellDef="let o"><app-patient-identity [patient]="patient(o.patientId)" /></td>
                        </ng-container>
                        <ng-container matColumnDef="va">
                            <th mat-header-cell *matHeaderCellDef>Visual acuity</th>
                            <td mat-cell *matCellDef="let o">R: {{ o.visualAcuityRight ?? '—' }} / L: {{ o.visualAcuityLeft ?? '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let o">
                                <a mat-icon-button [routerLink]="['/optometrists/edit', o.id]"><mat-icon svgIcon="heroicons_outline:pencil"></mat-icon></a>
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
export class OptometristsListComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    readonly AppModule = AppModule;

    readonly loading = signal(true);
    readonly items = signal<Optometrist[]>([]);
    readonly patients = signal<Map<string, Patient>>(new Map());
    readonly columns = ['patient', 'va', 'actions'];

    ngOnInit(): void {
        forkJoin({ patients: this.api.getPatients(), items: this.api.getOptometrists() }).subscribe({
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
