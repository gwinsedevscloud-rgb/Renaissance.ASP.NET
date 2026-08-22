import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { DispensationStatus, Patient, PharmacyDispenseUpdate, PharmacyPrescription } from '../../core/models/clinical.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';

interface DispenseItem {
    id: string;
    drugCategory?: string;
    drugName?: string;
    dosage?: string;
    frequency?: string;
    duration?: string;
    status: DispensationStatus;
    quantityDispensed?: number;
    dispensationNote?: string;
}

@Component({
    selector: 'app-pharmacy-dispense',
    imports: [
        FormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule,
        PageHeaderComponent, LoadingStateComponent, FormActionsComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            @if (loading()) {
                <app-loading-state message="Loading prescriptions..." />
            } @else if (!patient) {
                <div class="ren-empty-inline">Patient not found.</div>
            } @else {
                <app-page-header [title]="'Dispense — ' + patient.clientNumber" subtitle="Update dispensation status" icon="heroicons_outline:beaker" />
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    @if (items.length === 0) {
                        <p class="ren-text-muted">No prescriptions to dispense.</p>
                        <a mat-stroked-button [routerLink]="['/patient-dashboard', patientId]">Back</a>
                    } @else {
                        @for (item of items; track item.id; let i = $index) {
                            <div class="ren-dispense-item">
                                <strong>{{ item.drugName }}</strong>
                                <p class="ren-text-muted small">{{ item.drugCategory }} · {{ item.dosage }} · {{ item.frequency }} · {{ item.duration }}</p>
                                <div class="ren-form-row">
                                    <mat-form-field subscriptSizing="dynamic">
                                        <mat-label>Status</mat-label>
                                        <mat-select [(ngModel)]="items[i].status" [name]="'status' + i">
                                            @for (s of statuses; track s) {
                                                <mat-option [value]="s">{{ s }}</mat-option>
                                            }
                                        </mat-select>
                                    </mat-form-field>
                                    <mat-form-field subscriptSizing="dynamic">
                                        <mat-label>Quantity</mat-label>
                                        <input matInput type="number" [(ngModel)]="items[i].quantityDispensed" [name]="'qty' + i" />
                                    </mat-form-field>
                                    <mat-form-field class="flex-1" subscriptSizing="dynamic">
                                        <mat-label>Note</mat-label>
                                        <input matInput [(ngModel)]="items[i].dispensationNote" [name]="'note' + i" />
                                    </mat-form-field>
                                </div>
                            </div>
                        }
                        <app-form-actions saveLabel="Save Dispensation" [cancelHref]="'/patient-dashboard/' + patientId" [saving]="saving()" />
                    }
                </form>
            }
        </div>
    `
})
export class PharmacyDispenseComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    patientId = '';
    patient: Patient | null = null;
    items: DispenseItem[] = [];
    readonly statuses = Object.values(DispensationStatus);
    readonly loading = signal(true);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.patientId = this.route.snapshot.paramMap.get('patientId') ?? '';
        this.api.getPatient(this.patientId).subscribe(patient => {
            this.patient = patient;
            if (patient) {
                this.api.getPharmacyByPatient(this.patientId).subscribe(prescriptions => {
                    this.items = prescriptions.map(p => this.mapItem(p));
                    this.loading.set(false);
                });
            } else {
                this.loading.set(false);
            }
        });
    }

    save(): void {
        this.saving.set(true);
        const updates: PharmacyDispenseUpdate[] = this.items.map(item => ({
            id: item.id,
            status: item.status,
            quantityDispensed: item.quantityDispensed,
            dispensationNote: item.dispensationNote,
            dispensed: item.status === DispensationStatus.Dispensed
        }));
        this.api.dispensePharmacy(updates).subscribe({
            next: () => {
                this.notifications.showSuccess('Dispensation updated.');
                this.router.navigate(['/patient-dashboard', this.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }

    private mapItem(p: PharmacyPrescription): DispenseItem {
        return {
            id: p.id,
            drugCategory: p.drugCategory,
            drugName: p.drugName,
            dosage: p.dosage,
            frequency: p.frequency,
            duration: p.duration,
            status: p.status ?? DispensationStatus.Pending,
            quantityDispensed: p.quantityDispensed,
            dispensationNote: p.dispensationNote
        };
    }
}
