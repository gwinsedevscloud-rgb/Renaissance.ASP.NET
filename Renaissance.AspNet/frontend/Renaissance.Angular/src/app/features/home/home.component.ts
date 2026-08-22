import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthStateService } from '../../core/services/auth-state.service';
import { NavigationService } from '../../core/services/navigation.service';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { ModuleNavItem, MODULE_NAV_CLINICAL } from '../../core/models/auth.models';

@Component({
    selector: 'app-home',
    imports: [RouterLink, MatButtonModule, MatIconModule],
    templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
    readonly auth = inject(AuthStateService);
    private readonly navigation = inject(NavigationService);
    private readonly api = inject(RenaissanceApiService);

    readonly stats = signal([
        { label: 'Your modules', value: '—', icon: 'heroicons_outline:squares-2x2' },
        { label: 'Queue', value: '—', icon: 'heroicons_outline:inbox' },
        { label: 'Role', value: '—', icon: 'heroicons_outline:identification' }
    ]);

    readonly features = signal(this.navigation.featureCards());
    readonly clinicalModules = signal<ModuleNavItem[]>([]);
    readonly pendingModules = signal<(ModuleNavItem & { pendingCount: number })[]>([]);
    readonly totalPending = signal(0);
    readonly facilityName = signal<string | null>(null);

    ngOnInit(): void {
        const user = this.auth.user;
        const clinical = MODULE_NAV_CLINICAL.filter(m => this.auth.hasModule(m.module));
        this.clinicalModules.set(clinical);
        this.features.set(this.navigation.featureCards());

        this.api.getPublicSettings().subscribe(settings => {
            this.facilityName.set(settings?.facilityName ?? null);
        });

        this.api.getReferralModuleCounts().subscribe(counts => {
            const countMap = new Map(counts.map(c => [c.module, c.pendingCount]));
            const pending = clinical
                .map(item => ({ ...item, pendingCount: countMap.get(item.module) ?? 0 }))
                .filter(item => item.pendingCount > 0)
                .sort((a, b) => b.pendingCount - a.pendingCount);
            const total = counts.reduce((sum, c) => sum + c.pendingCount, 0);

            this.pendingModules.set(pending);
            this.totalPending.set(total);
            this.stats.set([
                { label: 'Your modules', value: String(user?.modules.length ?? 0), icon: 'heroicons_outline:squares-2x2' },
                { label: 'Queue', value: String(total), icon: 'heroicons_outline:inbox' },
                { label: 'Role', value: user?.roleName ?? '—', icon: 'heroicons_outline:identification' }
            ]);
        });
    }
}
