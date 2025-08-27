import {ChangeDetectorRef, Component, inject, Input, OnInit, signal} from '@angular/core';
import {FormBuilder} from "@angular/forms";
import {ClientDashboardComponent} from "../../../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {DentalConsultationDto} from "../../../dental-consultation/dental-consultation.service";
import {OptometristDto} from "../optometrist.service";
import {
    MatAccordion,
    MatExpansionPanel,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";
import {MatList, MatListItem} from "@angular/material/list";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-optometrist-list',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatList,
        MatListItem,
        NgIf
    ],
  templateUrl: './optometrist-list.component.html',
  styleUrl: './optometrist-list.component.scss'
})
export class OptometristListComponent implements OnInit{

    @Input() patientId!: string;
    @Input() optometrists = signal<OptometristDto[]>([]);

    ngOnInit(): void {

    }

}
