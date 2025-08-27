import {ChangeDetectorRef, Component, inject, OnInit, signal} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ClientDashboardComponent} from "../../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {OphthalmologistDto, OphthalmologistService} from "./ophthalmologist.service";
import {MatIcon} from "@angular/material/icon";
import {MatButton, MatIconAnchor, MatIconButton} from "@angular/material/button";
import {MatTooltip} from "@angular/material/tooltip";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {MatOption, MatSelect} from "@angular/material/select";
import {NgForOf, NgIf} from "@angular/common";
import {MatInput} from "@angular/material/input";
import {MatChip} from "@angular/material/chips";
import {MatCheckbox} from "@angular/material/checkbox";
import {MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle} from "@angular/material/expansion";
import {MatRadioButton, MatRadioGroup} from "@angular/material/radio";
import {DentalConsultationDto} from "../../dental-consultation/dental-consultation.service";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";
import {FuseAlertType} from "@mattae/angular-shared";

@Component({
  selector: 'app-ophthalmologis',
    imports: [
        MatIcon,
        MatIconAnchor,
        MatTooltip,
        ReactiveFormsModule,
        FormsModule,
        MatInput,
        MatButton,
        MatCheckbox,
        MatIconButton,
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatRadioButton,
        MatRadioGroup,
        MatFormField,
        MatLabel,
        MatOption,
        MatSelect,
        NgForOf
    ],
  templateUrl: './ophthalmologist.component.html',
  styleUrl: './ophthalmologist.component.scss'
})
export class OphthalmologistComponent implements OnInit{

    diagnosisOptions = ['GLAUCOMA', 'DRY EYE', 'ALLERGIC CONJUNCTIVITIS', 'BACTERIAL CONJUNCTIVITIS', 'IMMATURE CATARACT', 'MATURE CATARACT', 'PTERYGIUM', 'CORNEA OPACITY', 'FOREIGN BODY', 'RETINOPATHY', 'MACULOPATHY', 'MYOPIA', 'PRESBYOPIA', 'ASTIGMATISM', 'HYPEROPIA', 'OTHERS'];

    treatmentOptions = ['MEDICATION', 'SURGERY', 'REFRACTION', 'OTHERS', 'NA'];

    surgeryOptions = ['CATARACT', 'PTERYGIUM', 'OTHERS'];

    visualAcuityOptions: string[] = ['6/6', '6/9', '6/12', '6/18', '6/24', '6/36', '6/60', 'CF', 'HM', 'PL', 'NPL'];

    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private ophthalmologistService = inject(OphthalmologistService)

    id: any
    ophthalmologistId: any

    form!: FormGroup;
    editMode = false;

    model = {
        diagnoses: [] as string[],
        treatments: [] as string[],
        surgeries: [] as string[],
        referred: false
    };

    visualAcuityRight = signal('')
    visualAcuityLeft = signal('')
    glassesDispensed = signal(false)


    selectedDiagnoses = signal<string[]>([]);
    selectedTreatments = signal<string[]>([]);
    selectedSurgery = signal<string[]>([]);

    referred = signal<boolean>(false);

    otherDiagnosisInput = signal('');
    otherTreatmentInput = signal('');
    otherSurgeryInput = signal('');

    showOtherTreatmentInput = signal(false);
    showOtherDiagnosisInput = signal(false);
    showOtherSurgeryInput = signal(false);

    otherDiagnoses = signal<string[]>([]);
    otherTreatments = signal<string[]>([]);
    otherSurgery = signal<string[]>([]);

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };

    showAlert = signal(false);

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;
        this.ophthalmologistId = this._activatedRoute.snapshot.paramMap.get('ophId');

        if (this.ophthalmologistId){
            this.ophthalmologistService.findById(this.ophthalmologistId).subscribe((data) => {
                this.selectedDiagnoses.set(data.diagnoses || []);
                this.selectedTreatments.set(data.treatments || []);
                this.referred.set(data.referred || false);
                this.otherDiagnoses.set(data.othersDiagnosis || []);
                this.otherTreatments.set(data.othersTreatment || []);
                this.selectedSurgery.set(data.surgeries || []);
                this.visualAcuityLeft.set(data.visualAcuityLeft || '');
                this.visualAcuityRight.set(data.visualAcuityRight || '');
                this.glassesDispensed.set(data.glassesDispensed || false);
                this._changeDetectorRef.markForCheck();
            });
        }

    }

    addDiagnosis(t: string, checked: boolean) {
        if (t === 'OTHERS') {
            if (!checked){
                this.otherDiagnoses.set([]);
                return;
            }
            this.showOtherDiagnosisInput.set(checked);
            return;
        }
        const list = this.selectedDiagnoses();
        this.selectedDiagnoses.set(
            checked ? [...list, t] : list.filter(item => item !== t)
        );
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
    }

    addOtherDiagnosis() {
        const val = this.otherDiagnosisInput().trim();
        if (val && !this.otherDiagnoses().includes(val)) {
            this.otherDiagnoses.set([...this.otherDiagnoses(), val]);
            this.otherDiagnosisInput.set('');
        }
    }

    addSurgery(t: string, checked: boolean) {
        if (t === 'OTHERS') {
            if (!checked){
                this.otherSurgery.set([]);
                return;
            }
            this.showOtherSurgeryInput.set(checked);
            return;
        }
        const list = this.selectedSurgery();
        this.selectedSurgery.set(
            checked ? [...list, t] : list.filter(item => item !== t)
        );
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

    addOtherSurgery() {
        const val = this.otherSurgeryInput().trim();
        if (val && !this.otherSurgery().includes(val)) {
            this.otherSurgery.set([...this.otherSurgery(), val]);
            this.otherSurgeryInput.set('');
        }
    }

    removeOtherTreatment(index: number) {
        const list = [...this.otherTreatments()];
        list.splice(index, 1);
        this.otherTreatments.set(list);
    }

    removeOtherSurgery(index: number) {
        const list = [...this.otherSurgery()];
        list.splice(index, 1);
        this.otherSurgery.set(list);
    }


    toggleReferred(value: boolean) {
        this.referred.set(value);
    }

    toggleGlassesDispensed(value: boolean){
        this.glassesDispensed.set(value)
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadOphthalmologist()
        });
    }

    submit(): void {
        const payload: OphthalmologistDto = {
            diagnoses: this.selectedDiagnoses(),
            treatments: this.selectedTreatments(),
            patientId: this.id,
            referred: this.referred(),
            othersTreatment: this.otherTreatments(),
            othersDiagnosis: this.otherDiagnoses(),
            surgeries: this.selectedSurgery(),
            otherSurgery: this.otherSurgery(),
            visualAcuityRight: this.visualAcuityRight(),
            visualAcuityLeft: this.visualAcuityLeft(),
            glassesDispensed: this.glassesDispensed()
        };

        if(this.ophthalmologistId) {
            this.ophthalmologistService.update(this.ophthalmologistId, payload).pipe(
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
            this.ophthalmologistService.save(payload).pipe(
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

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

}
