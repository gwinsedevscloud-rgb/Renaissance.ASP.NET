import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { AuthStateService } from '../../core/services/auth-state.service';
import { HospitalModuleStateService } from '../../core/services/hospital-module-state.service';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';

@Component({
    selector: 'app-login',
    imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
    private readonly fb = inject(FormBuilder);
    private readonly api = inject(RenaissanceApiService);
    private readonly auth = inject(AuthStateService);
    private readonly modules = inject(HospitalModuleStateService);
    private readonly router = inject(Router);

    readonly loading = signal(false);
    readonly error = signal<string | null>(null);
    readonly facilityName = signal('Renaissance');

    readonly form = this.fb.nonNullable.group({
        userName: ['', Validators.required],
        password: ['', Validators.required]
    });

    ngOnInit(): void {
        this.api.getPublicSettings().subscribe({
            next: settings => this.facilityName.set(settings?.facilityName ?? 'Renaissance'),
            error: () => this.facilityName.set('Renaissance')
        });
    }

    submit(): void {
        if (this.form.invalid) {
            return;
        }

        this.loading.set(true);
        this.error.set(null);

        this.api.login(this.form.getRawValue()).subscribe({
            next: result => {
                this.auth.signIn(result.token, result.user);
                this.api.getActiveHospitalModules().subscribe({
                    next: state => this.modules.apply(state),
                    error: () => this.modules.apply({ enabledModules: [], referralLinks: [] })
                });
                this.router.navigate(['/']);
            },
            error: err => {
                this.error.set(err.message || 'Sign in failed.');
                this.loading.set(false);
            },
            complete: () => this.loading.set(false)
        });
    }
}
