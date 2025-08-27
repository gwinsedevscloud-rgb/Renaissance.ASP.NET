// @ts-ignore

import {ChangeDetectorRef, Component, EventEmitter, inject, Input, OnInit, Output, signal} from '@angular/core';
import {FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule} from "@angular/forms";
import {Location, NgForOf, NgSwitch, NgSwitchCase, NgSwitchDefault} from "@angular/common";
import {MatButton, MatIconAnchor} from "@angular/material/button";
import {ClientDashboardComponent} from "../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {MatIcon} from "@angular/material/icon";
import {MatTooltip} from "@angular/material/tooltip";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {MatOption, MatSelect} from "@angular/material/select";
import {MatInput} from "@angular/material/input";
import {MatCheckbox, MatCheckboxChange} from "@angular/material/checkbox";
import {LaboratoryDto, LaboratoryService} from "./laboratory.service";
import {FuseAlertType} from "@mattae/angular-shared";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";

// lab-test.model.ts
export interface LabTestOrder {
    fastingBloodSugar?: {
        sampleType: 'fasting' | 'random' | '';
        result?: number;
    };
    malariaParasite?: 'positive' | 'negative' | '';
    hepatitisB?: 'positive' | 'negative' | '';
    hepatitisC?: 'positive' | 'negative' | '';
    rvs?: 'positive' | 'negative' | '';
}

@Component({
  selector: 'app-laboratory',
    imports: [
        ReactiveFormsModule,
        NgForOf,
        MatButton,
        NgSwitch,
        NgSwitchCase,
        NgSwitchDefault,
        MatIcon,
        MatIconAnchor,
        MatTooltip,
        MatFormField,
        MatLabel,
        MatSelect,
        MatInput,
        MatOption,
        MatCheckbox
    ],
  templateUrl: './laboratory.component.html',
  styleUrl: './laboratory.component.scss'
})
export class LaboratoryComponent implements OnInit {
    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private laboratoryService = inject(LaboratoryService)

    type = signal<'order' | 'result'>('order');

    id: any
    labForm!: FormGroup;
    tests = [
        'Random Blood Sugar',
        'Malaria Parasite (RDT)',
        'Hepatitis B',
        'Hepatitis C',
        'HIV Test',
        'Cholesterol',
        'PSA'
    ];

    requestedTests = signal<string[]>([]);

    resultForm!: FormGroup;

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    showAlert = signal(false);

    @Output() formSubmit = new EventEmitter<LabTestOrder>();
    ngOnInit(): void {
        this.initForms()
        this.checkWhichFormType()
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;

    }

    private initForms(): void {
        this.labForm = this.fb.group({
            selectAll: false,
            requestedTests: [[]],
        });

        const resultControls: Record<string, FormControl> = {};
        this.tests.forEach((test) => {
            resultControls[this.formatKey(test)] = new FormControl('');
        });

        this.resultForm = this.fb.group(resultControls);
    }

    checkWhichFormType() {
        this._activatedRoute.paramMap.subscribe(params => {
            const param = params.get('type')
            if (param === 'order' || param === 'result'){
                this.type.set(param)
            }
        })
    }

    onCheckChange(event: MatCheckboxChange, test: string) {
        const current = this.labForm.value.requestedTests as string[];
        const updated = event.checked
            ? [...current, test]
            : current.filter((t) => t !== test);

        this.labForm.patchValue({ requestedTests: updated });
        // @ts-ignore
        this.requestedTests.set(updated);

        console.log(this.requestedTests());
    }


    toggleSelectAll(event: MatCheckboxChange): void {
        const isChecked = event.checked;
        const allTests = isChecked ? [...this.tests] : [];
        this.labForm.patchValue({ requestedTests: allTests, selectAll: isChecked });
        this.requestedTests.set(allTests); // assuming requestedTests is a signal
    }

    onSubmitOrder() {
        const testsToOrder = this.labForm.value.requestedTests;
        if (!testsToOrder?.length) return;

        // @ts-ignore
        const dtos: LaboratoryDto[] = testsToOrder.map((test) => ({
            patientId: this.id,
            testName: test,
            result: '',
        }));

        this.laboratoryService.saveAll(dtos).pipe(
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
        ).subscribe()

        console.log("Test Ordered ***** ", dtos)
    }

    formatKey(label: string): string {
        return label
            .replace(/[()]/g, '')                     // remove parentheses
            .replace(/[^a-zA-Z0-9 ]/g, '')            // remove other non-alphanumerics
            .trim()
            .split(/\s+/)
            .map((word, index) =>
                index === 0
                    ? word.toLowerCase()
                    : word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
            )
            .join('');
    }

    onSubmitResult() {
        console.log(this.toCamelCase(this.resultForm.value));
    }

    toCamelCase(obj: Record<string, any>): Record<string, any> {
        const result: any = {};
        for (const key in obj) {
            const camelKey = key.replace(/_([a-z])/g, (_, char) => char.toUpperCase());
            result[camelKey] = obj[key];
        }
        return result;
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadTests()
        });
    }
}
