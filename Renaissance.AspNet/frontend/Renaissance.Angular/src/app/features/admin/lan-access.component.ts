import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { LanAccessStateService } from '../../core/services/lan-access-state.service';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { DeploymentInfoDto } from '../../core/models/settings.models';

@Component({
    selector: 'app-lan-access',
    imports: [
        ReactiveFormsModule,
        RouterLink,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        PageHeaderComponent,
        LoadingStateComponent
    ],
    templateUrl: './lan-access.component.html'
})
export class LanAccessComponent implements OnInit {
    private readonly fb = inject(FormBuilder);
    private readonly api = inject(RenaissanceApiService);
    readonly lanAccess = inject(LanAccessStateService);

    readonly loading = signal(false);
    readonly unlocking = signal(false);
    readonly saving = signal(false);
    readonly changingPassword = signal(false);
    readonly message = signal<string | null>(null);
    readonly error = signal<string | null>(null);
    readonly deployment = signal<DeploymentInfoDto | null>(null);

    readonly unlockForm = this.fb.nonNullable.group({ password: [''] });
    readonly settingsForm = this.fb.nonNullable.group({
        webAccessUrl: [''],
        apiAccessUrl: ['']
    });
    readonly passwordForm = this.fb.nonNullable.group({
        currentPassword: [''],
        newPassword: ['']
    });

    ngOnInit(): void {
        if (this.lanAccess.isUnlocked) {
            this.load();
        }
    }

    unlock(): void {
        this.unlocking.set(true);
        this.error.set(null);
        this.api.unlockLanAccess({ password: this.unlockForm.controls.password.value }).subscribe({
            next: result => {
                this.lanAccess.applyUnlock(result.token, result.expiresAtUtc);
                this.unlockForm.reset();
                this.message.set('LAN settings unlocked.');
                this.load();
                this.unlocking.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.unlocking.set(false);
            }
        });
    }

    lock(): void {
        this.lanAccess.clear();
        this.deployment.set(null);
        this.message.set(null);
        this.error.set(null);
    }

    load(): void {
        this.loading.set(true);
        this.api.getLanSettings().subscribe({
            next: settings => {
                this.settingsForm.patchValue(settings);
                this.api.getDeploymentInfo().subscribe({
                    next: info => {
                        this.deployment.set(info);
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

    saveSettings(): void {
        this.saving.set(true);
        this.error.set(null);
        this.api.updateLanSettings(this.settingsForm.getRawValue()).subscribe({
            next: () => {
                this.message.set('Network settings saved.');
                this.api.getDeploymentInfo().subscribe(info => this.deployment.set(info));
                this.saving.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.saving.set(false);
            }
        });
    }

    changePassword(): void {
        this.changingPassword.set(true);
        this.error.set(null);
        this.api.changeLanAccessPassword(this.passwordForm.getRawValue()).subscribe({
            next: () => {
                this.passwordForm.reset();
                this.message.set('LAN access password updated.');
                this.changingPassword.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.changingPassword.set(false);
            }
        });
    }

    copyUrl(configured: string, suggested: string): void {
        const url = configured?.trim() || suggested;
        navigator.clipboard.writeText(url);
        this.message.set('URL copied to clipboard.');
    }
}
