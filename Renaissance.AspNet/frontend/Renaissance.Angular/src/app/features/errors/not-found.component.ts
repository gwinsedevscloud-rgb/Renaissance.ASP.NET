import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';

@Component({
    selector: 'app-not-found',
    imports: [RouterLink, MatButtonModule, MatIconModule, EmptyStateComponent],
    template: `
        <div class="ren-page">
            <app-empty-state
                icon="heroicons_outline:map"
                title="Page not found"
                message="The page you requested does not exist.">
                <a mat-flat-button color="primary" routerLink="/">
                    <mat-icon svgIcon="heroicons_outline:home"></mat-icon>
                    Back to home
                </a>
            </app-empty-state>
        </div>
    `
})
export class NotFoundComponent {}
