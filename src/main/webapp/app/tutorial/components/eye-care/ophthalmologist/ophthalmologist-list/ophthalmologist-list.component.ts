import {Component, Input, OnInit, signal} from '@angular/core';
import {DentalConsultationDto} from "../../../dental-consultation/dental-consultation.service";
import {OphthalmologistDto} from "../ophthalmologist.service";
import {
    MatAccordion,
    MatExpansionPanel,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";
import {MatList, MatListItem, MatListSubheaderCssMatStyler} from "@angular/material/list";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-ophthalmologist-list',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatList,
        MatListItem,
        NgIf,
        MatListSubheaderCssMatStyler
    ],
  templateUrl: './ophthalmologist-list.component.html',
  styleUrl: './ophthalmologist-list.component.scss'
})
export class OphthalmologistListComponent implements OnInit{

    @Input() patientId!: string;
    @Input() ophthalmologists = signal<OphthalmologistDto[]>([]);

    ngOnInit(): void {

    }

}
