import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Ophthalmologist } from '../../core/models/clinical.models';
import { listToText, textToList } from '../../core/utils/text-list.helper';

export interface OphthalmologistFormModel {
    patientId: string;
    diagnosesText: string;
    treatmentsText: string;
    surgeriesText: string;
    visualAcuityRight: string;
    visualAcuityLeft: string;
    glassesDispensed: boolean;
    referred: boolean;
}

@Component({
    selector: 'app-ophthalmologist-form',
    imports: [FormsModule, MatFormFieldModule, MatInputModule, MatCheckboxModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-grid">
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Diagnoses (one per line)</mat-label>
                <textarea matInput rows="3" [(ngModel)]="model.diagnosesText" name="diagnoses"></textarea>
            </mat-form-field>
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Treatments (one per line)</mat-label>
                <textarea matInput rows="3" [(ngModel)]="model.treatmentsText" name="treatments"></textarea>
            </mat-form-field>
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Surgeries (one per line)</mat-label>
                <textarea matInput rows="3" [(ngModel)]="model.surgeriesText" name="surgeries"></textarea>
            </mat-form-field>
            <div class="ren-form-row">
                <mat-form-field subscriptSizing="dynamic">
                    <mat-label>Visual acuity (right)</mat-label>
                    <input matInput [(ngModel)]="model.visualAcuityRight" name="vaRight" />
                </mat-form-field>
                <mat-form-field subscriptSizing="dynamic">
                    <mat-label>Visual acuity (left)</mat-label>
                    <input matInput [(ngModel)]="model.visualAcuityLeft" name="vaLeft" />
                </mat-form-field>
            </div>
            <div class="ren-checkbox-row">
                <mat-checkbox [(ngModel)]="model.glassesDispensed" name="glasses">Glasses dispensed</mat-checkbox>
                <mat-checkbox [(ngModel)]="model.referred" name="referred">Referred</mat-checkbox>
            </div>
        </div>
    `
})
export class OphthalmologistFormComponent {
    @Input({ required: true }) model!: OphthalmologistFormModel;

    static fromEntity(o: Ophthalmologist): OphthalmologistFormModel {
        return {
            patientId: o.patientId,
            diagnosesText: listToText(o.diagnoses),
            treatmentsText: listToText(o.treatments),
            surgeriesText: listToText(o.surgeries),
            visualAcuityRight: o.visualAcuityRight ?? '',
            visualAcuityLeft: o.visualAcuityLeft ?? '',
            glassesDispensed: !!o.glassesDispensed,
            referred: !!o.referred
        };
    }

    static toEntity(model: OphthalmologistFormModel, id: string): Ophthalmologist {
        return {
            id,
            patientId: model.patientId,
            diagnoses: textToList(model.diagnosesText),
            treatments: textToList(model.treatmentsText),
            surgeries: textToList(model.surgeriesText),
            visualAcuityRight: model.visualAcuityRight || undefined,
            visualAcuityLeft: model.visualAcuityLeft || undefined,
            glassesDispensed: model.glassesDispensed,
            referred: model.referred
        };
    }
}
