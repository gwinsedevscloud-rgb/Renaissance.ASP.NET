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
import { ConsultationFormComponent, ConsultationFormModel } from './consultation-form.component';

@Component({
    selector: 'app-consultation-create',
    imports: [FormsModule, PageHeaderComponent, FormActionsComponent, ConsultationFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            @if (!exists()) {
                <div class="ren-empty-inline">Patient not found.</div>
            } @else {
                <app-page-header title="New Consultation" subtitle="Record diagnosis, treatment, and referrals" icon="heroicons_outline:clipboard-document-list" />
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-consultation-form [model]="model" />
                    <app-form-actions saveLabel="Save Consultation" [cancelHref]="'/patient-dashboard/' + patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class ConsultationCreateComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly dialog = inject(MatDialog);
    private readonly notifications = inject(NotificationService);

    patientId = '';
    model: ConsultationFormModel = { patientId: '', diagnosesText: '', treatmentsText: '', referred: false, itnOrder: false, itnDispense: false };
    patient: Patient | null = null;
    readonly exists = signal(false);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.patientId = this.route.snapshot.paramMap.get('patientId') ?? '';
        this.model.patientId = this.patientId;
        this.api.getPatient(this.patientId).subscribe(patient => {
            this.patient = patient;
            this.exists.set(!!patient);
        });
    }

    save(): void {
        this.saving.set(true);
        const id = crypto.randomUUID();
        const entity = ConsultationFormComponent.toEntity(this.model, id);
        this.api.createConsultation(entity).subscribe({
            next: () => {
                this.saving.set(false);
                const parts = ['Consultation saved.'];
                if (this.model.itnOrder) parts.push('ITN added to pharmacy queue.');
                if (this.model.itnDispense) parts.push('ITN marked dispensed.');
                this.notifications.showSuccess(parts.join(' '));
                if (this.model.referred) {
                    this.dialog.open(ReferralPromptDialogComponent, {
                        width: '560px',
                        data: {
                            patientId: this.patientId,
                            patientName: this.patient?.fullName ?? 'the patient',
                            sourceModule: AppModule.Consultations,
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
