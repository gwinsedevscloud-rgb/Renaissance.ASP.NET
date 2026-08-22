import { Injectable, inject } from '@angular/core';
import { FuseNavigationItem } from '@mattae/angular-shared';
import { AuthStateService } from './auth-state.service';
import { HospitalModuleStateService } from './hospital-module-state.service';
import { AppModule, MODULE_NAV, MODULE_NAV_CLINICAL } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class NavigationService {
    private readonly auth = inject(AuthStateService);
    private readonly modules = inject(HospitalModuleStateService);

    navigations(): FuseNavigationItem[] {
        const items: FuseNavigationItem[] = [
            {
                title: 'Home',
                type: 'basic',
                link: '/',
                icon: 'heroicons_outline:home'
            }
        ];

        if (this.auth.hasModule(AppModule.Clients)) {
            items.push({
                title: 'Patients',
                type: 'basic',
                link: '/patients',
                icon: 'heroicons_outline:users'
            });
        }

        const clinical = MODULE_NAV_CLINICAL.filter(item => this.auth.hasModule(item.module));
        if (clinical.length > 0) {
            items.push({
                title: 'Clinical modules',
                type: 'collapsable',
                icon: 'heroicons_outline:heart',
                children: clinical.map(item => ({
                    title: item.label,
                    type: 'basic' as const,
                    link: item.href,
                    icon: item.icon
                }))
            });
        }

        if (this.auth.hasModule(AppModule.Stakeholders)) {
            items.push({
                title: 'Stakeholders',
                type: 'basic',
                link: '/stakeholders',
                icon: 'heroicons_outline:chart-bar'
            });
        }

        if (this.auth.hasModule(AppModule.Administration)) {
            items.push({
                title: 'Administration',
                type: 'collapsable',
                icon: 'heroicons_outline:shield-check',
                children: [
                    { title: 'Users', type: 'basic', link: '/admin/users', icon: 'heroicons_outline:users' },
                    { title: 'Roles', type: 'basic', link: '/admin/roles', icon: 'heroicons_outline:key' },
                    { title: 'Settings', type: 'basic', link: '/admin/settings', icon: 'heroicons_outline:cog-6-tooth' },
                    { title: 'Backup', type: 'basic', link: '/admin/backup', icon: 'heroicons_outline:cloud-arrow-down' },
                    { title: 'Export', type: 'basic', link: '/admin/export', icon: 'heroicons_outline:document-arrow-down' },
                    { title: 'LAN access', type: 'basic', link: '/admin/lan-access', icon: 'heroicons_outline:wifi' }
                ]
            });
        }

        return items;
    }

    featureCards() {
        return MODULE_NAV.filter(item =>
            item.module !== AppModule.Administration
            && item.module !== AppModule.ClientDashboard
            && this.auth.hasModule(item.module)
        );
    }

    clinicalModules() {
        return MODULE_NAV_CLINICAL.filter(item => this.auth.hasModule(item.module));
    }
}
