import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AppModule, ModuleNavItem, MODULE_NAV_REFERRAL_TARGETS } from '../models/auth.models';
import { HospitalModulesStateDto } from '../models/settings.models';

@Injectable({ providedIn: 'root' })
export class HospitalModuleStateService {
    private enabled = new Set<AppModule>();
    private referralLinks = new Set<string>();
    private readonly _loaded$ = new BehaviorSubject<boolean>(false);

    readonly loaded$ = this._loaded$.asObservable();

    isEnabled(module: AppModule): boolean {
        if (module === AppModule.Clients || module === AppModule.ClientDashboard || module === AppModule.Administration) {
            return true;
        }

        return this.enabled.has(module);
    }

    apply(state: HospitalModulesStateDto): void {
        this.enabled = new Set(state.enabledModules);
        this.referralLinks = new Set(
            state.referralLinks.map(link => `${link.sourceModule}-${link.targetModule}`)
        );
        this._loaded$.next(true);
    }

    canRefer(source: AppModule, target: AppModule): boolean {
        return source !== target && this.referralLinks.has(`${source}-${target}`);
    }

    getReferralTargets(source: AppModule): ModuleNavItem[] {
        return MODULE_NAV_REFERRAL_TARGETS.filter(item => this.canRefer(source, item.module));
    }

    filterNavItems(items: ModuleNavItem[]): ModuleNavItem[] {
        return items.filter(item => this.isEnabled(item.module));
    }
}
