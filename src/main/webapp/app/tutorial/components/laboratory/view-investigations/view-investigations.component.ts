import {Component, inject, Input, OnChanges, signal, SimpleChanges} from '@angular/core';
import {LaboratoryDto} from "../laboratory.service";
import {
    MatAccordion,
    MatExpansionPanel, MatExpansionPanelDescription,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle
} from "@angular/material/expansion";
import {MatButton} from "@angular/material/button";
import {ClientDashboardComponent} from "../../client-dashboard/client-dashboard.component";
import {Router} from "@angular/router";
import {MatDialog} from "@angular/material/dialog";
import {ResultComponent} from "../result/result.component";
import {HasAnyAuthorityDirective} from "@mattae/angular-shared";
import {MatIcon} from "@angular/material/icon";
import {MatTooltip} from "@angular/material/tooltip";

@Component({
    selector: 'app-view-investigations',
    imports: [
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatExpansionPanelDescription,
        MatButton,
        HasAnyAuthorityDirective,
        MatIcon,
        MatTooltip
    ],
    templateUrl: './view-investigations.component.html',
    styleUrl: './view-investigations.component.scss'
})
export class ViewInvestigationsComponent implements OnChanges {

    @Input() results = signal<LaboratoryDto[]>([]);
    patientId = signal('');

    private _dialog = inject(MatDialog);
    grouped: { [type: string]: LaboratoryDto[] } = {};
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _router = inject(Router);

    ngOnChanges(): void {
        console.log("From lAB view **** ", this.results())
    }

    groupByTestType(): void {
        const groupedMap: { [type: string]: LaboratoryDto[] } = {};
        for (const test of this.results()) {
            const key = test.testType || 'General';
            groupedMap[key] = groupedMap[key] || [];
            groupedMap[key].push(test);
        }
        this.grouped = groupedMap;
    }

    objectKeys(obj: any): string[] {
        return Object.keys(obj);
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    openLaboratoryResultForm (test: LaboratoryDto){
        const dia = this._dialog.open(ResultComponent, { width: '1200px', data: test});
        dia.afterClosed().subscribe((updated: boolean) => {
            if (updated) {
                this.clientDashboardComponent.loadTests();
            }
        });
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.patientId()]);
        });
    }
}
