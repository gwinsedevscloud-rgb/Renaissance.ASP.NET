import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Patient, PharmacyPrescription } from '../../core/models/clinical.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';

@Component({
    selector: 'app-pharmacy-create',
    imports: [FormsModule, MatFormFieldModule, MatInputModule, PageHeaderComponent, FormActionsComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            @if (!exists()) {
                <div class="ren-empty-inline">Patient not found.</div>
            } @else {
                <app-page-header title="New Prescription" subtitle="Add medication for dispensing" icon="heroicons_outline:beaker" />
                <form class="ren-card ren-card-body ren-form-grid" (ngSubmit)="save()">
                    <div class="ren-form-row">
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Drug category</mat-label>
                            <input matInput [(ngModel)]="model.drugCategory" name="drugCategory" />
                        </mat-form-field>
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Drug name</mat-label>
                            <input matInput [(ngModel)]="model.drugName" name="drugName" required />
                        </mat-form-field>
                    </div>
                    <div class="ren-form-row">
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Dosage</mat-label>
                            <input matInput [(ngModel)]="model.dosage" name="dosage" />
                        </mat-form-field>
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Frequency</mat-label>
                            <input matInput [(ngModel)]="model.frequency" name="frequency" />
                        </mat-form-field>
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Duration</mat-label>
                            <input matInput [(ngModel)]="model.duration" name="duration" />
                        </mat-form-field>
                    </div>
                    <app-form-actions saveLabel="Save Prescription" [cancelHref]="'/patient-dashboard/' + patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class PharmacyCreateComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    patientId = '';
    model: PharmacyPrescription = { id: '', patientId: '' };
    readonly exists = signal(false);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.patientId = this.route.snapshot.paramMap.get('patientId') ?? '';
        this.model.patientId = this.patientId;
        this.api.getPatient(this.patientId).subscribe(p => this.exists.set(!!p));
    }

    save(): void {
        this.saving.set(true);
        this.model.id = crypto.randomUUID();
        this.api.createPharmacy(this.model).subscribe({
            next: () => {
                this.notifications.showSuccess('Prescription saved.');
                this.router.navigate(['/patient-dashboard', this.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
