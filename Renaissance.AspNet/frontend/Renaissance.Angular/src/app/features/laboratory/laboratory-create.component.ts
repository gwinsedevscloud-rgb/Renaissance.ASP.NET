import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Laboratory } from '../../core/models/clinical.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';
import { LaboratoryFormComponent } from './laboratory-form.component';

@Component({
    selector: 'app-laboratory-create',
    imports: [FormsModule, PageHeaderComponent, FormActionsComponent, LaboratoryFormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            @if (!exists()) {
                <div class="ren-empty-inline">Patient not found.</div>
            } @else {
                <app-page-header title="New Laboratory Test" icon="heroicons_outline:beaker" />
                <form class="ren-card ren-card-body" (ngSubmit)="save()">
                    <app-laboratory-form [model]="model" />
                    <app-form-actions saveLabel="Save Test" [cancelHref]="'/patient-dashboard/' + patientId" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class LaboratoryCreateComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    patientId = '';
    model: Laboratory = { id: '', patientId: '' };
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
        this.api.createLaboratory(this.model).subscribe({
            next: () => {
                this.notifications.showSuccess('Laboratory test saved.');
                this.router.navigate(['/patient-dashboard', this.patientId]);
            },
            error: () => this.saving.set(false),
            complete: () => this.saving.set(false)
        });
    }
}
