import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ExportModuleInfoDto, ExportPreviewDto, ExportRecordModule, ExportRecordsRequest } from '../../core/models/export.models';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';

@Component({
    selector: 'app-admin-export',
    imports: [
        FormsModule, RouterLink, MatButtonModule, MatCheckboxModule, MatFormFieldModule,
        MatIconModule, MatInputModule, PageHeaderComponent, LoadingStateComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-page">
            <app-page-header title="Export records" subtitle="Download filtered clinical data as Excel" icon="heroicons_outline:document-arrow-down">
                <a mat-stroked-button routerLink="/admin/users"><mat-icon svgIcon="heroicons_outline:shield-check"></mat-icon> Admin</a>
            </app-page-header>
            @if (error()) { <div class="ren-alert ren-alert-error">{{ error() }}</div> }
            @if (loading()) {
                <app-loading-state message="Loading export options..." />
            } @else {
                <div class="ren-grid-2">
                    <div class="ren-card ren-card-body ren-form-grid">
                        <h3>Filters</h3>
                        <div class="ren-form-row">
                            <mat-form-field subscriptSizing="dynamic">
                                <mat-label>From date</mat-label>
                                <input matInput type="date" [(ngModel)]="fromDate" name="fromDate" (ngModelChange)="schedulePreview()" />
                            </mat-form-field>
                            <mat-form-field subscriptSizing="dynamic">
                                <mat-label>To date</mat-label>
                                <input matInput type="date" [(ngModel)]="toDate" name="toDate" (ngModelChange)="schedulePreview()" />
                            </mat-form-field>
                        </div>
                        <mat-form-field class="w-full" subscriptSizing="dynamic">
                            <mat-label>Patient filter</mat-label>
                            <input matInput [(ngModel)]="clientSearch" name="clientSearch" (ngModelChange)="schedulePreview()"
                                placeholder="Optional — patient number or name" />
                        </mat-form-field>
                        <mat-checkbox [(ngModel)]="includeArchived" name="includeArchived" (ngModelChange)="schedulePreview()">
                            Include archived records
                        </mat-checkbox>
                        <h3>Modules</h3>
                        <div class="ren-checkbox-row">
                            @for (mod of catalog(); track mod.module) {
                                <mat-checkbox
                                    [checked]="selectedModules.has(mod.module)"
                                    (change)="toggleModule(mod.module, $event.checked)">
                                    {{ mod.name }}
                                </mat-checkbox>
                            }
                        </div>
                        <div class="ren-form-actions">
                            <button mat-stroked-button type="button" (click)="selectAll()">Select all</button>
                            <button mat-stroked-button type="button" (click)="clearModules()">Clear</button>
                            <button mat-flat-button color="primary" type="button" [disabled]="downloading() || selectedModules.size === 0" (click)="download()">
                                Download Excel
                            </button>
                        </div>
                    </div>
                    <div class="ren-card ren-card-body">
                        <h3>Preview</h3>
                        @if (previewLoading()) {
                            <app-loading-state message="Calculating preview..." />
                        } @else if (preview()) {
                            <p><strong>{{ preview()!.totalRecords }}</strong> total records</p>
                            <ul>
                                @for (m of preview()!.modules; track m.module) {
                                    <li>{{ m.name }}: {{ m.count }}</li>
                                }
                            </ul>
                        } @else {
                            <p class="ren-text-muted">Select modules to preview record counts.</p>
                        }
                    </div>
                </div>
            }
        </div>
    `
})
export class ExportComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly notifications = inject(NotificationService);
    private previewTimer?: ReturnType<typeof setTimeout>;

    fromDate = '';
    toDate = '';
    clientSearch = '';
    includeArchived = false;
    selectedModules = new Set<ExportRecordModule>();

    readonly loading = signal(true);
    readonly previewLoading = signal(false);
    readonly downloading = signal(false);
    readonly error = signal<string | null>(null);
    readonly catalog = signal<ExportModuleInfoDto[]>([]);
    readonly preview = signal<ExportPreviewDto | null>(null);

    ngOnInit(): void {
        this.api.getExportModules().subscribe({
            next: catalog => {
                this.catalog.set(catalog);
                this.selectedModules = new Set(catalog.map(c => c.module));
                this.loading.set(false);
                this.schedulePreview();
            },
            error: err => {
                this.error.set(err.message);
                this.loading.set(false);
            }
        });
    }

    toggleModule(module: ExportRecordModule, checked: boolean): void {
        if (checked) this.selectedModules.add(module);
        else this.selectedModules.delete(module);
        this.schedulePreview();
    }

    selectAll(): void {
        this.selectedModules = new Set(this.catalog().map(c => c.module));
        this.schedulePreview();
    }

    clearModules(): void {
        this.selectedModules.clear();
        this.preview.set(null);
    }

    schedulePreview(): void {
        clearTimeout(this.previewTimer);
        this.previewTimer = setTimeout(() => this.loadPreview(), 400);
    }

    download(): void {
        this.downloading.set(true);
        this.error.set(null);
        this.api.downloadExport(this.buildRequest()).subscribe({
            next: result => {
                const blob = new Blob([result.content], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
                });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = result.fileName;
                a.click();
                URL.revokeObjectURL(url);
                this.notifications.showSuccess('Export downloaded.');
                this.downloading.set(false);
            },
            error: err => {
                this.error.set(err.message);
                this.downloading.set(false);
            }
        });
    }

    private loadPreview(): void {
        if (this.selectedModules.size === 0) {
            this.preview.set(null);
            return;
        }
        this.previewLoading.set(true);
        this.api.previewExport(this.buildRequest()).subscribe({
            next: preview => {
                this.preview.set(preview);
                this.previewLoading.set(false);
            },
            error: () => this.previewLoading.set(false)
        });
    }

    private buildRequest(): ExportRecordsRequest {
        return {
            modules: Array.from(this.selectedModules),
            fromDate: this.fromDate || undefined,
            toDate: this.toDate || undefined,
            includeArchived: this.includeArchived,
            clientSearch: this.clientSearch.trim() || undefined
        };
    }
}
