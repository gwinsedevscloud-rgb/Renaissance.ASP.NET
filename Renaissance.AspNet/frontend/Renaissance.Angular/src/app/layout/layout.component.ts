import { AsyncPipe } from '@angular/common';
import { Component, ViewEncapsulation, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { map, shareReplay } from 'rxjs';

@Component({
    selector: 'layout',
    templateUrl: './layout.component.html',
    encapsulation: ViewEncapsulation.None,
    imports: [
        AsyncPipe,
        RouterOutlet,
        RouterLink,
        RouterLinkActive,
        MatIconModule,
        MatButtonModule,
        MatSidenavModule,
        MatListModule,
        MatToolbarModule
    ]
})
export class LayoutComponent {
    private readonly breakpointObserver = inject(BreakpointObserver);

    navItems = [
        { title: 'Home', link: '/', icon: 'heroicons_outline:home' },
        { title: 'Clients', link: '/clients', icon: 'heroicons_outline:users' },
        { title: 'Stakeholders', link: '/stakeholders', icon: 'heroicons_outline:chart-bar' }
    ];

    isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset).pipe(
        map(result => result.matches),
        shareReplay({ bufferSize: 1, refCount: true })
    );

    get currentYear(): number {
        return new Date().getFullYear();
    }

    closeDrawerOnMobile(drawer: MatSidenav): void {
        if (this.breakpointObserver.isMatched(Breakpoints.Handset)) {
            drawer.close();
        }
    }
}
