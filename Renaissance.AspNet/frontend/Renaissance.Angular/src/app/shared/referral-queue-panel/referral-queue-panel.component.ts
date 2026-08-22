import { ChangeDetectionStrategy, Component, inject, Input, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AppModule, displayModuleName, queueActionHref } from '../../core/models/auth.models';
import { ReferralQueueItem, ReferralStatus } from '../../core/models/referral.models';
import { AuthStateService } from '../../core/services/auth-state.service';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { LoadingStateComponent } from '../loading-state/loading-state.component';
import { priorityLabel } from '../../core/utils/referral-display.helper';

@Component({
    selector: 'app-referral-queue-panel',
    imports: [RouterLink, MatButtonModule, MatIconModule, LoadingStateComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div
            class="ren-referral-queue-panel"
            [class.has-items]="items().length > 0"
            [class.is-expanded]="expanded()"
            [class.is-collapsed]="!expanded()">
            <button type="button" class="ren-referral-queue-header" (click)="toggleExpanded()" [attr.aria-expanded]="expanded()">
                <div class="ren-referral-queue-title">
                    <span class="ren-referral-queue-icon">
                        <mat-icon svgIcon="heroicons_outline:inbox"></mat-icon>
                    </span>
                    <div>
                        <h2>Referral queue</h2>
                        @if (expanded() || pendingCount() > 0) {
                            <p>Priority first, then first come first served</p>
                        } @else {
                            <p class="ren-referral-queue-collapsed-hint">Queue empty — expand to view details</p>
                        }
                    </div>
                </div>
                <div class="ren-referral-queue-header-badges">
                    @if (pendingCount() > 0) {
                        <span class="ren-referral-queue-badge">{{ pendingCount() }} waiting</span>
                    }
                    <mat-icon class="ren-referral-queue-chevron" [svgIcon]="expanded() ? 'heroicons_outline:chevron-up' : 'heroicons_outline:chevron-down'"></mat-icon>
                </div>
            </button>

            @if (expanded()) {
                @if (loading()) {
                    <app-loading-state message="Loading referral queue..." />
                } @else if (items().length === 0) {
                    <div class="ren-referral-queue-empty">
                        <mat-icon svgIcon="heroicons_outline:inbox"></mat-icon>
                        <p>No patients waiting. Referrals appear here automatically.</p>
                    </div>
                } @else {
                    <div class="ren-referral-queue-list">
                        @for (item of items(); track item.id) {
                            <article
                                class="ren-referral-queue-item"
                                [class.pending]="item.status === ReferralStatus.Pending"
                                [class.in-progress]="item.status === ReferralStatus.InProgress"
                                [class.sla-breached]="item.isSlaBreached">
                                <div class="ren-referral-queue-position">{{ item.queuePosition }}</div>
                                <div class="ren-referral-queue-body">
                                    <div class="ren-referral-queue-top">
                                        <strong>{{ item.patientFullName }}</strong>
                                        <span class="ren-client-number">{{ item.clientNumber }}</span>
                                        <span class="ren-referral-priority-chip">{{ priorityLabel(item.priority) }}</span>
                                    </div>
                                    <div class="ren-referral-queue-meta">
                                        <span [class.ren-text-danger]="item.isSlaBreached">
                                            {{ item.waitMinutes }} min wait
                                        </span>
                                        <span>from {{ displayModuleName(item.sourceModule) }}</span>
                                        @if (item.referredBy) {
                                            <span>by {{ item.referredBy }}</span>
                                        }
                                    </div>
                                    @if (item.notes) {
                                        <p class="ren-referral-queue-notes">{{ item.notes }}</p>
                                    }
                                </div>
                                <div class="ren-referral-queue-actions">
                                    @if (item.status === ReferralStatus.Pending) {
                                        <button
                                            mat-flat-button
                                            color="primary"
                                            [disabled]="!!busyId()"
                                            (click)="attend(item)">
                                            Attend
                                        </button>
                                    } @else {
                                        <a mat-flat-button color="primary" [routerLink]="actionHref(item.patientId)">
                                            Continue
                                        </a>
                                        <button
                                            mat-stroked-button
                                            color="primary"
                                            [disabled]="busyId() === item.id"
                                            (click)="complete(item)">
                                            Done
                                        </button>
                                    }
                                </div>
                            </article>
                        }
                    </div>
                }
            }
        </div>
    `
})
export class ReferralQueuePanelComponent implements OnInit {
    @Input({ required: true }) module!: AppModule;

    private readonly api = inject(RenaissanceApiService);
    private readonly auth = inject(AuthStateService);
    private readonly router = inject(Router);

    readonly ReferralStatus = ReferralStatus;
    readonly displayModuleName = displayModuleName;
    readonly priorityLabel = priorityLabel;

    readonly loading = signal(true);
    readonly expanded = signal(false);
    readonly items = signal<ReferralQueueItem[]>([]);
    readonly pendingCount = signal(0);
    readonly busyId = signal<string | null>(null);

    ngOnInit(): void {
        this.reload();
    }

    toggleExpanded(): void {
        this.expanded.update(value => !value);
    }

    actionHref(patientId: string): string {
        return queueActionHref(this.module, patientId);
    }

    attend(item: ReferralQueueItem): void {
        this.busyId.set(item.id);
        this.api.claimReferral(item.id).subscribe({
            next: () => {
                this.busyId.set(null);
                this.router.navigateByUrl(this.actionHref(item.patientId));
            },
            error: () => this.busyId.set(null)
        });
    }

    complete(item: ReferralQueueItem): void {
        this.busyId.set(item.id);
        this.api.completeReferral(item.id).subscribe({
            next: () => {
                this.busyId.set(null);
                this.reload();
            },
            error: () => this.busyId.set(null)
        });
    }

    private reload(): void {
        if (!this.auth.hasModule(this.module)) {
            this.items.set([]);
            this.pendingCount.set(0);
            this.loading.set(false);
            this.updateExpandedState();
            return;
        }

        this.loading.set(this.expanded() && this.items().length === 0);
        this.api.getReferralQueue(this.module).subscribe({
            next: queue => {
                this.items.set(queue);
                this.pendingCount.set(queue.filter(item => item.status === ReferralStatus.Pending).length);
                this.loading.set(false);
                this.updateExpandedState();
            },
            error: () => {
                this.items.set([]);
                this.pendingCount.set(0);
                this.loading.set(false);
                this.updateExpandedState();
            }
        });
    }

    private updateExpandedState(): void {
        if (this.items().length > 0) {
            this.expanded.set(true);
        }
    }
}
