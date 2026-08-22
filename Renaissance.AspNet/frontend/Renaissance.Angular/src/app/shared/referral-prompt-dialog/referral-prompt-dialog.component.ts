import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AppModule, displayModuleName } from '../../core/models/auth.models';
import { ReferralPriority } from '../../core/models/referral.models';
import { HospitalModuleStateService } from '../../core/services/hospital-module-state.service';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { priorityClass, priorityLabel } from '../../core/utils/referral-display.helper';
import {
    REFERRAL_PRIORITY_OPTIONS,
    ReferralPromptDialogData,
    ReferralPromptDialogResult
} from './referral-prompt-dialog.models';

@Component({
    selector: 'app-referral-prompt-dialog',
    imports: [
        FormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressSpinnerModule
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <h2 mat-dialog-title>Refer to another module</h2>
        <p class="ren-referral-dialog-subtitle">
            From <span class="ren-chip">{{ displayModuleName(data.sourceModule) }}</span>
            for <span class="ren-chip">{{ data.patientName }}</span>
        </p>

        <mat-dialog-content class="ren-referral-dialog-content">
            <section class="ren-referral-dialog-section">
                <h3>Priority</h3>
                <div class="ren-referral-priority-group">
                    @for (option of priorityOptions; track option.priority) {
                        <button
                            type="button"
                            class="ren-referral-priority-option"
                            [class]="'ren-referral-priority-option ' + option.cssClass"
                            [class.is-selected]="priority === option.priority"
                            (click)="selectPriority(option.priority)">
                            <mat-icon [svgIcon]="option.icon"></mat-icon>
                            <span>
                                <strong>{{ option.label }}</strong>
                                <small>{{ option.hint }}</small>
                            </span>
                        </button>
                    }
                </div>
            </section>

            <section class="ren-referral-dialog-section">
                <h3>Destination modules</h3>
                <div class="ren-referral-target-grid">
                    @for (target of availableTargets; track target.module) {
                        <button
                            type="button"
                            class="ren-referral-target-card"
                            [class.selected]="selected.has(target.module)"
                            (click)="toggleTarget(target.module)">
                            <mat-icon [svgIcon]="target.icon"></mat-icon>
                            <span>{{ target.label }}</span>
                        </button>
                    }
                </div>
            </section>

            <section class="ren-referral-dialog-section">
                <h3>Notes <span class="ren-text-muted">(optional)</span></h3>
                <mat-form-field class="w-full" subscriptSizing="dynamic">
                    <textarea
                        matInput
                        rows="3"
                        [(ngModel)]="notes"
                        placeholder="e.g. elevated BP, needs dental review…"></textarea>
                </mat-form-field>
            </section>
        </mat-dialog-content>

        <mat-dialog-actions align="end">
            <button mat-stroked-button type="button" (click)="skip()">Skip for now</button>
            <button
                mat-flat-button
                color="primary"
                type="button"
                [disabled]="submitting || selected.size === 0"
                (click)="submit()">
                @if (submitting) {
                    <mat-spinner class="ren-btn-spinner" diameter="18"></mat-spinner>
                }
                Refer patient
            </button>
        </mat-dialog-actions>
    `
})
export class ReferralPromptDialogComponent {
    readonly data = inject<ReferralPromptDialogData>(MAT_DIALOG_DATA);
    private readonly dialogRef = inject(MatDialogRef<ReferralPromptDialogComponent, ReferralPromptDialogResult>);
    private readonly api = inject(RenaissanceApiService);
    private readonly moduleState = inject(HospitalModuleStateService);

    readonly displayModuleName = displayModuleName;
    readonly priorityOptions = REFERRAL_PRIORITY_OPTIONS;
    readonly priorityLabel = priorityLabel;
    readonly priorityClass = priorityClass;

    priority = ReferralPriority.Routine;
    notes = '';
    submitting = false;
    selected = new Set<AppModule>();

    get availableTargets() {
        return this.moduleState.filterNavItems(this.moduleState.getReferralTargets(this.data.sourceModule))
            .filter(target => target.module !== this.data.sourceModule);
    }

    selectPriority(priority: ReferralPriority): void {
        this.priority = priority;
    }

    toggleTarget(module: AppModule): void {
        if (this.selected.has(module)) {
            this.selected.delete(module);
        } else {
            this.selected.add(module);
        }
    }

    skip(): void {
        this.dialogRef.close('skipped');
    }

    submit(): void {
        if (this.selected.size === 0 || this.submitting) {
            return;
        }

        this.submitting = true;
        this.api.createReferrals({
            patientId: this.data.patientId,
            sourceModule: this.data.sourceModule,
            sourceRecordId: this.data.sourceRecordId,
            targetModules: [...this.selected],
            notes: this.notes.trim() || undefined,
            priority: this.priority
        }).subscribe({
            next: () => {
                this.submitting = false;
                this.dialogRef.close('completed');
            },
            error: () => {
                this.submitting = false;
            }
        });
    }
}
