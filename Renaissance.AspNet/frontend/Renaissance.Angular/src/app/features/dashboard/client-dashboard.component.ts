import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { ClientDashboardDto } from '../../core/models/clinical.models';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';

interface ActionChip {
    title: string;
    icon: string;
    count: number;
    done: boolean;
    pending?: boolean;
}

interface DashboardSection {
    title: string;
    icon: string;
    items: string[];
}

@Component({
    selector: 'app-client-dashboard',
    imports: [RouterLink, MatButtonModule, MatIconModule, PageHeaderComponent, LoadingStateComponent],
    templateUrl: './client-dashboard.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientDashboardComponent implements OnInit {
    private readonly api = inject(RenaissanceApiService);
    private readonly route = inject(ActivatedRoute);

    dashboard = signal<ClientDashboardDto | null>(null);
    loading = signal(true);

    ngOnInit(): void {
        const patientId = this.route.snapshot.paramMap.get('id')!;
        this.api.getDashboard(patientId).subscribe({
            next: d => { this.dashboard.set(d); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    chips(d: ClientDashboardDto): ActionChip[] {
        return [
            { title: 'Triage', icon: 'heroicons_outline:heart', count: d.triages.length, done: d.triages.length > 0 },
            { title: 'Consultation', icon: 'heroicons_outline:clipboard-document-list', count: d.consultations.length, done: d.consultations.length > 0 },
            {
                title: 'Pharmacy',
                icon: 'heroicons_outline:beaker',
                count: d.pharmacyPrescriptions.length,
                done: d.pharmacyPrescriptions.length > 0,
                pending: d.hasPendingPharmacy
            },
            { title: 'Laboratory', icon: 'heroicons_outline:beaker', count: d.laboratories.length, done: d.laboratories.length > 0 },
            { title: 'Dental', icon: 'heroicons_outline:face-smile', count: d.dentalConsultations.length, done: d.dentalConsultations.length > 0 },
            { title: 'Ancillary', icon: 'heroicons_outline:plus-circle', count: d.ancillaries.length, done: d.ancillaries.length > 0 },
            { title: 'Optometrist', icon: 'heroicons_outline:eye', count: d.optometrists.length, done: d.optometrists.length > 0 },
            { title: 'Ophthalmologist', icon: 'heroicons_outline:eye', count: d.ophthalmologists.length, done: d.ophthalmologists.length > 0 }
        ];
    }

    sections(d: ClientDashboardDto): DashboardSection[] {
        return [
            {
                title: 'Triage',
                icon: 'heroicons_outline:heart',
                items: d.triages.map(t => `BP ${t.systolicBp}/${t.diastolicBp} · Pulse ${t.pulseRate} · Temp ${t.temperature}°C`)
            },
            {
                title: 'Consultations',
                icon: 'heroicons_outline:clipboard-document-list',
                items: d.consultations.map(c => (c.diagnoses || []).join(', ') || 'Consultation recorded')
            },
            {
                title: 'Pharmacy',
                icon: 'heroicons_outline:beaker',
                items: d.pharmacyPrescriptions.map(rx => `${rx.drugName} — ${rx.status}`)
            },
            {
                title: 'Laboratory',
                icon: 'heroicons_outline:beaker',
                items: d.laboratories.map(lab => `${lab.testName}: ${lab.result || 'Pending'}`)
            },
            {
                title: 'Dental',
                icon: 'heroicons_outline:face-smile',
                items: d.dentalConsultations.map(dc => (dc.diagnoses || []).join(', ') || 'Dental consultation')
            },
            {
                title: 'Ancillary',
                icon: 'heroicons_outline:plus-circle',
                items: d.ancillaries.map(a => (a.services || []).join(', ') || 'Ancillary service')
            }
        ];
    }

    completedCount(d: ClientDashboardDto): number {
        return this.chips(d).filter(c => c.done).length;
    }

    totalRecords(d: ClientDashboardDto): number {
        return this.chips(d).reduce((sum, c) => sum + c.count, 0);
    }
}
