import { ChangeDetectionStrategy, Component, DestroyRef, inject, Input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, tap, catchError } from 'rxjs';
import { RenaissanceApiService } from '../../core/services/renaissance-api.service';
import { Patient } from '../../core/models/clinical.models';
import { PatientIdentityComponent } from '../patient-identity/patient-identity.component';

@Component({
    selector: 'app-patient-quick-search',
    imports: [
        FormsModule,
        RouterLink,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressSpinnerModule,
        PatientIdentityComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="ren-quick-search-card">
            <div class="ren-quick-search-inline">
                <label class="ren-quick-search-leading" [attr.for]="inputId">
                    <span class="ren-quick-search-leading-icon">
                        <mat-icon svgIcon="heroicons_outline:magnifying-glass"></mat-icon>
                    </span>
                    <span class="ren-quick-search-leading-text">Search for a Patient</span>
                </label>
                <mat-form-field class="ren-quick-search-field" subscriptSizing="dynamic">
                    <input
                        matInput
                        [id]="inputId"
                        [(ngModel)]="searchQuery"
                        (ngModelChange)="onSearchInput($event)"
                        placeholder="Number, phone, or name..." />
                    @if (searching()) {
                        <mat-spinner matSuffix diameter="20"></mat-spinner>
                    }
                </mat-form-field>
            </div>

            @if (searched() && matches().length === 0) {
                <p class="ren-quick-search-empty">No matching patients. Register a patient first if you have access.</p>
            } @else if (matches().length > 0) {
                <div class="ren-quick-search-results">
                    @for (patient of matches(); track patient.id) {
                        <div class="ren-quick-search-row">
                            <app-patient-identity [patient]="patient" />
                            <a mat-flat-button color="primary" [routerLink]="createRoute(patient.id)">
                                {{ actionLabel }}
                            </a>
                        </div>
                    }
                </div>
            }
        </div>
    `
})
export class PatientQuickSearchComponent implements OnInit {
    @Input({ required: true }) createHrefPrefix!: string;
    @Input() actionLabel = 'Open';

    private readonly api = inject(RenaissanceApiService);
    private readonly destroyRef = inject(DestroyRef);
    private readonly search$ = new Subject<string>();

    readonly inputId = `patient-search-${crypto.randomUUID()}`;
    searchQuery = '';
    readonly searching = signal(false);
    readonly searched = signal(false);
    readonly matches = signal<Patient[]>([]);

    ngOnInit(): void {
        this.search$
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                tap(query => {
                    if (!query.trim()) {
                        this.searched.set(false);
                        this.matches.set([]);
                        this.searching.set(false);
                    } else {
                        this.searching.set(true);
                    }
                }),
                switchMap(query => {
                    if (!query.trim()) {
                        return of([] as Patient[]);
                    }

                    this.searched.set(true);
                    return this.api.getPatients(query).pipe(
                        catchError(() => of([] as Patient[]))
                    );
                }),
                takeUntilDestroyed(this.destroyRef)
            )
            .subscribe(patients => {
                this.matches.set(patients.slice(0, 8));
                this.searching.set(false);
            });
    }

    onSearchInput(query: string): void {
        this.search$.next(query);
    }

    createRoute(patientId: string): string {
        return `${this.createHrefPrefix}/${patientId}`;
    }
}
