import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { AncillaryFormComponent, AncillaryFormModel } from './ancillary-form.component';

@Component({
    selector: 'app-ancillary-edit',
    imports: [FormsModule, PageHeaderComponent, LoadingStateComponent, FormActionsComponent, AncillaryFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Edit Ancillary Record" icon="heroicons_outline:shield-check" backLink="/ancillary" backLabel="Ancillary" />
            @if (loading()) {
                <app-loading-state message="Loading record..." />
            } @else if (!recordId) {
                <div class="ren-empty-inline">Record not found.</div>
            } @else {
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-ancillary-form [model]="model" />
                    <app-form-actions saveLabel="Save Changes" [cancelHref]="'/patient-dashboard/' + model.patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class AncillaryEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    recordId = '';
    model: AncillaryFormModel = { patientId: '', servicesText: '', pregnancyStatus: '' };
    readonly loading = signal(true);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.recordId = this.route.snapshot.paramMap.get('id') ?? '';
        this.api.getAncillary(this.recordId).subscribe(a => {
            if (a) this.model = AncillaryFormComponent.fromEntity(a);
            this.loading.set(false);
        });
    }

    save(): void {
        if (!this.recordId) return;
        this.saving.set(true);
        this.api.updateAncillary(this.recordId, AncillaryFormComponent.toEntity(this.model, this.recordId)).subscribe({
            next: () => {
                this.notifications.showSuccess('Ancillary record updated.');
                this.router.navigate(['/patient-dashboard', this.model.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
