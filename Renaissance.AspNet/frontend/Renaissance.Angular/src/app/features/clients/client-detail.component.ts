import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { Patient } from '../../core/models/clinical.models';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';

interface FieldGroup {
    title: string;
    icon: string;
    iconClass: string;
    fields: { label: string; value?: string }[];
}

@Component({
    selector: 'app-client-detail',
    imports: [RouterLink, MatButtonModule, MatIconModule, LoadingStateComponent],
    templateUrl: './client-detail.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientDetailComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);

    patient = signal<Patient | null>(null);
    loading = signal(true);

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id')!;
        this.api.getPatient(id).subscribe({
            next: p => { this.patient.set(p); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    fieldGroups(p: Patient): FieldGroup[] {
        return [
            {
                title: 'Contact',
                icon: 'heroicons_outline:phone',
                iconClass: 'ren-section-icon-blue',
                fields: [
                    { label: 'Phone', value: p.phoneNumber },
                    { label: 'Address', value: p.address }
                ]
            },
            {
                title: 'Personal background',
                icon: 'heroicons_outline:user-circle',
                iconClass: 'ren-section-icon-violet',
                fields: [
                    { label: 'Marital Status', value: p.maritalStatus },
                    { label: 'Tribe', value: p.tribe },
                    { label: 'Religion', value: p.religion },
                    { label: 'Occupation', value: p.occupation },
                    { label: 'Education', value: p.education }
                ]
            }
        ];
    }
}
