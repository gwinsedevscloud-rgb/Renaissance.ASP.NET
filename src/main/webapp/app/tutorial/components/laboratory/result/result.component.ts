import {Component, Inject, signal} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";
import {LaboratoryDto, LaboratoryService} from "../laboratory.service";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {MatInput} from "@angular/material/input";
import {FuseAlertType} from "@mattae/angular-shared";
import {map} from "rxjs/operators";
import {catchError, EMPTY, finalize} from "rxjs";
import {MatOption, MatSelect} from "@angular/material/select";
import {NgSwitch, NgSwitchCase, NgSwitchDefault} from "@angular/common";
import {MatButton} from "@angular/material/button";

@Component({
  selector: 'app-result',
    imports: [
        MatFormField,
        MatLabel,
        FormsModule,
        MatInput,
        ReactiveFormsModule,
        MatSelect,
        MatOption,
        NgSwitchCase,
        NgSwitch,
        NgSwitchDefault,
        MatButton
    ],
  templateUrl: './result.component.html',
  styleUrl: './result.component.scss'
})
export class ResultComponent {

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    showAlert = signal(false);
    resultForm: FormGroup;
    protected isSaving = signal(false);

    constructor(
        private fb: FormBuilder,
        private labService: LaboratoryService,
        private dialogRef: MatDialogRef<ResultComponent>,
        @Inject(MAT_DIALOG_DATA) public data: LaboratoryDto
    ) {
        this.resultForm = this.fb.group({
            result: [data.result || '', Validators.required],
            note: [data.note || '']
        });
    }

    onSubmit(): void {
        if (this.resultForm.invalid) return;
        const updated: LaboratoryDto = {
            ...this.data,
            result: this.resultForm.value.result,
            note: this.resultForm.value.note
        };
        this.isSaving.set(true);
        this.labService.update(updated.id!, updated)
            .pipe(
                map(res => {
                    this.alert.message = 'Result updated successfully'
                    this.dialogRef.close(true);
                }),
                catchError(error => {
                    this.alert = {
                        message: 'There was an error updating result',
                        type: 'error'
                    }
                    return EMPTY;
                }),
                finalize(() => {
                    this.showAlert.set(true);
                })
            )
            .subscribe(() => {
                this.dialogRef.close(true);
            });
    }

}
