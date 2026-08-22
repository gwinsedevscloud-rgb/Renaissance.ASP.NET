import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Patient } from '../../core/models/clinical.models';
import { clientDisplayName, patientInitials } from '../../core/utils/list-text.helper';

@Component({
    selector: 'app-patient-identity',
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-table-client">
            @if (!patient) {
                <span class="ren-text-muted">—</span>
            } @else {
                <span class="ren-client-avatar">{{ initials }}</span>
                <span>
                    <strong>{{ displayName }}</strong>
                    <span class="ren-client-number">{{ patient.clientNumber }}</span>
                </span>
            }
        </div>
    `
})
export class PatientIdentityComponent {
    @Input() patient?: Patient | null;

    get displayName(): string {
        return this.patient
            ? clientDisplayName(this.patient.fullName, this.patient.address)
            : '—';
    }

    get initials(): string {
        return this.patient
            ? patientInitials(this.patient.fullName, this.patient.address, this.patient.clientNumber)
            : 'CL';
    }
}
