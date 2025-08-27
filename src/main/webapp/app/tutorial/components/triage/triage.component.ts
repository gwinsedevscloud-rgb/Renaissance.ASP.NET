import {ChangeDetectorRef, Component, inject, OnInit, signal} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {MatButton, MatIconAnchor} from "@angular/material/button";
import {ClientDashboardComponent} from "../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {Location} from "@angular/common";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {MatInput} from "@angular/material/input";
import {MatOption, MatSelect} from "@angular/material/select";
import {MatIcon} from "@angular/material/icon";
import {MatTooltip} from "@angular/material/tooltip";
import {Triage, TriageService} from "./triage.service";
import {catchError, EMPTY, finalize, map} from "rxjs";
import {FuseAlertType} from "@mattae/angular-shared";

@Component({
  selector: 'app-triage',
    imports: [
        ReactiveFormsModule,
        MatButton,
        MatFormField,
        MatLabel,
        MatInput,
        MatSelect,
        MatOption,
        MatIcon,
        MatIconAnchor,
        MatTooltip
    ],
  templateUrl: './triage.component.html',
  styleUrl: './triage.component.scss'
})
export class TriageComponent implements OnInit{

    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private triageService = inject(TriageService)
    triageForm!: FormGroup;
    protected triage = signal<Triage | null>(null)
    id: any
    triageId: any;

    showAlert = false;
    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };

    bmi: string = '';
    editMode = signal(false);

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;
        this.triageId = this._activatedRoute.snapshot.paramMap.get('triageId');
        this.triageForm = this.fb.group({
            id: [null],
            weight: ['', Validators.required],
            height: ['', Validators.required],
            // Vital Signs
            temperature: [''],
            systolicBp: ['', Validators.required],
            diastolicBp: ['', Validators.required],
            pulseRate: [''],
            respiratoryRate: [''],
            patientId: [this.id]
        });

        this.triageForm.get('weight')?.valueChanges.subscribe(() => this.updateBMI());
        this.triageForm.get('height')?.valueChanges.subscribe(() => this.updateBMI());

        if (this.triageId) {
            console.log("Triage Id **** ", this.triageId)
            this.loadTriage();
            this.editMode.set(true);
        }
    }


    loadTriage(): void {
        this.triageService.getTriageById(this.triageId).subscribe(triage => {
            this.triage.set(triage);
            this.triageForm.patchValue(triage);
            this._changeDetectorRef.markForCheck();
        });
    }

    updateBMI(): void {
        const weight = parseFloat(this.triageForm.get('weight')?.value);
        const height = parseFloat(this.triageForm.get('height')?.value);

        if (weight > 0 && height > 0) {
            const heightInMeters = height / 100;
            const bmiValue = weight / (heightInMeters * heightInMeters);
            this.bmi = bmiValue.toFixed(2);
        } else {
            this.bmi = '';
        }
    }

    onSubmit(): void {
        console.log(this.triageForm.value);
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadTriage()
        });
    }

    saveTriage (){
        this.triage.set(this.triageForm.value);
        this.showAlert = false;
        if(this.editMode()){
            this.triageService.updateTriage(this.triageId, this.triage()).pipe(
                map(res => {
                    this.alert.message = this.editMode() ? 'Triage updated successfully' : 'Triage saved successfully';
                    this.closeDrawer()
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error saving this patient',
                        type: 'error'
                    }
                    return EMPTY;
                }),
                finalize(() => {
                    // this.editMode = false;
                    this.showAlert = true;

                })
            ).subscribe();
        } else {
            this.triageService.createTriage(this.triage()).pipe(
                map(res => {
                    this.alert.message = this.editMode() ? 'Triage updated successfully' : 'Triage saved successfully';
                    this.closeDrawer()
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error saving this patient',
                        type: 'error'
                    }
                    return EMPTY;
                }),
                finalize(() => {
                    // this.editMode = false;
                    this.showAlert = true;

                })
            ).subscribe();
        }

    }
}
