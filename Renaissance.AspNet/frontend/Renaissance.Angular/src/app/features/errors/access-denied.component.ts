import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';

@Component({
    selector: 'app-access-denied',
    imports: [RouterLink, MatButtonModule, MatIconModule, EmptyStateComponent],
    template: `
        <div class="ren-page">
            <app-empty-state
                icon="heroicons_outline:shield-exclamation"
                title="You do not have access to this module"
                message="Ask an administrator to grant this module to your role.">
                <a mat-flat-button color="primary" routerLink="/">
                    <mat-icon svgIcon="heroicons_outline:home"></mat-icon>
                    Back to home
                </a>
            </app-empty-state>
        </div>
    `
})
export class AccessDeniedComponent {}
