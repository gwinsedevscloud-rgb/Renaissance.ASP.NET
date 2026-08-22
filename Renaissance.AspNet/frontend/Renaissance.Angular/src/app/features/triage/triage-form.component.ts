import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Triage } from '../../core/models/clinical.models';

@Component({
    selector: 'app-triage-form',
    imports: [FormsModule, MatFormFieldModule, MatInputModule, MatCheckboxModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-grid">
            <section class="ren-form-section">
                <h3>Medical history</h3>
                <div class="ren-checkbox-row">
                    <mat-checkbox [(ngModel)]="model.diabetes" name="diabetes">Diabetes</mat-checkbox>
                    <mat-checkbox [(ngModel)]="model.asthma" name="asthma">Asthma</mat-checkbox>
                    <mat-checkbox [(ngModel)]="model.sickleCell" name="sickleCell">Sickle cell</mat-checkbox>
                    <mat-checkbox [(ngModel)]="model.smoking" name="smoking">Smoking</mat-checkbox>
                </div>
            </section>
            <section class="ren-form-section">
                <h3>Vitals</h3>
                <div class="ren-form-row">
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Weight (kg)</mat-label>
                        <input matInput type="number" [(ngModel)]="model.weight" name="weight" />
                    </mat-form-field>
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Height (cm)</mat-label>
                        <input matInput type="number" [(ngModel)]="model.height" name="height" />
                    </mat-form-field>
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Temperature (°C)</mat-label>
                        <input matInput type="number" step="0.1" [(ngModel)]="model.temperature" name="temperature" />
                    </mat-form-field>
                </div>
                <div class="ren-form-row">
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Systolic BP</mat-label>
                        <input matInput type="number" [(ngModel)]="model.systolicBp" name="systolicBp" />
                    </mat-form-field>
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Diastolic BP</mat-label>
                        <input matInput type="number" [(ngModel)]="model.diastolicBp" name="diastolicBp" />
                    </mat-form-field>
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Pulse (bpm)</mat-label>
                        <input matInput type="number" [(ngModel)]="model.pulseRate" name="pulseRate" />
                    </mat-form-field>
                    <mat-form-field subscriptSizing="dynamic">
                        <mat-label>Respiratory rate</mat-label>
                        <input matInput type="number" [(ngModel)]="model.respiratoryRate" name="respiratoryRate" />
                    </mat-form-field>
                </div>
            </section>
        </div>
    `
})
export class TriageFormComponent {
    @Input({ required: true }) model!: Triage;
}
