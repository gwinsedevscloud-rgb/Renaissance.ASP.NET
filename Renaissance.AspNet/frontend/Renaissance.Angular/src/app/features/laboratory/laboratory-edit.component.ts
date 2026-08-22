import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Laboratory } from '../../core/models/clinical.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { LaboratoryFormComponent } from './laboratory-form.component';

@Component({
    selector: 'app-laboratory-edit',
    imports: [FormsModule, PageHeaderComponent, LoadingStateComponent, FormActionsComponent, LaboratoryFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Edit Laboratory Test" icon="heroicons_outline:beaker" backLink="/laboratory" backLabel="Laboratory" />
            @if (loading()) {
                <app-loading-state message="Loading test..." />
            } @else if (!model) {
                <div class="ren-empty-inline">Record not found.</div>
            } @else {
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-laboratory-form [model]="model" />
                    <app-form-actions saveLabel="Save Changes" [cancelHref]="'/patient-dashboard/' + model.patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class LaboratoryEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    model: Laboratory | null = null;
    readonly loading = signal(true);
    readonly saving = signal(false);

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id') ?? '';
        this.api.getLaboratory(id).subscribe(l => {
            this.model = l;
            this.loading.set(false);
        });
    }

    save(): void {
        if (!this.model) return;
        this.saving.set(true);
        this.api.updateLaboratory(this.model.id, this.model).subscribe({
            next: () => {
                this.notifications.showSuccess('Laboratory test updated.');
                this.router.navigate(['/patient-dashboard', this.model!.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
