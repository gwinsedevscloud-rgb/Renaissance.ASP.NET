import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { RoleDto, displayModuleName } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';

@Component({
    selector: 'app-admin-roles',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Roles" subtitle="Module access templates for staff" icon="heroicons_outline:key">
                <a mat-stroked-button routerLink="/admin/users"><mat-icon svgIcon="heroicons_outline:users"></mat-icon> Users</a>
                <a mat-flat-button color="primary" routerLink="/admin/roles/create"><mat-icon svgIcon="heroicons_outline:plus"></mat-icon> New role</a>
            </app-page-header>
            @if (loading()) {
                <app-loading-state message="Loading roles..." />
            } @else if (roles().length === 0) {
                <app-empty-state icon="heroicons_outline:key" title="No roles yet" message="Create a role template.">
                    <a mat-flat-button color="primary" routerLink="/admin/roles/create">New role</a>
                </app-empty-state>
            } @else {
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="roles()" class="ren-table w-full">
                        <ng-container matColumnDef="name">
                            <th mat-header-cell *matHeaderCellDef>Role</th>
                            <td mat-cell *matCellDef="let r"><strong>{{ r.name }}</strong></td>
                        </ng-container>
                        <ng-container matColumnDef="description">
                            <th mat-header-cell *matHeaderCellDef>Description</th>
                            <td mat-cell *matCellDef="let r">{{ r.description || '—' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="modules">
                            <th mat-header-cell *matHeaderCellDef>Modules</th>
                            <td mat-cell *matCellDef="let r">{{ r.modules.length }}</td>
                        </ng-container>
                        <ng-container matColumnDef="users">
                            <th mat-header-cell *matHeaderCellDef>Users</th>
                            <td mat-cell *matCellDef="let r">{{ r.userCount }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let r">
                                <a mat-stroked-button color="primary" [routerLink]="['/admin/roles/edit', r.id]">Edit</a>
                            </td>
                        </ng-container>
                        <tr mat-header-row *matHeaderRowDef="columns"></tr>
                        <tr mat-row *matRowDef="let row; columns: columns"></tr>
                    </table>
                </div>
            }
        </div>
    `
})
export class RolesComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    readonly displayModuleName = displayModuleName;

    readonly loading = signal(true);
    readonly roles = signal<RoleDto[]>([]);
    readonly columns = ['name', 'description', 'modules', 'users', 'actions'];

    ngOnInit(): void {
        this.api.getRoles().subscribe({
            next: roles => {
                this.roles.set(roles);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }
}
