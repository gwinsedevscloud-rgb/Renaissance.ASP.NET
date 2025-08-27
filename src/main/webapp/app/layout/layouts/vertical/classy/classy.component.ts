import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    inject,
    OnDestroy,
    OnInit,
    signal,
    ViewEncapsulation
} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import {
    FuseFullscreenComponent,
    FuseMediaWatcherService,
    FuseNavigationItem,
    FuseNavigationService,
    FuseScrollbarDirective,
    FuseScrollResetDirective,
    FuseVerticalNavigationComponent
} from '@mattae/angular-shared';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { NavigationService } from '../../../../services/navigation.service';
import { LanguagesComponent } from '../../../languages/languages.component';
import { ColorSchemeComponent } from '../../../color-scheme/color-scheme.component';

export interface Navigation {
    compact?: FuseNavigationItem[];
    default: FuseNavigationItem[];
    futuristic?: FuseNavigationItem[];
    horizontal?: FuseNavigationItem[];
}

@Component({
    selector: 'classy-layout',
    templateUrl: './classy.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FuseVerticalNavigationComponent,
        MatIconModule,
        MatButtonModule,
        FuseFullscreenComponent,
        RouterOutlet,
        FuseScrollbarDirective,
        MatSidenavModule,
        FuseScrollResetDirective,
        LanguagesComponent,
        ColorSchemeComponent
    ],

    styles: [
        `
            .app-background {
                background-color: var(--mat-app-background-color);
            }
        `
    ]
})
export class ClassyLayoutComponent implements OnInit, OnDestroy {
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private _fuseMediaWatcherService = inject(FuseMediaWatcherService);
    private _fuseNavigationService = inject(FuseNavigationService);

    isScreenSmall!: boolean
    navigation: any[] = [];
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    navigationService = inject(NavigationService);
    fuseScrollbarOptions = signal({});

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for current year
     */
    get currentYear(): number {
        return new Date().getFullYear();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    async ngOnInit() {


        // Subscribe to media changes
        this._fuseMediaWatcherService.onMediaChange$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(({matchingAliases}) => {

                // Check if the screen is small
                this.isScreenSmall = !matchingAliases.includes('md');
                this._changeDetectorRef.markForCheck();
            });

        this.navigation = this.navigationService.navigations();
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Toggle navigation
     *
     * @param name
     */
    toggleNavigation(name: string): void {
        // Get the navigation
        const navigation = this._fuseNavigationService.getComponent<FuseVerticalNavigationComponent>(name);

        if (navigation) {
            // Toggle the opened status
            navigation.toggle();
            this._changeDetectorRef.markForCheck();
        }
    }
}
