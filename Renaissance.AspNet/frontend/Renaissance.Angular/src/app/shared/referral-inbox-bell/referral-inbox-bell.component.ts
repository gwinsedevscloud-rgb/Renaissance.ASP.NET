import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { interval, Subject, switchMap, takeUntil } from 'rxjs';
import { displayModuleName, MODULE_NAV_CLINICAL } from '../../core/models/auth.models';
import { ReferralInboxItem, ReferralStatus } from '../../core/models/referral.models';
import { AuthStateService } from '../../core/services/auth-state.service';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { priorityClass, priorityLabel, statusLabel, waitLabel } from '../../core/utils/referral-display.helper';

@Component({
    selector: 'app-referral-inbox-bell',
    imports: [MatButtonModule, MatIconModule, MatMenuModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <button
            mat-icon-button
            type="button"
            [matMenuTriggerFor]="inboxMenu"
            aria-label="Referral inbox"
            (menuOpened)="loadInbox()">
            <mat-icon svgIcon="heroicons_outline:bell"></mat-icon>
            @if (unreadCount() > 0) {
                <span class="ren-referral-inbox-count">{{ unreadCount() > 99 ? '99+' : unreadCount() }}</span>
            }
        </button>

        <mat-menu #inboxMenu="matMenu" class="ren-referral-inbox-menu" xPosition="before">
            <div class="ren-referral-inbox-header" (click)="$event.stopPropagation()">
                <strong>Referrals</strong>
                @if (items().length > 0) {
                    <button mat-button type="button" (click)="markAllRead()">Mark all read</button>
                }
            </div>

            @if (loading() && items().length === 0) {
                <div class="ren-referral-inbox-empty">Loading…</div>
            } @else if (items().length === 0) {
                <div class="ren-referral-inbox-empty">No recent referrals for your modules.</div>
            } @else {
                <div class="ren-referral-inbox-list">
                    @for (item of items().slice(0, 12); track item.referralId) {
                        <button type="button" class="ren-referral-inbox-item" [class.unread]="!item.isRead" (click)="openItem(item)">
                            <div class="ren-referral-inbox-item-top">
                                <strong>{{ item.patientFullName }}</strong>
                                <span class="ren-referral-priority-chip" [class]="priorityClass(item.priority)">
                                    {{ priorityLabel(item.priority) }}
                                </span>
                            </div>
                            <small class="ren-referral-inbox-route">
                                {{ displayModuleName(item.sourceModule) }} → {{ displayModuleName(item.targetModule) }}
                            </small>
                            <small class="ren-referral-inbox-status" [class.pending]="item.status === ReferralStatus.Pending">
                                {{ statusLabel(item.status) }} · {{ waitLabel(item.waitMinutes) }}
                            </small>
                        </button>
                    }
                </div>
            }
        </mat-menu>
    `
})
export class ReferralInboxBellComponent implements OnInit, OnDestroy {
    private readonly api = inject(RenaissanceApiService);
    private readonly auth = inject(AuthStateService);
    private readonly router = inject(Router);
    private readonly destroy$ = new Subject<void>();

    readonly items = signal<ReferralInboxItem[]>([]);
    readonly unreadCount = signal(0);
    readonly loading = signal(false);

    readonly ReferralStatus = ReferralStatus;
    readonly displayModuleName = displayModuleName;
    readonly priorityLabel = priorityLabel;
    readonly priorityClass = priorityClass;
    readonly statusLabel = statusLabel;
    readonly waitLabel = waitLabel;

    ngOnInit(): void {
        if (!this.auth.isAuthenticated) {
            return;
        }

        this.loadInbox();
        interval(60_000)
            .pipe(
                takeUntil(this.destroy$),
                switchMap(() => this.api.getReferralInbox())
            )
            .subscribe(items => this.applyItems(items));
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadInbox(): void {
        if (!this.auth.isAuthenticated) {
            return;
        }

        this.loading.set(this.items().length === 0);
        this.api.getReferralInbox().subscribe(items => {
            this.applyItems(items);
            this.loading.set(false);
        });
    }

    openItem(item: ReferralInboxItem): void {
        this.api.markReferralRead(item.referralId).subscribe(() => {
            const href = MODULE_NAV_CLINICAL.find(m => m.module === item.targetModule)?.href ?? '/';
            void this.router.navigateByUrl(href);
            this.loadInbox();
        });
    }

    markAllRead(): void {
        this.api.markAllReferralsRead().subscribe(() => this.loadInbox());
    }

    private applyItems(items: ReferralInboxItem[]): void {
        this.items.set(items);
        this.unreadCount.set(
            items.filter(i => !i.isRead && (i.status === ReferralStatus.Pending || i.status === ReferralStatus.InProgress)).length
        );
    }
}
