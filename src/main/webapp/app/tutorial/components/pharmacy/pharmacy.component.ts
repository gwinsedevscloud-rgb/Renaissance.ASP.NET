import {ChangeDetectorRef, Component, inject, Input, OnInit, signal} from '@angular/core';
import {FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {MatError, MatFormField, MatLabel} from "@angular/material/form-field";
import {MatIcon} from "@angular/material/icon";
import {MatOption, MatSelect} from "@angular/material/select";
import {ClientDashboardComponent} from "../client-dashboard/client-dashboard.component";
import {MatButton, MatIconAnchor} from "@angular/material/button";
import {MatTooltip} from "@angular/material/tooltip";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {MatInput} from "@angular/material/input";
import {PharmacyService} from "./pharmacy.service";
import {map} from "rxjs/operators";
import {FuseAlertType} from "@mattae/angular-shared";
import {catchError, EMPTY, finalize} from "rxjs";
import {MatRadioButton, MatRadioGroup} from "@angular/material/radio";
import {
    MatAccordion,
    MatExpansionPanel,
    MatExpansionPanelDescription,
    MatExpansionPanelHeader, MatExpansionPanelTitle
} from "@angular/material/expansion";

@Component({
  selector: 'app-pharmacy',
    imports: [
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatIcon,
        MatSelect,
        MatOption,
        MatError,
        MatIconAnchor,
        MatTooltip,
        RouterLink,
        MatInput,
        MatButton,
        MatRadioButton,
        MatRadioGroup,
        MatAccordion,
        MatExpansionPanel,
        MatExpansionPanelDescription,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle
    ],
  templateUrl: './pharmacy.component.html',
  styleUrl: './pharmacy.component.scss'
})
export class PharmacyComponent implements OnInit{

      drugList: { [category: string]: string[] } = {
        'Vitamins and Minerals': [
            'SYR VITAMIN C (FIDSON)',
            'EVANS MULTIVITAMIN SYRUP',
            'RANFERON SYRUP',
            'TAB VITAMIN C (WHITE)',
            'TAB FERROUS SULPHATE',
            'TAB FOLIC ACID',
            'TAB VITAMIN C (COLOURED)',
            'TAB VITAMIN B COMPLEX',
            'TAB MULTIVITAMIN',
            'ORS'
        ],
        'Anti-Inflammatory / Antiallergy / Antiemetic / Bronchodilators & Cough Syrup': [
            'SYR CHLORPHENIRAMINE',
            'TAB CHLORPHENIRAMINE',
            'TAB PREDNISOLONE',
            'TAB METOCLOPRAMIDE',
            'TAB LOPERAMIDE (DIAFLUSH)',
            'TAB SALBUTAMOL',
            'SYR SALBUTAMOL',
            'SALBUTAMOL INHALER',
            'SALBUTAMOL NEBULES 2.5MCG /5MCG',
            'INJECTION WATER FOR THE NEBULES',
            'TAB IVERMECTIN',
            'EMZOLYN EXPECTORANT',
            'COFLIN EXPECTORANT',
            'COFLIN LINTUS',
            'GREENLIN BABY SYRUP',
            'COFMIX SYRUP FOR CHILDREN',
            'EMZOLYN FOR CHILDREN'
        ],
        'Anti-Ulcer / Anti-Helminthics / Purgatives': [
            'SUSP MIST MAGNESIUM',
            'TAB MAGNESIUM TRISILICATE',
            'CAP OMEPRAZOLE',
            'SUSP ALBENDAZOLE',
            'TAB ALBENDAZOLE 400mg',
            'TAB ALBENDAZOLE 200MG',
            'GESTID SUSPENSION',
            'GASCOL SUSPENSION'
        ],
        'Antispasmodic': [
            'HYOSCINE N- BUTYLBROMIDE 10MG'
        ],
        'Anti-Malaria': [
            'TAB ARTEMETHER+LUMENFANTRINE',
            'LONART SYRUP (ARTEMETER +LUMEFANTRINE)',
            'TAB ARTEMETHER + LUMENFANTRINE DS',
            'TAB ARTEMETHER + LUMENFANTRINE',
            'TAB SULPHADOXINE + PYRIMETHAMINE'
        ],
        'Antihypertensive': [
            'TAB LISINOPRIL',
            'TAB LISINOPRIL',
            'TAB HYDROCHLOROTHIAZIDE',
            'TAB LABETALOL',
            'TAB METHYLDOPA',
            'TAB AMLODIPINE',
            'TAB AMLODIPINE',
            'TAB NIFEDIPINE',
            'TAB VASOPRIN',
            'TAB CLOPIDOGREL',
            'TAB ROSUVASTATIN',
            'TAB METFORMIN',
            'TABS GLIBENCLAMIDE',
            'NORMORETIC',
            'LOSARTAN TABLET',
            'ARTOVASTATIN',
            'RENIX TABLET (Furosemide)'
        ],
        'Anti-Diabetic': [
            'TAB METFORMIN',
            'TABS GLIBENCLAMIDE'
        ],
        'Analgesics / Muscle Relaxants / Anxiolytics / Anaesthetic': [
            'SYR IBUPROFEN',
            'SYR PARACETAMOL',
            'PARACETAMOL DROP',
            'TAB CHYMORAL',
            'TAB IBUPROFEN',
            'TAB DICLOFENAC',
            'TAB PARACETAMOL',
            'TAB ORHENADRINE + PARACETAMOL (NORGESIC)',
            'TAB NEUROVITE FORTE',
            'TAB TRANEXAMIC ACID',
            'ACECLOFENAC TABLET'
        ],
        'Anxiolytic': [
            'TAB DIAZEPAM',
            'TAB BROMAZEPAM'
        ],
        'Eye / Nasal Preparation': [
            'OTOMED EAR drops',
            'MYCOTEN solution (ear drops)',
            'Chloramphenicol eyedrop',
            'Antallerg eyedrop',
            'Timolol eyedrop',
            'Hypromellose eyedrop',
            'Bet-n eyedrop',
            'Ivyflur eyedrop'
        ],
        'Antifungals': [
            'TAB FLUCONAZOLE',
            'SUSP FLUCONAZOLE',
            'TAB GRISEOFULVIN',
            'SUSP GRISEOFULVIN',
            'SUSP NYSTATIN',
            'CLOTRIMAZOLE VAGINA PESSARY 200MG',
            'CLOTRIMAZOLE CREAM 20GM',
            'CLOTRIMAZOLE+NEOMYCIN+BETAMETHASONE (TRIPLE ACTION CREAM) 30GM',
            'CALAMINE LOTION',
            'BENZYL BENZOATE LOTION',
            'HYDROCORTISONE CREAM 15GM'
        ],
        'Antibiotics': [
            'SUSP AMOXICILLIN',
            'SUSP AMPICILLIN + CLOXACILLIN',
            'SUSP CO-TRIMOXAZOLE',
            'SUSP METRONIDAZOLE',
            'SUSP AZITHROMYCIN',
            'AMPICLOX DROP',
            'CAP AMOXICILLIN',
            'TAB CEFUROXIME',
            'TAB NITROFURANTOIN',
            'TAB LEVOFLOXACIN',
            'SUSP ERYTHROMYCIN',
            'TAB ERYTHROMYCIN',
            'TAB CLARITHROMYCIN',
            'TAB CIPROFLOXACIN',
            'TAB AZITHROMYCIN',
            'TAB AMOXICILLIN + CLAVULANIC ACID',
            'CAP AMPICILLIN + CLOXACILLIN',
            'CAP DOXYCYCLINE',
            'TAB METRONIDAZOLE',
            'TAB OFLOXACIN + ORNDAZOLE',
            'TINIDAZOLE TABLET'
        ]
    };

    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private pharmacyService = inject(PharmacyService)
    private _router = inject(Router);
    prescriptionForm!: FormGroup;

    selectedCategory = '';
    drugCategories: string[] = [];
    id: any;
    prescriptions: any = []
    showAlert = false;
    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };

    @Input() patientId: any;
    @Input() listMode: any;

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;

        this.prescriptionForm = this.fb.group({
            drugCategory: ['', Validators.required],
            drugName: ['', Validators.required],
            dosage: ['', Validators.required],
            frequency: ['', Validators.required],
            duration: ['', Validators.required],
            patientId: [this.id]
        });
        this.drugCategories = Object.keys(this.drugList);

        this._changeDetectorRef.detectChanges()
    }

    onCategoryChange(category: string) {
        this.selectedCategory = category;
        this.prescriptionForm.patchValue({ drugName: '' });
    }

    addPrescription(): void {
        const { drugName, dosage, frequency, duration } = this.prescriptionForm.value;

        if (!drugName || !dosage || !frequency || !duration) return;

        this.prescriptions.push(this.prescriptionForm.value);

        // Reset input
        this.prescriptionForm.patchValue({
            drugName: '',
            dosage: '',
            frequency: '',
            duration: ''
        });
    }

    onSubmit() {
        console.log('Prescriptions:', this.prescriptionForm.value.prescriptions);
    }

    get drugsInSelectedCategory(): string[] {
        return this.selectedCategory ? this.drugList[this.selectedCategory] : [];
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadPrescriptions()
            this.clientDashboardComponent.hasPendingPrescriptions()
        });
    }
    removePrescription(index: number) {
        this.prescriptions.splice(index, 1);
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    savePrescriptions() {
        this.showAlert = false;

        this.pharmacyService.savePrescriptions(this.prescriptions).pipe(
            map(() => {
                this.alert.message = 'Prescriptions saved successfully';
                this.closeDrawer();
            }),
            catchError(error => {
                this.alert = {
                    message: 'There was an error saving the prescriptions',
                    type: 'error'
                };
                return EMPTY;
            }),
            finalize(() => {
                this.showAlert = true;
            })
        ).subscribe();
    }

}
