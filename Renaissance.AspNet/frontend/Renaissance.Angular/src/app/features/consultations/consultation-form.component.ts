import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Consultation } from '../../core/models/clinical.models';
import { listToText, textToList } from '../../core/utils/text-list.helper';

export interface ConsultationFormModel {
    patientId: string;
    diagnosesText: string;
    treatmentsText: string;
    referred: boolean;
    itnOrder: boolean;
    itnDispense: boolean;
}

@Component({
    selector: 'app-consultation-form',
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
            <div class="ren-checkbox-row">
                <mat-checkbox [(ngModel)]="model.referred" name="referred">Referred to another module</mat-checkbox>
                <mat-checkbox [(ngModel)]="model.itnOrder" name="itnOrder">ITN order</mat-checkbox>
                <mat-checkbox [(ngModel)]="model.itnDispense" name="itnDispense">ITN dispensed</mat-checkbox>
            </div>
        </div>
    `
})
export class ConsultationFormComponent implements OnChanges {
    @Input({ required: true }) model!: ConsultationFormModel;

    ngOnChanges(): void { /* model is bound by reference */ }

    static fromEntity(c: Consultation): ConsultationFormModel {
        return {
            patientId: c.patientId,
            diagnosesText: listToText(c.diagnoses),
            treatmentsText: listToText(c.treatments),
            referred: !!c.referred,
            itnOrder: !!c.itnOrder,
            itnDispense: !!c.itnDispense
        };
    }

    static toEntity(model: ConsultationFormModel, id: string): Consultation {
        return {
            id,
            patientId: model.patientId,
            diagnoses: textToList(model.diagnosesText),
            treatments: textToList(model.treatmentsText),
            referred: model.referred,
            itnOrder: model.itnOrder,
            itnDispense: model.itnDispense
        };
    }
}
