import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { ConsultationFormComponent, ConsultationFormModel } from './consultation-form.component';

@Component({
    selector: 'app-consultation-edit',
    imports: [FormsModule, PageHeaderComponent, LoadingStateComponent, FormActionsComponent, ConsultationFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Edit Consultation" icon="heroicons_outline:clipboard-document-list" backLink="/consultations" backLabel="Consultations" />
            @if (loading()) {
                <app-loading-state message="Loading consultation..." />
            } @else if (!recordId) {
                <div class="ren-empty-inline">Consultation not found.</div>
            } @else {
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-consultation-form [model]="model" />
                    <app-form-actions saveLabel="Save Changes" [cancelHref]="'/patient-dashboard/' + model.patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class ConsultationEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    recordId = '';
    model: ConsultationFormModel = { patientId: '', diagnosesText: '', treatmentsText: '', referred: false, itnOrder: false, itnDispense: false };
    readonly loading = signal(true);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.recordId = this.route.snapshot.paramMap.get('id') ?? '';
        this.api.getConsultation(this.recordId).subscribe(c => {
            if (c) this.model = ConsultationFormComponent.fromEntity(c);
            this.loading.set(false);
        });
    }

    save(): void {
        if (!this.recordId) return;
        this.saving.set(true);
        this.api.updateConsultation(this.recordId, ConsultationFormComponent.toEntity(this.model, this.recordId)).subscribe({
            next: () => {
                this.notifications.showSuccess('Consultation updated.');
                this.router.navigate(['/patient-dashboard', this.model.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
