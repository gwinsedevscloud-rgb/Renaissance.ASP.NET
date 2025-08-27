import {Component, inject, Input, OnInit, signal} from '@angular/core';
import {ConsultationDto, ConsultationNoteService} from "../consultation-note.service";
import {
    MatAccordion,
    MatExpansionPanel, MatExpansionPanelDescription,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";
import {ClientDashboardComponent} from "../../client-dashboard/client-dashboard.component";
import {HasAnyAuthorityDirective} from "@mattae/angular-shared";
import {MatButton, MatIconButton} from "@angular/material/button";
import {MatIcon} from "@angular/material/icon";
import {MatTooltip} from "@angular/material/tooltip";
import {Router, RouterLink} from "@angular/router";
import {TranslocoPipe} from "@jsverse/transloco";

@Component({
  selector: 'app-consultation-list',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatExpansionPanelDescription,
        HasAnyAuthorityDirective,
        MatButton,
        MatIconButton,
        MatIcon,
        MatTooltip,
        TranslocoPipe,
        RouterLink
    ],
  templateUrl: './consultation-list.component.html',
  styleUrl: './consultation-list.component.scss'
})
export class ConsultationListComponent implements OnInit{
    private _router = inject(Router);

    @Input() patientId!: string;

    @Input() consultations = signal<ConsultationDto[]>([]);
    private service = inject(ConsultationNoteService);
    private clientDashboardComponent = inject(ClientDashboardComponent)


    ngOnInit(): void {
        /*if (this.patientId) {
            this.service.getByPatientId(this.patientId).subscribe({
                next: data => this.consultations.set(data),
                error: err => console.error('Failed to load consultations:', err)
            });
        }*/
    }

    onToggleItnDispense(newValue: boolean, consultationId: any): void {
        this.service.updateItnDispense(consultationId, newValue).subscribe({
            next: (updated) => {
                console.log('Updated Consultation:', updated);
                this.clientDashboardComponent.loadConsultations()
            },
            error: (err) => {
                console.error('Failed to update itnDispense', err);
            },
        });
    }

    onEditConsultation(c: ConsultationDto) {
        this._router.navigate(['consultation', c.id])
    }
}
