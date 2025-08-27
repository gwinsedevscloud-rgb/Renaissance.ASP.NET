import {ChangeDetectorRef, Component, inject, OnInit, signal} from '@angular/core';
import {MatButton, MatIconAnchor} from "@angular/material/button";
import {MatCheckbox} from "@angular/material/checkbox";
import {MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle} from "@angular/material/expansion";
import {MatIcon} from "@angular/material/icon";
import {FormBuilder} from "@angular/forms";
import {ClientDashboardComponent} from "../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {AncillaryServiceDto, AncillaryServiceService} from "./ancillary-service.service";
import {FuseAlertType} from "@mattae/angular-shared";
import {DentalConsultationDto} from "../dental-consultation/dental-consultation.service";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";
import {MatTooltip} from "@angular/material/tooltip";

@Component({
  selector: 'app-ancillary-service',
    imports: [
        MatButton,
        MatCheckbox,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatIcon,
        MatIconAnchor,
        MatTooltip
    ],
  templateUrl: './ancillary-service.component.html',
  styleUrl: './ancillary-service.component.scss'
})
export class AncillaryServiceComponent implements OnInit{

    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private ancillaryService = inject(AncillaryServiceService)

    id: any

    services = signal<string[]>([]);

    pregnancyStatus = signal("")

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };

    showAlert = signal(false);

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;
    }

    addDispensedItem(item: string, checked: boolean): void {
        const current = this.services();
        this.services.set(
            checked ? [...current, item] : current.filter(i => i !== item)
        );
    }

    submit(): void {
        const payload: AncillaryServiceDto = {
            patientId: this.id,
            services: this.services(),
            pregnancyStatus: this.pregnancyStatus()
        };
        this.ancillaryService.create(payload).pipe(
            map(res => {
                this.alert.message = 'Services saved successfully'
                this.closeDrawer()
            }),
            catchError(error => {
                this.alert = {
                    message: 'There was an error saving services',
                    type: 'error'
                }
                return EMPTY;
            }),
            finalize(() => {
                this.showAlert.set(true);
            })
        ).subscribe();
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadAncillaryService()
        });
    }

}
