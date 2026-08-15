import { DecimalPipe, NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { StakeholdersDashboardDto } from '../../core/models/stakeholders.models';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';

@Component({
    selector: 'app-stakeholders-dashboard',
    imports: [DecimalPipe, NgClass, MatButtonModule, MatIconModule, PageHeaderComponent, LoadingStateComponent],
    templateUrl: './stakeholders-dashboard.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StakeholdersDashboardComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);

    dashboard = signal<StakeholdersDashboardDto | null>(null);
    loading = signal(true);

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.api.getStakeholdersDashboard().subscribe({
            next: d => { this.dashboard.set(d); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    formatGrowth(value: number): string {
        return value >= 0 ? `+${value}%` : `${value}%`;
    }

    trendClass(delta: number): string {
        return delta > 0 ? 'ren-trend-up' : delta < 0 ? 'ren-trend-down' : 'ren-trend-flat';
    }

    formatDelta(delta: number): string {
        return delta === 0 ? '—' : delta > 0 ? `+${delta}` : `${delta}`;
    }
}
