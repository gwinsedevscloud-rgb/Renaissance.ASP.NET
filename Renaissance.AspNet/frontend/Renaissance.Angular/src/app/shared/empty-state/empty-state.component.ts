import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-empty-state',
    imports: [MatIconModule],
    template: `
        <div class="ren-empty-state">
            <div class="ren-empty-state-icon">
                <mat-icon [svgIcon]="icon"></mat-icon>
            </div>
            <h3 class="ren-empty-state-title">{{ title }}</h3>
            <p class="ren-empty-state-message">{{ message }}</p>
            <div class="ren-empty-state-actions">
                <ng-content></ng-content>
            </div>
        </div>
    `
})
export class EmptyStateComponent {
    @Input() icon = 'heroicons_outline:inbox';
    @Input() title = 'Nothing here yet';
    @Input() message = 'No records found.';
}
