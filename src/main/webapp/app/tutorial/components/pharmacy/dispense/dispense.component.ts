import {Component, inject, OnInit, signal} from '@angular/core';
import {MatIcon} from "@angular/material/icon";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {MatInput} from "@angular/material/input";
import {ActivatedRoute, Router} from "@angular/router";
import {FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {PharmacyPrescription, PharmacyService} from "../pharmacy.service";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";
import {ClientDashboardComponent} from "../../client-dashboard/client-dashboard.component";
import {MatOption, MatSelect} from "@angular/material/select";
import {MatButton, MatIconAnchor} from "@angular/material/button";
import {MatTooltip} from "@angular/material/tooltip";
import {FuseAlertType} from "@mattae/angular-shared";
import {MatRadioButton, MatRadioGroup} from "@angular/material/radio";

@Component({
  selector: 'app-despense',
    imports: [
        MatFormField,
        MatLabel,
        MatInput,
        ReactiveFormsModule,
        MatSelect,
        MatOption,
        MatButton,
        FormsModule,
        MatIcon,
        MatIconAnchor,
        MatTooltip,
        MatRadioButton,
        MatRadioGroup
    ],
  templateUrl: './dispense.component.html',
  styleUrl: './dispense.component.scss'
})
export class DispenseComponent implements OnInit{
    private route = inject(ActivatedRoute);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private fb = inject(FormBuilder);
    private pharmacyService = inject(PharmacyService);
    private _router = inject(Router);

    prescriptionArray = signal<PharmacyPrescription[]>([]);

    patientId!: string;
    isLoading = signal(false);
    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    showAlert = signal(false);

    form!: FormGroup;

    itnDispense = signal<boolean>(false)

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.patientId = this.route.parent?.snapshot.paramMap.get('id')!;
        this.loadPrescriptions();

    }

    toggleItnDispense(value: boolean) {
        this.itnDispense.set(value);
    }

    loadPrescriptions(): void {
        this.pharmacyService.getByPatientId(this.patientId).pipe(
            map(data =>
                data.map(p => ({
                    ...p,
                    dispensationNote: p.dispensationNote || '',
                    status: p.status || 'PENDING',
                    quantityDispensed: p.quantityDispensed || 0
                }))
            ),
            map(mapped => this.prescriptionArray.set(mapped)),
            catchError(() => {
                console.log("Fail to load prescriptions")
                return EMPTY;
            }),
            finalize(() => this.isLoading.set(false))
        ).subscribe();
    }

    submitDispensations(): void {
        this.pharmacyService.updatePrescriptions(this.prescriptionArray()).pipe(
            map(res => {
                this.alert.message = 'Prescription updated successfully'
                this.closeDrawer()
            }),
            catchError(error => {
                this.alert = {
                    message: 'There was an error updating this prescriptions',
                    type: 'error'
                }
                return EMPTY;
            }),
            finalize(() => {
                this.showAlert.set(true);
            })
        ).subscribe();
    }

    trackByFn(index: number, item: PharmacyPrescription) {
        return item.drugName + index;
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.patientId]);
        });
    }
}
