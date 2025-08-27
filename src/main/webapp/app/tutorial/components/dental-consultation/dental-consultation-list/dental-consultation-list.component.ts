import {Component, inject, Input, OnInit, signal} from '@angular/core';
import {
    MatAccordion,
    MatExpansionPanel,
    MatExpansionPanelHeader, MatExpansionPanelTitle
} from "@angular/material/expansion";
import {DentalConsultationDto, DentalConsultationService} from "../dental-consultation.service";

@Component({
  selector: 'app-dental-consultation-list',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle
    ],
  templateUrl: './dental-consultation-list.component.html',
  styleUrl: './dental-consultation-list.component.scss'
})
export class DentalConsultationListComponent implements OnInit{


    @Input() patientId!: string;
    @Input() consultations = signal<DentalConsultationDto[]>([]);

    private service = inject(DentalConsultationService);

    ngOnInit(): void {
        console.log("From DS **** ", this.consultations().length)
        /*if (this.patientId) {
            this.service.getByPatientId(this.patientId).subscribe({
                next: data => this.consultations.set(data),
                error: err => console.error('Failed to load consultations:', err)
            });
        }*/
    }

}
