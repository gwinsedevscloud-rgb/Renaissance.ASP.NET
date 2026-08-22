import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Laboratory } from '../../core/models/clinical.models';

@Component({
    selector: 'app-laboratory-form',
    imports: [FormsModule, MatFormFieldModule, MatInputModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-grid">
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Test name</mat-label>
                <input matInput [(ngModel)]="model.testName" name="testName" required />
            </mat-form-field>
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Result</mat-label>
                <input matInput [(ngModel)]="model.result" name="result" />
            </mat-form-field>
            <mat-form-field class="w-full" subscriptSizing="dynamic">
                <mat-label>Note</mat-label>
                <textarea matInput rows="3" [(ngModel)]="model.note" name="note"></textarea>
            </mat-form-field>
        </div>
    `
})
export class LaboratoryFormComponent {
    @Input({ required: true }) model!: Laboratory;
}
