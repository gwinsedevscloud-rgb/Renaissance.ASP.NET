import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { AuthStateService } from '../../core/services/auth-state.service';
import { ClientDashboardDto } from '../../core/models/clinical.models';
import { AppModule } from '../../core/models/auth.models';
import { PatientJourneyStep } from '../../core/models/referral.models';
import { PageHeaderComponent } from '../../shared/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/loading-state/loading-state.component';
import { displayModuleName } from '../../core/models/auth.models';

interface ActionChip {
    title: string;
    icon: string;
    count: number;
    done: boolean;
    pending?: boolean;
    module: AppModule;
    createHref: string;
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
    private readonly auth = inject(AuthStateService);
    private readonly route = inject(ActivatedRoute);

    dashboard = signal<ClientDashboardDto | null>(null);
    journey = signal<PatientJourneyStep[]>([]);
    loading = signal(true);
    patientId = '';

    ngOnInit(): void {
        this.patientId = this.route.snapshot.paramMap.get('id')!;
        this.api.getDashboard(this.patientId).subscribe({
            next: d => {
                this.dashboard.set(d);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
        this.api.getPatientJourney(this.patientId).subscribe({
            next: steps => this.journey.set(steps),
            error: () => this.journey.set([])
        });
    }

    chips(d: ClientDashboardDto): ActionChip[] {
        const id = d.patient?.id ?? this.patientId;
        return [
            { title: 'Triage', icon: 'heroicons_outline:heart', count: d.triages.length, done: d.triages.length > 0, module: AppModule.Triage, createHref: `/triage/create/${id}` },
            { title: 'Consultation', icon: 'heroicons_outline:clipboard-document-list', count: d.consultations.length, done: d.consultations.length > 0, module: AppModule.Consultations, createHref: `/consultations/create/${id}` },
            { title: 'Pharmacy', icon: 'heroicons_outline:beaker', count: d.pharmacyPrescriptions.length, done: d.pharmacyPrescriptions.length > 0, pending: d.hasPendingPharmacy, module: AppModule.Pharmacy, createHref: `/pharmacy/create/${id}` },
            { title: 'Laboratory', icon: 'heroicons_outline:beaker', count: d.laboratories.length, done: d.laboratories.length > 0, module: AppModule.Laboratory, createHref: `/laboratory/create/${id}` },
            { title: 'Dental', icon: 'heroicons_outline:face-smile', count: d.dentalConsultations.length, done: d.dentalConsultations.length > 0, module: AppModule.Dental, createHref: `/dental/create/${id}` },
            { title: 'Ancillary', icon: 'heroicons_outline:plus-circle', count: d.ancillaries.length, done: d.ancillaries.length > 0, module: AppModule.Ancillary, createHref: `/ancillary/create/${id}` },
            { title: 'Optometrist', icon: 'heroicons_outline:eye', count: d.optometrists.length, done: d.optometrists.length > 0, module: AppModule.Optometrists, createHref: `/optometrists/create/${id}` },
            { title: 'Ophthalmologist', icon: 'heroicons_outline:eye', count: d.ophthalmologists.length, done: d.ophthalmologists.length > 0, module: AppModule.Ophthalmologists, createHref: `/ophthalmologists/create/${id}` }
        ].filter(c => this.auth.hasModule(c.module));
    }

    sections(d: ClientDashboardDto): DashboardSection[] {
        return [
            { title: 'Triage', icon: 'heroicons_outline:heart', items: d.triages.map(t => `BP ${t.systolicBp}/${t.diastolicBp} · Pulse ${t.pulseRate} · Temp ${t.temperature}°C`) },
            { title: 'Consultations', icon: 'heroicons_outline:clipboard-document-list', items: d.consultations.map(c => (c.diagnoses || []).join(', ') || 'Consultation recorded') },
            { title: 'Pharmacy', icon: 'heroicons_outline:beaker', items: d.pharmacyPrescriptions.map(rx => `${rx.drugName} — ${rx.status}`) },
            { title: 'Laboratory', icon: 'heroicons_outline:beaker', items: d.laboratories.map(lab => `${lab.testName}: ${lab.result || 'Pending'}`) },
            { title: 'Dental', icon: 'heroicons_outline:face-smile', items: d.dentalConsultations.map(dc => (dc.diagnoses || []).join(', ') || 'Dental consultation') },
            { title: 'Ancillary', icon: 'heroicons_outline:plus-circle', items: d.ancillaries.map(a => (a.services || []).join(', ') || 'Ancillary service') },
            { title: 'Optometrists', icon: 'heroicons_outline:eye', items: d.optometrists.map(o => `VA R/L ${o.visualAcuityRight}/${o.visualAcuityLeft}`) },
            { title: 'Ophthalmologists', icon: 'heroicons_outline:eye', items: d.ophthalmologists.map(o => (o.diagnoses || []).join(', ') || 'Ophthalmology record') }
        ];
    }

    completedCount(d: ClientDashboardDto): number {
        return this.chips(d).filter(c => c.done).length;
    }

    totalRecords(d: ClientDashboardDto): number {
        return this.chips(d).reduce((sum, c) => sum + c.count, 0);
    }

    journeyLabel(step: PatientJourneyStep): string {
        return displayModuleName(step.targetModule);
    }
}
