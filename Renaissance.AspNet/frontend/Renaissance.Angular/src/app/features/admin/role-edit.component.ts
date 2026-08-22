import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AppModule, ModuleDescriptorDto, SaveRoleRequest } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { FormActionsComponent } from '../../shared/form-actions/form-actions.component';

@Component({
    selector: 'app-role-edit',
    imports: [
        FormsModule, MatCheckboxModule, MatFormFieldModule, MatInputModule,
        PageHeaderComponent, LoadingStateComponent, FormActionsComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header [title]="isNew ? 'New role' : 'Edit role'" subtitle="Define module access template"
                icon="heroicons_outline:key" backLink="/admin/roles" backLabel="Roles" />
            @if (error()) {
                <div class="ren-alert ren-alert-error">{{ error() }}</div>
            }
            @if (loading()) {
                <app-loading-state message="Loading role..." />
            } @else {
                <form class="ren-card ren-card-body ren-form-grid" (ngSubmit)="save()">
                    <mat-form-field class="w-full" subscriptSizing="dynamic">
                        <mat-label>Role name</mat-label>
                        <input matInput [(ngModel)]="name" name="name" required [disabled]="isSystem" />
                    </mat-form-field>
                    <mat-form-field class="w-full" subscriptSizing="dynamic">
                        <mat-label>Description</mat-label>
                        <textarea matInput rows="2" [(ngModel)]="description" name="description"></textarea>
                    </mat-form-field>
                    <section>
                        <h3>Modules</h3>
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
                    <app-form-actions [saveLabel]="isNew ? 'Create role' : 'Save role'" cancelHref="/admin/roles" [saving]="saving()" />
                </form>
            }
        </div>
    `
})
export class RoleEditComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly notifications = inject(NotificationService);

    roleId?: string;
    isNew = true;
    isSystem = false;
    name = '';
    description = '';
    selectedModules = new Set<AppModule>();

    readonly loading = signal(true);
    readonly saving = signal(false);
    readonly error = signal<string | null>(null);
    readonly moduleCatalog = signal<ModuleDescriptorDto[]>([]);

    ngOnInit(): void {
        this.roleId = this.route.snapshot.paramMap.get('id') ?? undefined;
        this.isNew = !this.roleId || this.route.snapshot.url.some(s => s.path === 'create');

        this.api.getModuleCatalog().subscribe(catalog => this.moduleCatalog.set(catalog));

        if (this.isNew) {
            this.loading.set(false);
        } else if (this.roleId) {
            this.api.getRole(this.roleId).subscribe(role => {
                if (role) {
                    this.name = role.name;
                    this.description = role.description ?? '';
                    this.isSystem = role.isSystem;
                    this.selectedModules = new Set(role.modules);
                }
                this.loading.set(false);
            });
        }
    }

    toggleModule(module: AppModule, checked: boolean): void {
        if (checked) this.selectedModules.add(module);
        else this.selectedModules.delete(module);
    }

    save(): void {
        this.saving.set(true);
        this.error.set(null);
        const request: SaveRoleRequest = {
            name: this.name,
            description: this.description || undefined,
            modules: Array.from(this.selectedModules)
        };

        const obs = this.isNew
            ? this.api.createRole(request)
            : this.api.updateRole(this.roleId!, request);

        obs.subscribe({
            next: () => {
                this.notifications.showSuccess(this.isNew ? 'Role created.' : 'Role updated.');
                this.router.navigate(['/admin/roles']);
            },
            error: err => {
                this.error.set(err.message);
                this.saving.set(false);
            }
        });
    }
}
