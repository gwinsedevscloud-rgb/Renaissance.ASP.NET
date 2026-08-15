import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { Patient } from '../../core/models/clinical.models';

import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';

@Component({
    selector: 'app-client-form',
    imports: [
        ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule,
        MatInputModule, MatSelectModule, MatIconModule, MatProgressSpinnerModule,
        PageHeaderComponent, LoadingStateComponent
    ],
    templateUrl: './client-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientFormComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly fb = inject(FormBuilder);

    saving = signal(false);
    loading = signal(false);
    isEdit = false;
    patientId?: string;

    form = this.fb.group({
        fullName: ['', Validators.required],
        clientNumber: [{ value: '', disabled: true }],
        age: [null as number | null],
        ageUnit: ['Years'],
        sex: ['', Validators.required],
        maritalStatus: [''],
        tribe: [''],
        religion: [''],
        occupation: [''],
        education: [''],
        phoneNumber: [''],
        address: ['']
    });

    ngOnInit(): void {
        this.patientId = this.route.snapshot.paramMap.get('id') ?? undefined;
        this.isEdit = !!this.patientId && this.route.snapshot.url.some(s => s.path === 'edit');

        if (this.isEdit && this.patientId) {
            this.loading.set(true);
            this.api.getPatient(this.patientId).subscribe({
                next: patient => {
                    this.form.patchValue(patient);
                    this.loading.set(false);
                },
                error: () => this.loading.set(false)
            });
        } else if (!this.patientId) {
            this.api.generateClientNumber().subscribe(num => this.form.patchValue({ clientNumber: num }));
        }
    }

    save(): void {
        if (this.form.invalid) return;
        this.saving.set(true);
        const value = this.form.getRawValue() as Partial<Patient>;
        const payload = { ...value, id: this.patientId ?? crypto.randomUUID() } as Patient;

        const request$ = this.isEdit && this.patientId
            ? this.api.updatePatient(this.patientId, payload)
            : this.api.createPatient(payload);

        request$.subscribe({
            next: patient => {
                this.saving.set(false);
                this.router.navigate(['/client-dashboard', patient.id]);
            },
            error: () => this.saving.set(false)
        });
    }
}
