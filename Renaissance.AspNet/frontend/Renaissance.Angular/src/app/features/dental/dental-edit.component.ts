import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { DentalFormComponent, DentalFormModel } from './dental-form.component';

@Component({
    selector: 'app-dental-edit',
    imports: [FormsModule, PageHeaderComponent, LoadingStateComponent, FormActionsComponent, DentalFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Edit Dental Consultation" icon="heroicons_outline:face-smile" backLink="/dental" backLabel="Dental" />
            @if (loading()) {
                <app-loading-state message="Loading record..." />
            } @else if (!recordId) {
                <div class="ren-empty-inline">Record not found.</div>
            } @else {
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-dental-form [model]="model" />
                    <app-form-actions saveLabel="Save Changes" [cancelHref]="'/patient-dashboard/' + model.patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class DentalEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    recordId = '';
    model: DentalFormModel = { patientId: '', diagnosesText: '', treatmentsText: '', referred: false };
    readonly loading = signal(true);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.recordId = this.route.snapshot.paramMap.get('id') ?? '';
        this.api.getDental(this.recordId).subscribe(d => {
            if (d) this.model = DentalFormComponent.fromEntity(d);
            this.loading.set(false);
        });
    }

    save(): void {
        if (!this.recordId) return;
        this.saving.set(true);
        this.api.updateDental(this.recordId, DentalFormComponent.toEntity(this.model, this.recordId)).subscribe({
            next: () => {
                this.notifications.showSuccess('Dental consultation updated.');
                this.router.navigate(['/patient-dashboard', this.model.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
