import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-form-actions',
    imports: [RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-form-footer">
            <button type="submit" mat-flat-button color="primary" [disabled]="saving">
                @if (saving) {
                    <mat-spinner class="ren-btn-spinner" diameter="18"></mat-spinner>
                } @else {
                    <ng-container>
                        <mat-icon svgIcon="heroicons_outline:check"></mat-icon>
                    </ng-container>
                }
                {{ saveLabel }}
            </button>
            @if (cancelHref) {
                <a mat-stroked-button [routerLink]="cancelHref">
                    <mat-icon svgIcon="heroicons_outline:x-mark"></mat-icon>
                    Cancel
                </a>
            }
            <ng-content></ng-content>
        </div>
    `
})
export class FormActionsComponent {
    @Input() saveLabel = 'Save';
    @Input() cancelHref?: string;
    @Input() saving = false;
}
