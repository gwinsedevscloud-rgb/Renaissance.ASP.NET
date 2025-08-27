import {ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject, signal} from '@angular/core';
import {FormGroup, ReactiveFormsModule, UntypedFormBuilder, Validators} from '@angular/forms';
import {TutorialService} from '../../tutorial.service';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {MatDrawerToggleResult} from '@angular/material/sidenav';
import {TutorialListComponent} from '../list/tutorial-list.component';
import {MatButtonModule} from "@angular/material/button";
import {TranslocoModule} from "@jsverse/transloco";
import {MatCheckboxModule} from "@angular/material/checkbox";
import {MatTooltipModule} from "@angular/material/tooltip";
import {MatInputModule} from "@angular/material/input";
import {MatIconModule} from "@angular/material/icon";
import {catchError, EMPTY, finalize, map, startWith, combineLatest} from "rxjs";
import {FuseAlertComponent, FuseAlertType} from "@mattae/angular-shared";
import {MatOption, MatSelect} from "@angular/material/select";

@Component({
    selector: 'tutorial-details',
    templateUrl: './tutorial.detail.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        MatButtonModule,
        RouterLink,
        TranslocoModule,
        MatCheckboxModule,
        MatTooltipModule,
        ReactiveFormsModule,
        MatInputModule,
        MatIconModule,
        FuseAlertComponent,
        MatSelect,
        MatOption
    ],
})
export class TutorialDetailsComponent implements OnInit {
    private _tutorialService = inject(TutorialService);
    private _activatedRoute = inject(ActivatedRoute);
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private _fb = inject(UntypedFormBuilder);
    private _router = inject(Router);
    private tutorialListComponent = inject(TutorialListComponent);

    tutorial: any;
    pathHasId = false;
    clientNumber = signal<string>('');

