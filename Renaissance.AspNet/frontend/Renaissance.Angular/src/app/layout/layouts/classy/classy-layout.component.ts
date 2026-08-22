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
import { NavigationService } from '../../../core/services/navigation.service';
import { AuthStateService } from '../../../core/services/auth-state.service';
import { RenaissanceAccountService } from '../../../core/services/renaissance-account.service';
import { ReferralInboxBellComponent } from '../../../shared/referral-inbox-bell/referral-inbox-bell.component';

@Component({
    selector: 'classy-layout',
    templateUrl: './classy-layout.component.html',
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
        ReferralInboxBellComponent
    ],
    styles: [`.app-background { background-color: var(--mat-app-background-color); }`]
})
export class ClassyLayoutComponent implements OnInit, OnDestroy {
    private readonly changeDetectorRef = inject(ChangeDetectorRef);
    private readonly fuseMediaWatcherService = inject(FuseMediaWatcherService);
    private readonly fuseNavigationService = inject(FuseNavigationService);
    private readonly navigationService = inject(NavigationService);
    private readonly auth = inject(AuthStateService);
    private readonly account = inject(RenaissanceAccountService);
    private readonly destroy$ = new Subject<void>();

    isScreenSmall = false;
    navigation: FuseNavigationItem[] = [];
    fuseScrollbarOptions = signal({});
    userName = signal('Clinical User');
    userRole = signal('Renaissance EMR');

    get currentYear(): number {
        return new Date().getFullYear();
    }

    ngOnInit(): void {
        this.fuseMediaWatcherService.onMediaChange$
            .pipe(takeUntil(this.destroy$))
            .subscribe(({ matchingAliases }) => {
                this.isScreenSmall = !matchingAliases.includes('md');
                this.changeDetectorRef.markForCheck();
            });

        this.navigation = this.navigationService.navigations();
        this.auth.user$.pipe(takeUntil(this.destroy$)).subscribe(user => {
            this.userName.set(user?.fullName ?? 'Clinical User');
            this.userRole.set(user?.roleName ?? 'Renaissance EMR');
            this.navigation = this.navigationService.navigations();
            this.changeDetectorRef.markForCheck();
        });
    }

    logout(): void {
        this.account.logout();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    toggleNavigation(name: string): void {
        const navigation = this.fuseNavigationService.getComponent<FuseVerticalNavigationComponent>(name);
        if (navigation) {
            navigation.toggle();
            this.changeDetectorRef.markForCheck();
        }
    }
}
