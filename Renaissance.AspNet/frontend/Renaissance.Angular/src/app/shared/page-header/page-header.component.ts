import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-page-header',
    imports: [MatIconModule, RouterLink],
    template: `
        <div class="ren-page-header">
            <div class="ren-page-header-main">
                @if (backLink) {
                    <a class="ren-breadcrumb" [routerLink]="backLink">
                        <mat-icon [svgIcon]="'heroicons_outline:arrow-left'"></mat-icon>
                        <span>{{ backLabel }}</span>
                    </a>
                }
                @if (icon) {
                    <div class="ren-page-header-title-row">
                        <span class="ren-header-icon"><mat-icon [svgIcon]="icon"></mat-icon></span>
                        <h1>{{ title }}</h1>
                    </div>
                } @else {
                    <h1>{{ title }}</h1>
                }
                @if (subtitle) {
                    <p class="subtitle">{{ subtitle }}</p>
                }
            </div>
            <div class="ren-page-header-actions">
                <ng-content></ng-content>
            </div>
        </div>
    `
})
export class PageHeaderComponent {
    @Input({ required: true }) title!: string;
    @Input() subtitle?: string;
    @Input() icon?: string;
    @Input() backLink?: string;
    @Input() backLabel = 'Back';
}
