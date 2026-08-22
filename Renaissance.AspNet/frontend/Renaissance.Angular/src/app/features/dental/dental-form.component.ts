import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { DentalConsultation } from '../../core/models/clinical.models';
import { listToText, textToList } from '../../core/utils/text-list.helper';

export interface DentalFormModel {
    patientId: string;
    diagnosesText: string;
    treatmentsText: string;
    referred: boolean;
}

@Component({
    selector: 'app-dental-form',
    imports: [FormsModule, MatFormFieldModule, MatInputModule, MatCheckboxModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-grid">
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Diagnoses (one per line)</mat-label>
                <textarea matInput rows="4" [(ngModel)]="model.diagnosesText" name="diagnoses"></textarea>
            </mat-form-field>
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Treatments (one per line)</mat-label>
                <textarea matInput rows="4" [(ngModel)]="model.treatmentsText" name="treatments"></textarea>
            </mat-form-field>
            <mat-checkbox [(ngModel)]="model.referred" name="referred">Referred to another module</mat-checkbox>
        </div>
    `
})
export class DentalFormComponent {
    @Input({ required: true }) model!: DentalFormModel;

    static fromEntity(d: DentalConsultation): DentalFormModel {
        return {
            patientId: d.patientId,
            diagnosesText: listToText(d.diagnoses),
            treatmentsText: listToText(d.treatments),
            referred: !!d.referred
        };
    }

    static toEntity(model: DentalFormModel, id: string): DentalConsultation {
        return {
            id,
            patientId: model.patientId,
            diagnoses: textToList(model.diagnosesText),
            treatments: textToList(model.treatmentsText),
            referred: model.referred
        };
    }
}
