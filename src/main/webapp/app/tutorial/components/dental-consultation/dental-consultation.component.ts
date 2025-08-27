import {ChangeDetectorRef, Component, inject, OnInit, signal} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ClientDashboardComponent} from "../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";
import {FuseAlertType} from "@mattae/angular-shared";
import {DentalConsultationDto, DentalConsultationService} from "./dental-consultation.service";
import {MatButton, MatIconAnchor, MatIconButton} from "@angular/material/button";
import {MatCheckbox} from "@angular/material/checkbox";
import {MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle} from "@angular/material/expansion";
import {MatIcon} from "@angular/material/icon";
import {MatRadioButton, MatRadioGroup} from "@angular/material/radio";
import {MatTooltip} from "@angular/material/tooltip";
import {MatInput} from "@angular/material/input";

export const DIAGNOSES = [
    'DENTAL CARIES', 'TOOTH SENSITIVITY', 'ORAL SORENESS', 'ORAL THRUSH',
    'BLEEDING GUM', 'DENTAL PLAQUE', 'ACUTE PERIODONTITIS', 'CHRONIC PERIODONTITIS',
    'FETOR ORIS', 'GINGIVAL RECESSION', 'GINGIVAL POCKET', 'OTHERS'
];

export const TREATMENTS = [
    'TOOTH EXTRACTION', 'SCALING AND POLISHING', 'DEEP CURATTAGE',
    'INCISION AND DRAINAGE', 'MEDICATION', 'OTHERS', 'NOT APPLICABLE'
];

@Component({
  selector: 'app-dental-consultation',
    imports: [
        MatButton,
        MatCheckbox,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatIcon,
        MatIconAnchor,
        MatRadioButton,
        MatRadioGroup,
        MatTooltip,
        MatIconButton,
        MatInput,
        ReactiveFormsModule,
        FormsModule
    ],
  templateUrl: './dental-consultation.component.html',
  styleUrl: './dental-consultation.component.scss'
})
export class DentalConsultationComponent implements OnInit{
    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private dentalConsultationService = inject(DentalConsultationService)
    form!: FormGroup;
    id: any

    diagnosisOptions = signal(DIAGNOSES)
    treatmentOptions = signal(TREATMENTS)

    selectedDiagnoses = signal<string[]>([]);
    selectedTreatments = signal<string[]>([]);
    referred = signal<boolean>(false);
    servicesReferred = signal<string[]>([]);
    dispensedItems = signal<string[]>([]);

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };

    showAlert = signal(false);

    consultationId: any;

    otherDiagnosisInput = signal('');
    otherTreatmentInput = signal('');

    showOtherTreatmentInput = signal(false);
    showOtherDiagnosisInput = signal(false);

    otherDiagnoses = signal<string[]>([]);
    otherTreatments = signal<string[]>([]);

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;
        this.consultationId = this._activatedRoute.snapshot.paramMap.get('consultationId');
        this.form = this.fb.group({
            diagnoses: [[]],
            treatments: [[]],
            servicesReferred: [[]],
            dispensedItems: [[]],
            referred: [false]
        });

        if (this.consultationId) {
            this.dentalConsultationService.getById(this.consultationId).subscribe((data) => {
                this.selectedDiagnoses.set(data.diagnoses || []);
                this.selectedTreatments.set(data.treatments || []);
                this.referred.set(data.referred || false);
                this.servicesReferred.set(data.servicesReferred || []);
                this.dispensedItems.set(data.dispensedItems || []);
                this.otherDiagnoses.set(data.othersDiagnosis || []);
                this.otherTreatments.set(data.othersTreatment || []);
                this.updateServicesReferred();
                this._changeDetectorRef.markForCheck();
            });
        }
    }

    addDiagnosis(d: string, checked: boolean) {
        if (d === 'OTHERS') {
            if (!checked){
                this.otherDiagnoses.set([])
                return;
            }
            this.showOtherDiagnosisInput.set(checked);
            return;
        }
        const list = this.selectedDiagnoses();
        this.selectedDiagnoses.set(
            checked ? [...list, d] : list.filter(item => item !== d)
        );
        this.updateServicesReferred();
    }

    addTreatment(t: string, checked: boolean) {
        if (t === 'OTHERS') {
            if (!checked){
                this.otherTreatments.set([]);
                return;
            }
            this.showOtherTreatmentInput.set(checked);
            return;
        }
        const list = this.selectedTreatments();
        this.selectedTreatments.set(
            checked ? [...list, t] : list.filter(item => item !== t)
        );
        this.updateServicesReferred();
    }

    addOtherDiagnosis() {
        const val = this.otherDiagnosisInput().trim();
        if (val && !this.otherDiagnoses().includes(val)) {
            this.otherDiagnoses.set([...this.otherDiagnoses(), val]);
            this.otherDiagnosisInput.set('');
        }
    }

    removeOtherDiagnosis(index: number) {
        const list = [...this.otherDiagnoses()];
        list.splice(index, 1);
        this.otherDiagnoses.set(list);
    }

    addOtherTreatment() {
        const val = this.otherTreatmentInput().trim();
        if (val && !this.otherTreatments().includes(val)) {
            this.otherTreatments.set([...this.otherTreatments(), val]);
            this.otherTreatmentInput.set('');
        }
    }

    removeOtherTreatment(index: number) {
        const list = [...this.otherTreatments()];
        list.splice(index, 1);
        this.otherTreatments.set(list);
    }



    addDispensedItem(item: string, checked: boolean): void {
        const current = this.dispensedItems();
        this.dispensedItems.set(
            checked ? [...current, item] : current.filter(i => i !== item)
        );
    }

    toggleReferred(value: boolean) {
        this.referred.set(value);
        this.updateServicesReferred();
    }

    private updateServicesReferred() {
        if (this.referred()) {
            const missing = this.selectedDiagnoses().filter(
                d => !this.selectedTreatments().includes(d)
            );
            this.servicesReferred.set(missing);
        } else {
            this.servicesReferred.set([]);
        }
    }


    submit(): void {
        const payload: DentalConsultationDto = {
            diagnoses: this.selectedDiagnoses(),
            treatments: this.selectedTreatments(),
            patientId: this.id,
            dispensedItems: this.dispensedItems(),
            referred: this.referred(),
            servicesReferred: this.servicesReferred(),
            othersTreatment: this.otherTreatments(),
            othersDiagnosis: this.otherDiagnoses()
        };

        if(this.consultationId){
            this.dentalConsultationService.update(this.consultationId, payload).pipe(
                map(res => {
                    this.alert.message = 'Consultation saved successfully'
                    this.closeDrawer()
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error saving this consultation',
                        type: 'error'
                    }
                    return EMPTY;
                }),
                finalize(() => {
                    this.showAlert.set(true);
                })
            ).subscribe();
        } else {
            this.dentalConsultationService.create(payload).pipe(
                map(res => {
                    this.alert.message = 'Consultation saved successfully'
                    this.closeDrawer()
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error saving this consultation',
                        type: 'error'
                    }
                    return EMPTY;
                }),
                finalize(() => {
                    this.showAlert.set(true);
                })
            ).subscribe();
        }
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadDentalConsultations()
        });
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

}
