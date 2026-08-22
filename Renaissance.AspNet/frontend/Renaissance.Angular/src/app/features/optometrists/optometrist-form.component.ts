import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Optometrist } from '../../core/models/clinical.models';

@Component({
    selector: 'app-optometrist-form',
    imports: [FormsModule, MatFormFieldModule, MatInputModule, MatCheckboxModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-grid">
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
                <mat-checkbox [(ngModel)]="model.referred" name="referred">Referred to ophthalmologist</mat-checkbox>
            </div>
        </div>
    `
})
export class OptometristFormComponent {
    @Input({ required: true }) model!: Optometrist;
}
