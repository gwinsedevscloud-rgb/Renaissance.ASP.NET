import {Component, computed, inject, Input, OnChanges, signal} from '@angular/core';
import {PharmacyPrescription, PharmacyService} from "../pharmacy.service";
import {
    MatAccordion,
    MatExpansionPanel, MatExpansionPanelDescription,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";
import {formatDate, NgClass} from "@angular/common";
import {MatIcon} from "@angular/material/icon";
import {MatButton} from "@angular/material/button";
import {ClientDashboardComponent} from "../../client-dashboard/client-dashboard.component";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";

@Component({
  selector: 'app-view-prescription',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatExpansionPanelDescription,
        NgClass,
        MatIcon,
        MatButton
    ],
  templateUrl: './view-prescription.component.html',
  styleUrl: './view-prescription.component.scss'
})
export class ViewPrescriptionComponent implements  OnChanges{

    @Input() prescriptions: PharmacyPrescription[] = [];
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private pharmacyService = inject(PharmacyService)

    // Signal for reactive tracking (optional)
    prescriptionsSignal = signal<PharmacyPrescription[]>([]);
    // Computed signal to group by date
    groupedPrescriptions = computed(() => {
        const grouped: { [date: string]: PharmacyPrescription[] } = {};
        for (const p of this.prescriptionsSignal()) {
            // @ts-ignore
            const date = new Date(p.createdDate).toDateString();
            if (!grouped[date]) {
                grouped[date] = [];
            }
            grouped[date].push(p);
        }
        return grouped;
    });

    objectKeys = Object.keys;

    ngOnChanges() {
        this.prescriptionsSignal.set(this.prescriptions);
    }

    deletePrescription(id: string | undefined) {
        this.pharmacyService.archive(id).pipe(
            map(() => {
                this.clientDashboardComponent.loadPrescriptions()
            }),
            catchError(error => {
                console.log(error)
                return EMPTY;
            }),
            finalize(() => {

            })
        ).subscribe();
    }


}
