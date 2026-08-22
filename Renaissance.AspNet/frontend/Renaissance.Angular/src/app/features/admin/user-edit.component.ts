import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { AppModule, CreateUserRequest, ModuleDescriptorDto, RoleDto, UpdateUserRequest } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';

@Component({
    selector: 'app-user-edit',
    imports: [
        FormsModule, MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatInputModule,
        MatSelectModule, MatSlideToggleModule, PageHeaderComponent, LoadingStateComponent, FormActionsComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header [title]="isNew ? 'New user' : 'Edit user'" subtitle="Staff account and module access"
                icon="heroicons_outline:user-circle" backLink="/admin/users" backLabel="Users" />
            @if (error()) {
                <div class="ren-alert ren-alert-error">{{ error() }}</div>
            }
            @if (loading()) {
                <app-loading-state message="Loading user..." />
            } @else {
                <form class="ren-card ren-card-body ren-form-grid" (ngSubmit)="save()">
                    <div class="ren-form-row">
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Full name</mat-label>
                            <input matInput [(ngModel)]="fullName" name="fullName" required />
                        </mat-form-field>
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Username</mat-label>
                            <input matInput [(ngModel)]="userName" name="userName" [disabled]="!isNew" required />
                        </mat-form-field>
                    </div>
                    <div class="ren-form-row">
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>{{ isNew ? 'Password' : 'New password (optional)' }}</mat-label>
                            <input matInput type="password" [(ngModel)]="password" name="password" [required]="isNew" />
                        </mat-form-field>
                        <mat-form-field subscriptSizing="dynamic">
                            <mat-label>Role</mat-label>
                            <mat-select [(ngModel)]="roleId" name="roleId" (ngModelChange)="onRoleChange()" required>
                                @for (role of roles(); track role.id) {
                                    <mat-option [value]="role.id">{{ role.name }}</mat-option>
                                }
                            </mat-select>
                        </mat-form-field>
                    </div>
                    <mat-slide-toggle [(ngModel)]="isActive" name="isActive">Active account</mat-slide-toggle>
                    @if (!isAdminRole) {
                        <section>
                            <h3>Module access</h3>
                            <div class="ren-checkbox-row">
                                @for (mod of moduleCatalog(); track mod.module) {
                                    <mat-checkbox
                                        [checked]="selectedModules.has(mod.module)"
                                        (change)="toggleModule(mod.module, $event.checked)">
                                        {{ mod.name }}
                                    </mat-checkbox>
                                }
                            </div>
                        </section>
                    } @else {
                        <p class="ren-text-muted">Administrators have access to all modules.</p>
                    }
                    <app-form-actions [saveLabel]="isNew ? 'Create user' : 'Save user'" cancelHref="/admin/users" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class UserEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    userId?: string;
    isNew = true;
    fullName = '';
    userName = '';
    password = '';
    roleId = '';
    isActive = true;
    isAdminRole = false;
    selectedModules = new Set<AppModule>();

    readonly loading = signal(true);
    readonly saving = signal(false);
    readonly error = signal<string | null>(null);
    readonly roles = signal<RoleDto[]>([]);
    readonly moduleCatalog = signal<ModuleDescriptorDto[]>([]);

    ngOnInit(): void {
        this.userId = this.route.snapshot.paramMap.get('id') ?? undefined;
        this.isNew = !this.userId || this.route.snapshot.url.some(s => s.path === 'create');

        this.api.getRoles().subscribe(roles => this.roles.set(roles));
        this.api.getModuleCatalog().subscribe(catalog => this.moduleCatalog.set(catalog));

        if (this.isNew) {
            this.loading.set(false);
        } else if (this.userId) {
            this.api.getUser(this.userId).subscribe(user => {
                if (user) {
                    this.fullName = user.fullName;
                    this.userName = user.userName;
                    this.roleId = user.roleId;
                    this.isActive = user.isActive;
                    this.selectedModules = new Set(user.modules);
                    this.syncAdminRole();
                }
                this.loading.set(false);
            });
        }
    }

    onRoleChange(): void {
        const role = this.roles().find(r => r.id === this.roleId);
        if (role) {
            this.selectedModules = new Set(role.modules);
            this.syncAdminRole();
        }
    }

    toggleModule(module: AppModule, checked: boolean): void {
        if (checked) this.selectedModules.add(module);
        else this.selectedModules.delete(module);
    }

    save(): void {
        this.saving.set(true);
        this.error.set(null);
        const modules = Array.from(this.selectedModules);

        if (this.isNew) {
            const request: CreateUserRequest = {
                userName: this.userName,
                fullName: this.fullName,
                password: this.password,
                roleId: this.roleId,
                isActive: this.isActive,
                modules
            };
            this.api.createUser(request).subscribe({
                next: () => {
                    this.notifications.showSuccess('User created.');
                    this.router.navigate(['/admin/users']);
                },
                error: err => {
                    this.error.set(err.message);
                    this.saving.set(false);
                }
            });
        } else if (this.userId) {
            const request: UpdateUserRequest = {
                fullName: this.fullName,
                password: this.password || undefined,
                roleId: this.roleId,
                isActive: this.isActive,
                modules
            };
            this.api.updateUser(this.userId, request).subscribe({
                next: () => {
                    this.notifications.showSuccess('User updated.');
                    this.router.navigate(['/admin/users']);
                },
                error: err => {
                    this.error.set(err.message);
                    this.saving.set(false);
                }
            });
        }
    }

    private syncAdminRole(): void {
        const role = this.roles().find(r => r.id === this.roleId);
        this.isAdminRole = role?.name.toLowerCase().includes('admin') ?? false;
    }
}
