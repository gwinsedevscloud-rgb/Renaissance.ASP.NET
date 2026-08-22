import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { UserDto } from '../../core/models/auth.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { displayModuleName } from '../../core/models/auth.models';
import { patientInitials as getInitials } from '../../core/utils/list-text.helper';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';

@Component({
    selector: 'app-admin-users',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Users & staff access" subtitle="Create accounts and assign module access" icon="heroicons_outline:users">
                <a mat-stroked-button routerLink="/admin/roles"><mat-icon svgIcon="heroicons_outline:key"></mat-icon> Role templates</a>
                <a mat-flat-button color="primary" routerLink="/admin/users/create"><mat-icon svgIcon="heroicons_outline:plus"></mat-icon> New user</a>
            </app-page-header>
            @if (loading()) {
                <app-loading-state message="Loading users..." />
            } @else if (users().length === 0) {
                <app-empty-state icon="heroicons_outline:users" title="No users yet" message="Create the first staff account.">
                    <a mat-flat-button color="primary" routerLink="/admin/users/create">New user</a>
                </app-empty-state>
            } @else {
                <div class="ren-results-bar"><span class="ren-results-count">{{ users().length }} users</span></div>
                <div class="ren-card ren-table-card">
                    <table mat-table [dataSource]="users()" class="ren-table w-full">
                        <ng-container matColumnDef="name">
                            <th mat-header-cell *matHeaderCellDef>Name</th>
                            <td mat-cell *matCellDef="let u">
                                <div class="ren-table-client">
                                    <span class="ren-client-avatar">{{ initials(u.fullName) }}</span>
                                    <strong>{{ u.fullName }}</strong>
                                </div>
                            </td>
                        </ng-container>
                        <ng-container matColumnDef="userName">
                            <th mat-header-cell *matHeaderCellDef>Username</th>
                            <td mat-cell *matCellDef="let u"><code>{{ u.userName }}</code></td>
                        </ng-container>
                        <ng-container matColumnDef="role">
                            <th mat-header-cell *matHeaderCellDef>Role</th>
                            <td mat-cell *matCellDef="let u"><span class="ren-chip">{{ u.roleName }}</span></td>
                        </ng-container>
                        <ng-container matColumnDef="modules">
                            <th mat-header-cell *matHeaderCellDef class="ren-col-hide-lg">Modules</th>
                            <td mat-cell *matCellDef="let u" class="ren-col-hide-lg">
                                @for (m of u.modules.slice(0, 3); track m) {
                                    <span class="ren-chip">{{ displayModuleName(m) }}</span>
                                }
                                @if (u.modules.length > 3) { <span class="ren-chip">+{{ u.modules.length - 3 }}</span> }
                            </td>
                        </ng-container>
                        <ng-container matColumnDef="status">
                            <th mat-header-cell *matHeaderCellDef>Status</th>
                            <td mat-cell *matCellDef="let u">{{ u.isActive ? 'Active' : 'Disabled' }}</td>
                        </ng-container>
                        <ng-container matColumnDef="actions">
                            <th mat-header-cell *matHeaderCellDef></th>
                            <td mat-cell *matCellDef="let u">
                                <a mat-stroked-button color="primary" [routerLink]="['/admin/users/edit', u.id]">Edit</a>
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
export class UsersComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    readonly displayModuleName = displayModuleName;

    initials(name: string): string {
        return getInitials(name);
    }

    readonly loading = signal(true);
    readonly users = signal<UserDto[]>([]);
    readonly columns = ['name', 'userName', 'role', 'modules', 'status', 'actions'];

    ngOnInit(): void {
        this.api.getUsers().subscribe({
            next: users => {
                this.users.set(users);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }
}
