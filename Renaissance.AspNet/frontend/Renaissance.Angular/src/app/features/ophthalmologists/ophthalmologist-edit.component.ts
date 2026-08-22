import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { OphthalmologistFormComponent, OphthalmologistFormModel } from './ophthalmologist-form.component';

@Component({
    selector: 'app-ophthalmologist-edit',
    imports: [FormsModule, PageHeaderComponent, LoadingStateComponent, FormActionsComponent, OphthalmologistFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Edit Ophthalmology Record" icon="heroicons_outline:eye" backLink="/ophthalmologists" backLabel="Ophthalmologists" />
            @if (loading()) {
                <app-loading-state message="Loading record..." />
            } @else if (!recordId) {
                <div class="ren-empty-inline">Record not found.</div>
            } @else {
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-ophthalmologist-form [model]="model" />
                    <app-form-actions saveLabel="Save Changes" [cancelHref]="'/patient-dashboard/' + model.patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class OphthalmologistEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    recordId = '';
    model: OphthalmologistFormModel = {
        patientId: '', diagnosesText: '', treatmentsText: '', surgeriesText: '',
        visualAcuityRight: '', visualAcuityLeft: '', glassesDispensed: false, referred: false
    };
    readonly loading = signal(true);
    readonly saving = signal(false);

    ngOnInit(): void {
        this.recordId = this.route.snapshot.paramMap.get('id') ?? '';
        this.api.getOphthalmologist(this.recordId).subscribe(o => {
            if (o) this.model = OphthalmologistFormComponent.fromEntity(o);
            this.loading.set(false);
        });
    }

    save(): void {
        if (!this.recordId) return;
        this.saving.set(true);
        this.api.updateOphthalmologist(this.recordId, OphthalmologistFormComponent.toEntity(this.model, this.recordId)).subscribe({
            next: () => {
                this.notifications.showSuccess('Ophthalmology record updated.');
                this.router.navigate(['/patient-dashboard', this.model.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
