import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { BackupInfoDto } from '../../core/models/backup.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';

@Component({
    selector: 'app-admin-backup',
    imports: [
        RouterLink, MatButtonModule, MatIconModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Database backup" subtitle="Export, download, and restore clinical data" icon="heroicons_outline:cloud-arrow-down">
                <a mat-stroked-button routerLink="/admin/users"><mat-icon svgIcon="heroicons_outline:users"></mat-icon> Users</a>
                <button mat-flat-button color="primary" type="button" [disabled]="busy()" (click)="createBackup()">
                    <mat-icon svgIcon="heroicons_outline:plus"></mat-icon> Create backup
                </button>
            </app-page-header>
            @if (message()) { <div class="ren-alert ren-alert-success">{{ message() }}</div> }
            @if (error()) { <div class="ren-alert ren-alert-error">{{ error() }}</div> }
            <div class="ren-grid-2">
                <div>
                    @if (loading()) {
                        <app-loading-state message="Loading backups..." />
                    } @else if (backups().length === 0) {
                        <app-empty-state icon="heroicons_outline:cloud-arrow-down" title="No backups yet"
                            message="Create your first backup to download a portable copy of all data.">
                            <button mat-flat-button color="primary" type="button" [disabled]="busy()" (click)="createBackup()">Create backup</button>
                        </app-empty-state>
                    } @else {
                        <div class="ren-results-bar"><span class="ren-results-count">{{ backups().length }} saved backups</span></div>
                        <div class="ren-card ren-table-card">
                            <table mat-table [dataSource]="backups()" class="ren-table w-full">
                                <ng-container matColumnDef="file">
                                    <th mat-header-cell *matHeaderCellDef>File</th>
                                    <td mat-cell *matCellDef="let b"><code>{{ b.fileName }}</code></td>
                                </ng-container>
                                <ng-container matColumnDef="size">
                                    <th mat-header-cell *matHeaderCellDef>Size</th>
                                    <td mat-cell *matCellDef="let b">{{ b.sizeLabel }}</td>
                                </ng-container>
                                <ng-container matColumnDef="created">
                                    <th mat-header-cell *matHeaderCellDef>Created (UTC)</th>
                                    <td mat-cell *matCellDef="let b">{{ formatDate(b.createdAtUtc) }}</td>
                                </ng-container>
                                <ng-container matColumnDef="actions">
                                    <th mat-header-cell *matHeaderCellDef></th>
                                    <td mat-cell *matCellDef="let b">
                                        <button mat-stroked-button color="primary" type="button" [disabled]="busy()" (click)="download(b.fileName)">Download</button>
                                        <button mat-stroked-button color="warn" type="button" [disabled]="busy()" (click)="remove(b.fileName)">Delete</button>
                                    </td>
                                </ng-container>
                                <tr mat-header-row *matHeaderRowDef="columns"></tr>
                                <tr mat-row *matRowDef="let row; columns: columns"></tr>
                            </table>
                        </div>
                    }
                </div>
                <div class="ren-card ren-card-body">
                    <h2>Restore from file</h2>
                    <p class="ren-text-muted">Upload a Renaissance backup (.zip). This replaces all current data.</p>
                    <div class="ren-alert ren-alert-warning">Restore cannot be undone. Create a fresh backup first.</div>
                    <input type="file" accept=".zip" (change)="onFileSelected($event)" [disabled]="busy()" />
                    @if (restoreFile()) {
                        <p class="mt-2">Selected: {{ restoreFile()!.name }}</p>
                        <button mat-flat-button color="primary" type="button" [disabled]="busy() || restoring()" (click)="restore()">
                            Restore backup
                        </button>
                    }
                </div>
            </div>
        </div>
    `
})
export class BackupComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly notifications = inject(NotificationService);

    readonly loading = signal(true);
    readonly busy = signal(false);
    readonly restoring = signal(false);
    readonly backups = signal<BackupInfoDto[]>([]);
    readonly message = signal<string | null>(null);
    readonly error = signal<string | null>(null);
    readonly restoreFile = signal<File | null>(null);
    readonly columns = ['file', 'size', 'created', 'actions'];

    ngOnInit(): void {
        this.reload();
    }

    createBackup(): void {
        this.busy.set(true);
        this.message.set(null);
        this.error.set(null);
        this.api.createBackup().subscribe({
            next: result => {
                this.message.set(result.message);
                this.notifications.showSuccess('Backup created.');
                this.reload();
                this.busy.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.busy.set(false);
            }
        });
    }

    download(fileName: string): void {
        this.busy.set(true);
        this.api.downloadBackup(fileName).subscribe({
            next: buffer => {
                const blob = new Blob([buffer], { type: 'application/zip' });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName;
                a.click();
                URL.revokeObjectURL(url);
                this.busy.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.busy.set(false);
            }
        });
    }

    remove(fileName: string): void {
        if (!confirm(`Delete backup ${fileName}?`)) return;
        this.busy.set(true);
        this.api.deleteBackup(fileName).subscribe({
            next: () => {
                this.notifications.showSuccess('Backup deleted.');
                this.reload();
                this.busy.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.busy.set(false);
            }
        });
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        this.restoreFile.set(input.files?.[0] ?? null);
    }

    restore(): void {
        const file = this.restoreFile();
        if (!file || !confirm('Restore will replace ALL current data. Continue?')) return;
        this.restoring.set(true);
        this.busy.set(true);
        this.error.set(null);
        this.api.restoreBackup(file).subscribe({
            next: result => {
                this.message.set(result.message);
                this.notifications.showSuccess(`Restored ${result.patientsRestored} patients.`);
                this.restoreFile.set(null);
                this.reload();
                this.restoring.set(false);
                this.busy.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.restoring.set(false);
                this.busy.set(false);
            }
        });
    }

    formatDate(value: string): string {
        return new Date(value).toISOString().slice(0, 16).replace('T', ' ');
    }

    private reload(): void {
        this.loading.set(true);
        this.api.getBackups().subscribe({
            next: backups => {
                this.backups.set(backups);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }
}
