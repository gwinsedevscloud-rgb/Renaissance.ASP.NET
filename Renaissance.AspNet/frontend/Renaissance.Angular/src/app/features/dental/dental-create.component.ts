import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Patient } from '../../core/models/clinical.models';
import { AppModule } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { ReferralPromptDialogComponent } from '../../shared/referral-prompt-dialog/referral-prompt-dialog.component';
import { DentalFormComponent, DentalFormModel } from './dental-form.component';

@Component({
    selector: 'app-dental-create',
    imports: [FormsModule, PageHeaderComponent, FormActionsComponent, DentalFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            @if (!exists()) {
                <div class="ren-empty-inline">Patient not found.</div>
            } @else {
                <app-page-header title="New Dental Consultation" icon="heroicons_outline:face-smile" />
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-dental-form [model]="model" />
                    <app-form-actions saveLabel="Save Consultation" [cancelHref]="'/patient-dashboard/' + patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class DentalCreateComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly dialog = inject(MatDialog);
    private readonly notifications = inject(NotificationService);

    patientId = '';
    model: DentalFormModel = { patientId: '', diagnosesText: '', treatmentsText: '', referred: false };
    patient: Patient | null = null;
    readonly exists = signal(false);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.patientId = this.route.snapshot.paramMap.get('patientId') ?? '';
        this.model.patientId = this.patientId;
        this.api.getPatient(this.patientId).subscribe(p => {
            this.patient = p;
            this.exists.set(!!p);
        });
    }

    save(): void {
        this.saving.set(true);
        const id = crypto.randomUUID();
        this.api.createDental(DentalFormComponent.toEntity(this.model, id)).subscribe({
            next: () => {
                this.saving.set(false);
                this.notifications.showSuccess('Dental consultation saved.');
                if (this.model.referred) {
                    this.dialog.open(ReferralPromptDialogComponent, {
                        width: '560px',
                        data: {
                            patientId: this.patientId,
                            patientName: this.patient?.fullName ?? 'the patient',
                            sourceModule: AppModule.Dental,
                            sourceRecordId: id
                        }
                    }).afterClosed().subscribe(() => this.router.navigate(['/patient-dashboard', this.patientId]));
                } else {
                    this.router.navigate(['/patient-dashboard', this.patientId]);
                }
            },
            error: () => this.saving.set(false)
        });
    }
}
