import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-loading-state',
    imports: [MatProgressSpinnerModule, MatIconModule],
    template: `
        <div class="ren-loading">
            <div class="ren-loading-card">
                <mat-spinner [diameter]="diameter" color="primary"></mat-spinner>
                @if (message) {
                    <span class="ren-loading-message">{{ message }}</span>
                }
            </div>
        </div>
    `
})
export class LoadingStateComponent {
    @Input() message = 'Loading...';
    @Input() diameter = 44;
}
