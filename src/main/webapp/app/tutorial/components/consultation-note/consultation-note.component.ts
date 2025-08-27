import {ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {Editor, NgxEditorComponent} from "ngx-editor";
import {Location, NgForOf} from "@angular/common";
import {MatButton, MatIconAnchor, MatIconButton} from "@angular/material/button";
import {ClientDashboardComponent} from "../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {MatIcon} from "@angular/material/icon";
import {MatTooltip} from "@angular/material/tooltip";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {MatOption, MatSelect} from "@angular/material/select";
import {MatInput} from "@angular/material/input";
import {ConsultationDto, ConsultationNoteService} from "./consultation-note.service";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";
import {FuseAlertType} from "@mattae/angular-shared";
import {MatCheckbox} from "@angular/material/checkbox";
import {MatRadioButton, MatRadioGroup} from "@angular/material/radio";
import {MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle} from "@angular/material/expansion";
import {index} from "@angular-architects/module-federation/src/schematics/migrate-to-13/schematic";

export const DIAGNOSES = [
    'SATISFACTORY HEALTH STATUS',
    'HYPERTENSION (HIGH BP)',
    'HYPOTENSION (LOW BP)',
    'HYPERCHOLESTEROLEMIA',
    'HYPOCHOLESTEROLEMIA',
    'HYPERGLYCEMIA (DIABETES)',
    'HYPOGLYCEMIA',
    'RETROVIRAL POSITIVE',
    'MALARIA',
    'GIT DISORDERS (Diarrhea, Constipation, nausea, vomiting, etc)',
    'PEPTIC ULCER DISEASE/ HEARTBURN',
    'ALLERGIES',
    'MUSCULOSKELETAL DISORDER',
    'SKIN INFECTION (Eczema, ringworm, etc)',
    'MEASLES',
    'TRAUMA- (WOUND)',
    'LIKELY OSTEOARTHRITIS',
    'LIKELY LUMBAR SPONDYLOSIS',
    'RESPIRATORY TRACT INFECTIONS (Cough, Catarrh, Sore throat etc)',
    'OTHERS'
];

export const TREATMENTS = [
    'MALARIA',
    'INFECTION (SKIN/STI)',
    'PEPTIC ULCER DISEASE/ HEARTBURN',
    'RESPIRATORY TRACT INFECTION (Cough, Catarrh etc)',
    'GIT DISORDERS',
    'NCD (HYPERTENSION, DIABETES, SCD, ETC)',
    'ANC SERVICES',
    'OTHERS'
];

export const ANCILLARY_SERVICE = [
    'DEWORMING',
    'ITN',
    'DENTAL MATERIAL'
]


@Component({
  selector: 'app-consultation-note',
    imports: [
        ReactiveFormsModule,
        MatButton,
        MatIcon,
        MatIconAnchor,
        MatTooltip,
        MatCheckbox,
        MatRadioGroup,
        MatRadioButton,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatInput,
        FormsModule,
        MatIconButton
    ],
  templateUrl: './consultation-note.component.html',
  styleUrl: './consultation-note.component.scss'
})
export class ConsultationNoteComponent implements OnInit, OnDestroy{

    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private consultationService = inject(ConsultationNoteService);

    id: any

    form!: FormGroup;
    sections = ['Complaints', 'Examination Findings', 'Diagnosis', 'Treatment'];

    diagnosisOptions = signal(DIAGNOSES)
    treatmentOptions = signal(TREATMENTS)
    ancillaryOptions = signal(ANCILLARY_SERVICE)
    selectedDiagnoses = signal<string[]>([]);
    selectedTreatments = signal<string[]>([]);
    referred = signal<boolean>(false);
    servicesReferred = signal<string[]>([]);
    isSaving = signal(false)
    itnOrder = signal<boolean>(false)


    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    showAlert = signal(false);

    otherDiagnosisInput = signal('');
    otherTreatmentInput = signal('');

    showOtherTreatmentInput = signal(false);
    showOtherDiagnosisInput = signal(false);

    otherDiagnoses = signal<string[]>([]);
    otherTreatments = signal<string[]>([]);

    consultationId: any;

    ngOnDestroy(): void {

    }
    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;
        this.consultationId = this._activatedRoute.snapshot.paramMap.get('consultationId');
        this.form = this.fb.group({
            diagnoses: [[]],
            treatments: [[]],
            servicesReferred: [[]],
            referred: [false]
        });

        if (this.consultationId) {
            this.consultationService.getById(this.consultationId).subscribe((data) => {
                this.selectedDiagnoses.set(data.diagnoses || []);
                this.selectedTreatments.set(data.treatments || []);
                this.referred.set(data.referred || false);
                this.servicesReferred.set(data.servicesReferred || []);
                this.itnOrder.set(data.itnOrder || false);
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


    toggleReferred(value: boolean) {
        this.referred.set(value);
        this.updateServicesReferred();
    }

    toggleItnOrder(value: boolean) {
        this.itnOrder.set(value);
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
        const payload: ConsultationDto = {
            id: this.consultationId,
            referred: this.referred(),
            servicesReferred: this.servicesReferred(),
            diagnoses: this.selectedDiagnoses(),
            treatments: this.selectedTreatments(),
            patientId: this.id,
            itnOrder: this.itnOrder(),
            itnDispense: false,
            othersTreatment: this.otherTreatments(),
            othersDiagnosis: this.otherDiagnoses()
        };
        if(this.consultationId){
            this.consultationService.update(this.consultationId, payload).pipe(
                map(res => {
                    this.alert.message = 'Test order successfully'
                    this.closeDrawer()
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error ordering these test',
                        type: 'error'
                    }
                    return EMPTY;
                }),
                finalize(() => {
                    this.showAlert.set(true);
                })
            ).subscribe();
        } else{
            this.consultationService.save(payload).pipe(
                map(res => {
                    this.alert.message = 'Test order successfully'
                    this.closeDrawer()
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error ordering these test',
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
            this.clientDashboardComponent.loadConsultations()
        });
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    protected readonly index = index;
}
