import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { Patient } from '../../core/models/clinical.models';

import { clientDisplayName } from '../../core/utils/list-text.helper';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/empty-state/empty-state.component';

@Component({
    selector: 'app-clients-list',
    imports: [
        RouterLink, FormsModule, MatButtonModule, MatIconModule,
        MatFormFieldModule, MatInputModule, MatTableModule,
        PageHeaderComponent, LoadingStateComponent, EmptyStateComponent
    ],
    templateUrl: './clients-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientsListComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);

    patients = signal<Patient[]>([]);
    loading = signal(true);
    searchQuery = '';
    displayedColumns = ['client', 'phoneNumber', 'occupation', 'actions'];

    ngOnInit(): void {
        this.loadPatients();
    }

    loadPatients(q?: string): void {
        this.loading.set(true);
        this.api.getPatients(q).subscribe({
            next: data => {
                this.patients.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    onSearch(): void {
        this.loadPatients(this.searchQuery.trim() || undefined);
    }

    clearSearch(): void {
        this.searchQuery = '';
        this.loadPatients();
    }

    initials(p: Patient): string {
        const name = clientDisplayName(p.fullName, p.address);
        if (name !== '—') {
            const parts = name.split(/\s+/).filter(Boolean);
            if (parts.length >= 2) {
                return (parts[0][0] + parts[1][0]).toUpperCase();
            }
            return name.slice(0, 2).toUpperCase();
        }
        const num = p.clientNumber?.replace(/[^A-Z0-9]/gi, '') ?? '';
        return num.slice(0, 2).toUpperCase() || 'CL';
    }

    displayName(p: Patient): string {
        return clientDisplayName(p.fullName, p.address);
    }
}
