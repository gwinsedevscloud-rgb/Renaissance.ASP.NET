import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Ancillary } from '../../core/models/clinical.models';
import { listToText, textToList } from '../../core/utils/text-list.helper';

export interface AncillaryFormModel {
    patientId: string;
    servicesText: string;
    pregnancyStatus: string;
}

@Component({
    selector: 'app-ancillary-form',
    imports: [FormsModule, MatFormFieldModule, MatInputModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-grid">
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Services (one per line)</mat-label>
                <textarea matInput rows="4" [(ngModel)]="model.servicesText" name="services"></textarea>
            </mat-form-field>
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Pregnancy status</mat-label>
                <input matInput [(ngModel)]="model.pregnancyStatus" name="pregnancyStatus" />
            </mat-form-field>
        </div>
    `
})
export class AncillaryFormComponent {
    @Input({ required: true }) model!: AncillaryFormModel;

    static fromEntity(a: Ancillary): AncillaryFormModel {
        return {
            patientId: a.patientId,
            servicesText: listToText(a.services),
            pregnancyStatus: a.pregnancyStatus ?? ''
        };
    }

    static toEntity(model: AncillaryFormModel, id: string): Ancillary {
        return {
            id,
            patientId: model.patientId,
            services: textToList(model.servicesText),
            pregnancyStatus: model.pregnancyStatus || undefined
        };
    }
}
