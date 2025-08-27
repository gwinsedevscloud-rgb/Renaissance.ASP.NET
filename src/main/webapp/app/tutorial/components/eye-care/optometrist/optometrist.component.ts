import {ChangeDetectorRef, Component, inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {ClientDashboardComponent} from "../../client-dashboard/client-dashboard.component";
import {ActivatedRoute, Router} from "@angular/router";
import {OptometristService} from "./optometrist.service";
import {MatIcon} from "@angular/material/icon";
import {MatButton, MatIconAnchor} from "@angular/material/button";
import {MatTooltip} from "@angular/material/tooltip";
import {MatFormField, MatLabel} from "@angular/material/form-field";
import {MatCheckbox} from "@angular/material/checkbox";
import {MatOption, MatSelect} from "@angular/material/select";
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-optometrist',
    imports: [
        MatIcon,
        MatIconAnchor,
        MatTooltip,
        ReactiveFormsModule,
        MatButton,
        MatFormField,
        MatLabel,
        MatCheckbox,
        MatSelect,
        MatOption,
        NgForOf
    ],
  templateUrl: './optometrist.component.html',
  styleUrl: './optometrist.component.scss'
})
export class OptometristComponent implements OnInit{

    private fb = inject(FormBuilder)
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private clientDashboardComponent = inject(ClientDashboardComponent)
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private optometristService = inject(OptometristService)

    visualAcuityOptions: string[] = ['6/6', '6/9', '6/12', '6/18', '6/24', '6/36', '6/60', 'CF', 'HM', 'PL', 'NPL'];


    id: any;
    optometristId: any

    form!: FormGroup;
    editMode = false;
    selectedId: any;

    ngOnInit(): void {
        this.clientDashboardComponent.matDrawer().open()
        this.id = this._activatedRoute.parent?.snapshot.paramMap.get('id')!;
        this.optometristId = this._activatedRoute.snapshot.paramMap.get('optId');
        console.log("Opt value **** ", this.optometristId)

        this.buildForm()

        if (this.optometristId) {
            this.optometristService.findById(this.optometristId).subscribe((data) => {
                this.form.patchValue(data);
                this.editMode = true
                this._changeDetectorRef.markForCheck();
            });
        }

    }

    buildForm() {
        this.form = this.fb.group({
            visualAcuityRight: ['', Validators.required],
            visualAcuityLeft: ['', Validators.required],
            glassesDispensed: [false],
            referred: [false]
        });
    }

    submit() {
        const dto = {
            ...this.form.value,
            patientId: this.id
        };

        if (this.editMode) {
            this.optometristService.update(this.selectedId, dto).subscribe(() => {
                this.closeDrawer()
            });
        } else {
            this.optometristService.save(dto).subscribe(() => {
                this.closeDrawer()
            });
        }
    }

    closeDrawer(): Promise<void> {
        return this.clientDashboardComponent.matDrawer().close().then(() => {
            this._router.navigate(['/client-dashboard', this.id]);
            this.clientDashboardComponent.loadOptometrist()
        });
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

}
