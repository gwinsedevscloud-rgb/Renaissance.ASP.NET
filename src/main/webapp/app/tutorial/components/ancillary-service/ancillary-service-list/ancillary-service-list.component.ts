import {Component, inject, Input, OnInit, signal} from '@angular/core';
import {ConsultationDto, ConsultationNoteService} from "../../consultation-note/consultation-note.service";
import {AncillaryServiceDto, AncillaryServiceService} from "../ancillary-service.service";
import {
    MatAccordion,
    MatExpansionPanel,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";

@Component({
  selector: 'app-ancillary-service-list',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle
    ],
  templateUrl: './ancillary-service-list.component.html',
  styleUrl: './ancillary-service-list.component.scss'
})
export class AncillaryServiceListComponent implements OnInit{

    @Input() patientId!: string;

    @Input() services = signal<AncillaryServiceDto[]>([]);
    private ancillaryService = inject(AncillaryServiceService);

    ngOnInit(): void {
        /*if (this.patientId) {
            this.ancillaryService.getByPatientId(this.patientId).subscribe({
                next: data => this.services.set(data),
                error: err => console.error('Failed to load ancillary service:', err)
            });
        }*/
    }

}
