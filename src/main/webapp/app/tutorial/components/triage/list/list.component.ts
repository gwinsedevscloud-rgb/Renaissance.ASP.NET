import {Component, inject, Input, OnChanges, OnInit, signal, SimpleChanges} from '@angular/core';
import {Triage, TriageService} from "../triage.service";
import {
    MatAccordion,
    MatExpansionPanel,
    MatExpansionPanelDescription,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";
import {DatePipe} from "@angular/common";

@Component({
  selector: 'app-triage-list',
    imports: [
        MatExpansionPanelDescription,
        MatExpansionPanelTitle,
        MatExpansionPanelHeader,
        MatExpansionPanel,
        MatAccordion,
        DatePipe
    ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss'
})
export class ListComponent implements OnChanges{

    private triageService = inject(TriageService)

    @Input() triages = signal<Triage[] | []>([])
    @Input() patientId!: string;

    ngOnChanges(changes: SimpleChanges): void {
        console.log("Patient Id ***** ", this.patientId)
        if (changes['patientId'] && this.patientId) {
            this.loadTriageHistory();
        }
    }

    loadTriageHistory(): void {
        this.triageService.getTriagesByPatientId(this.patientId).subscribe({
            next: (data) => (this.triages.set(data)),
            error: (err) => console.error('Failed to load triages:', err)
        });
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    computeBMI(weight: any, height: any): any {
        if (weight > 0 && height > 0) {
            const heightInMeters = height / 100;
            const bmiValue = weight / (heightInMeters * heightInMeters);
            return bmiValue.toFixed(2);
        } else {
            return '';
        }
    }

}
