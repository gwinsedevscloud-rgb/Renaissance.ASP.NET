import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { AppModule } from '../../core/models/auth.models';
import { HospitalModuleStateService } from '../../core/services/hospital-module-state.service';
import { HospitalModulesConfigDto, ModuleReferralLinkDto } from '../../core/models/settings.models';

@Component({
    selector: 'app-admin-settings',
    imports: [
        ReactiveFormsModule,
        RouterLink,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatSlideToggleModule,
        PageHeaderComponent,
        LoadingStateComponent
    ],
    templateUrl: './settings.component.html'
})
export class SettingsComponent implements OnInit {
    private readonly fb = inject(FormBuilder);
    private readonly api = inject(RenaissanceApiService);
    private readonly modules = inject(HospitalModuleStateService);

    readonly loading = signal(true);
    readonly saving = signal(false);
    readonly savingModules = signal(false);
    readonly message = signal<string | null>(null);
    readonly error = signal<string | null>(null);
    readonly moduleConfig = signal<HospitalModulesConfigDto | null>(null);
    readonly enabledModules = signal<Set<AppModule>>(new Set());

    readonly form = this.fb.nonNullable.group({
        facilityName: [''],
        clientNumberPrefix: [''],
        timeZoneId: ['UTC']
    });

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.api.getHospitalSettings().subscribe({
            next: settings => {
                this.form.patchValue(settings);
                this.api.getHospitalModulesConfig().subscribe({
                    next: config => {
                        this.moduleConfig.set(config);
                        this.enabledModules.set(new Set(
                            config.services.filter(s => !s.isCore && s.isEnabled).map(s => s.module)
                        ));
                        this.loading.set(false);
                    },
                    error: err => {
                        this.error.set(err.message);
                        this.loading.set(false);
                    }
                });
            },
            error: err => {
                this.error.set(err.message);
                this.loading.set(false);
            }
        });
    }

    saveFacility(): void {
        this.saving.set(true);
        this.message.set(null);
        this.error.set(null);
        this.api.updateHospitalSettings(this.form.getRawValue()).subscribe({
            next: () => {
                this.message.set('Facility settings saved.');
                this.saving.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.saving.set(false);
            }
        });
    }

    toggleModule(module: AppModule, enabled: boolean): void {
        const next = new Set(this.enabledModules());
        if (enabled) {
            next.add(module);
        } else {
            next.delete(module);
        }
        this.enabledModules.set(next);
    }

    saveModules(): void {
        const enabled = this.enabledModules();
        if (enabled.size === 0) {
            this.error.set('At least one clinical service must remain enabled.');
            return;
        }

        const config = this.moduleConfig();
        const links = (config?.referralLinks ?? []).filter(
            l => enabled.has(l.sourceModule) && enabled.has(l.targetModule)
        );

        this.savingModules.set(true);
        this.message.set(null);
        this.error.set(null);

        this.api.updateHospitalModulesConfig({
            enabledModules: Array.from(enabled).sort((a, b) => a - b),
            referralLinks: links
        }).subscribe({
            next: updated => {
                this.moduleConfig.set(updated);
                this.api.getActiveHospitalModules().subscribe(state => this.modules.apply(state));
                this.message.set('Module configuration saved.');
                this.savingModules.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.savingModules.set(false);
            }
        });
    }

    optionalServices() {
        return this.moduleConfig()?.services.filter(s => !s.isCore) ?? [];
    }
}