    editMode: boolean = false;
    formGroup!: FormGroup;
    showAlert = false;
    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };

    pathHasView = false;

    clientCodee = signal<string>('');


    ngOnInit(): void {
        this.formGroup = this._fb.group({
            clientNumber: [null, Validators.required],
            age: [null, Validators.required],
            ageUnit: [null],
            sex: [null],
            tribe: [null],
            religion: [null],
            address: [null],
            occupation: [null],
            education: [null],
            maritalStatus: [null],
            phoneNumber: [null]
        });

        this.setupDynamicValidation()
        this.readOnlyModeForPatientFormView()

        /*this.formGroup.statusChanges.subscribe(status => {
            console.log('Form Validity:', status);

            if (status === 'INVALID') {
                const invalidControls = Object.keys(this.formGroup.controls).filter(
                    key => this.formGroup.get(key)?.invalid
                );

                console.log('Invalid Fields:', invalidControls);

                // Optional: Log error details
                invalidControls.forEach(field => {
                    console.log(`${field} errors:`, this.formGroup.get(field)?.errors);
                });
            }
        });*/

        this.tutorialListComponent.matDrawer().open();
        this.pathHasId = !!this._activatedRoute.snapshot.params['id'];
        this.pathHasView = !!this._activatedRoute.snapshot.params['view'];
        console.log('View Has **** ', this.pathHasView)
        console.log('Path Has Id **** ', this.pathHasId)
        this._activatedRoute.data.subscribe(({tutorial}) => {
            if (tutorial) {
                this.tutorial = tutorial;
                this.formGroup.patchValue(tutorial);
            } else {
                this.editMode = true;
                this.tutorial = {}
            }

            this._changeDetectorRef.markForCheck();
        });

        // this.generateClientNumber()

    }

    readOnlyModeForPatientFormView(): void {
        if (this.pathHasView) {
            Object.keys(this.formGroup.controls).forEach(controlName => {
                this.formGroup.get(controlName)?.disable({ emitEvent: false, onlySelf: true});
            });
        } else {
            Object.keys(this.formGroup.controls).forEach(controlName => {
                if (controlName === 'clientNumber') return;
                this.formGroup.get(controlName)?.enable({ emitEvent: false, onlySelf: true });
            });
        }

        this._changeDetectorRef.markForCheck(); // << Trigger update
    }


    setupDynamicValidation(): void {
        const ageControl = this.formGroup.get('age');
        const ageUnitControl = this.formGroup.get('ageUnit');
        const phoneControl = this.formGroup.get('phoneNumber');
        const addressControl = this.formGroup.get('address');
        const sexControl = this.formGroup.get('sex');
        const educationControl = this.formGroup.get('education');
        const occupationControl = this.formGroup.get('occupation');

        combineLatest([
            ageControl!.valueChanges.pipe(startWith(ageControl!.value)),
            phoneControl!.valueChanges.pipe(startWith(phoneControl!.value)),
            addressControl!.valueChanges.pipe(startWith(addressControl!.value))
        ]).subscribe(([ageVal, phoneVal, addressVal]) => {
            const age = parseInt(ageVal, 10);
            const hasPhone = !!phoneVal && nigerianPhoneValidator({ value: phoneVal }) === null;
            const hasAddress = !!addressVal && addressVal.trim().length > 0;

            // Clear all validators before reapplying
            ageUnitControl?.clearValidators();
            sexControl?.clearValidators();
            educationControl?.clearValidators();
            occupationControl?.clearValidators();
            phoneControl?.clearValidators();
            addressControl?.clearValidators();

            // Always require ageUnit
            ageUnitControl?.setValidators(Validators.required);

            if (!isNaN(age)) {
                // Always required
                sexControl?.setValidators(Validators.required);
                educationControl?.setValidators(Validators.required);

                if (age >= 16) {
                    occupationControl?.setValidators(Validators.required);

                    if (!hasPhone && !hasAddress) {
                        // Neither provided — require both
                        phoneControl?.setValidators([Validators.required, nigerianPhoneValidator]);
                        addressControl?.setValidators(Validators.required);
                    } else if (!hasPhone && hasAddress) {
                        // Only address provided
                        addressControl?.clearValidators();
                        phoneControl?.clearValidators(); // not required
                    } else {
                        // Valid phone, address optional
                        phoneControl?.setValidators(nigerianPhoneValidator);
                    }
                }
            }

            // Trigger re-validation
            ageUnitControl?.updateValueAndValidity();
            sexControl?.updateValueAndValidity();
            educationControl?.updateValueAndValidity();
            occupationControl?.updateValueAndValidity();
            phoneControl?.updateValueAndValidity();
            addressControl?.updateValueAndValidity();
        });
    }


    closeDrawer(): Promise<MatDrawerToggleResult> {
        return this.tutorialListComponent.matDrawer().close();
    }

    toggleEditMode(editMode: boolean | null = null): void {
        if (editMode === null) {
            this.editMode = !this.editMode;
        } else {
            this.editMode = editMode;
        }

        this._changeDetectorRef.markForCheck();
    }

    generateClientNumber (){
        this._tutorialService.generateClientNumber().subscribe({
            next: number => {
                this.clientNumber.set(number);
                this.formGroup.patchValue({ clientNumber: number });
                // this.formGroup.get('clientNumber')?.disable();
            },
            error: err => console.error('Failed to generate client number', err)
        });
    }

    isClientNumberLessThanSixteen(): boolean {
        const value = this.formGroup.get('age')?.value;
        const ageUnit = this.formGroup.get('ageUnit')?.value;
        return (value !== null && value !== undefined && Number(value) < 16) || (ageUnit !== null && ageUnit !== undefined && ageUnit == 'months');
    }


    save(): void {
        this.tutorial = this.formGroup.value;
        this.showAlert = false;

        this._tutorialService.create(this.tutorial).pipe(
            map(res => {
                this.tutorial = res;
                this.alert.message = `Patient saved successfully.<br><br>Client code is <b>${this.clientNumber()}</b>`;
                this.formGroup.reset({
                    clientNumber: null,
                    age: null,
                    ageUnit: 'years',
                    sex: 'female',
                    tribe: null,
                    religion: null,
                    address: null,
                    occupation: null,
                    education: null,
                    maritalStatus: null,
                    phoneNumber: null
                });
                this.tutorialListComponent.getAllPatients()
            }),
            catchError(error => {
                this.alert = {
                    message: 'There was an error saving this patient',
                    type: 'error'
                }
                return EMPTY;
            }),
            finalize(() => {
                this.editMode = false;
                this.showAlert = true;

                this._changeDetectorRef.markForCheck();
            })
        ).subscribe();
    }

    delete() {
        this._tutorialService.archive(this.tutorial.id).subscribe(res => {
            this._router.navigate(['../..'])
        })
    }
}


import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function nigerianPhoneValidator(control: { value: any }): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    // Check length = 11
    if (value.length !== 11) {
        return { invalidPhoneNumber: true };
    }

    // Nigerian phone format: starts with 0 followed by 7x, 8x, or 9[0-1] and then 8 digits
    const regex = /^0(7[0-9]|8[0-9]|9[0-1])\d{8}$/;
    const valid = regex.test(value);

    return valid ? null : { invalidPhoneNumber: true };
}


export function addressIfNoPhoneValidator(group: AbstractControl): ValidationErrors | null {
    const phone = group.get('phoneNumber')?.value;
    const address = group.get('address');

    if (!phone || phone.trim() === '') {
        if (!address?.value || address.value.trim() === '') {
            address?.setErrors({ required: true });
            return { addressRequired: true };
        } else {
            address?.setErrors(null);
        }
    } else {
        address?.setErrors(null); // clear if phone is provided
    }

    return null;
}
