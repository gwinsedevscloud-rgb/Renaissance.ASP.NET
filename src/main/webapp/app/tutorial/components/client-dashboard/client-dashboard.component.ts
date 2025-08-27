import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    inject,
    OnDestroy,
    OnInit, signal,
    viewChild
} from '@angular/core';
import {MatAnchor, MatButton, MatButtonModule} from "@angular/material/button";
import {MatDrawer, MatDrawerContainer, MatDrawerContent, MatSidenavModule} from "@angular/material/sidenav";
import {MatIcon, MatIconModule} from "@angular/material/icon";
import {MatInput, MatInputModule} from "@angular/material/input";
import {ActivatedRoute, Router, RouterLink, RouterOutlet} from "@angular/router";
import {Patient, TutorialService} from "../../tutorial.service";
import {FuseMediaWatcherService, HasAnyAuthorityDirective} from "@mattae/angular-shared";
import {Subject, takeUntil} from "rxjs";
import {MatDialog, MatDialogModule} from "@angular/material/dialog";
import {TriageComponent} from "../triage/triage.component";
import {LaboratoryComponent} from "../laboratory/laboratory.component";
import {ConsultationNoteComponent} from "../consultation-note/consultation-note.component";
import {PharmacyComponent} from "../pharmacy/pharmacy.component";
import {Triage, TriageService} from "../triage/triage.service";
import {ListComponent} from "../triage/list/list.component";
import {PharmacyPrescription, PharmacyService} from "../pharmacy/pharmacy.service";
import {ViewPrescriptionComponent} from "../pharmacy/view-prescription/view-prescription.component";
import {LaboratoryDto, LaboratoryService} from "../laboratory/laboratory.service";
import {ViewInvestigationsComponent} from "../laboratory/view-investigations/view-investigations.component";
import {ConsultationListComponent} from "../consultation-note/consultation-list/consultation-list.component";
import {ConsultationDto, ConsultationNoteService} from "../consultation-note/consultation-note.service";
import {DentalConsultationDto, DentalConsultationService} from "../dental-consultation/dental-consultation.service";
import {
    DentalConsultationListComponent
} from "../dental-consultation/dental-consultation-list/dental-consultation-list.component";
import {AncillaryServiceDto, AncillaryServiceService} from "../ancillary-service/ancillary-service.service";
import {
    AncillaryServiceListComponent
} from "../ancillary-service/ancillary-service-list/ancillary-service-list.component";
import {MatTooltip} from "@angular/material/tooltip";
import {OptometristDto, OptometristService} from "../eye-care/optometrist/optometrist.service";
import {OphthalmologistDto, OphthalmologistService} from "../eye-care/ophthalmologist/ophthalmologist.service";
import {OptometristListComponent} from "../eye-care/optometrist/optometrist-list/optometrist-list.component";
import {
    OphthalmologistListComponent
} from "../eye-care/ophthalmologist/ophthalmologist-list/ophthalmologist-list.component";

@Component({
  selector: 'app-client-dashboard',
    imports: [
        MatButton,
        MatDrawer,
        MatDrawerContainer,
        MatDrawerContent,
        RouterOutlet,
        MatSidenavModule,
        RouterOutlet,
        MatButtonModule,
        RouterLink,
        MatIconModule,
        MatInputModule,
        MatDialogModule,
        ConsultationListComponent,
        HasAnyAuthorityDirective,
        ViewPrescriptionComponent,
        ViewInvestigationsComponent,
        ConsultationListComponent,
        ConsultationListComponent,
        ConsultationListComponent,
        ListComponent,
        DentalConsultationListComponent,
        AncillaryServiceListComponent,
        MatTooltip,
        PharmacyComponent,
        OptometristListComponent,
        OphthalmologistListComponent
    ],
  templateUrl: './client-dashboard.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './client-dashboard.component.scss'
})
export class ClientDashboardComponent implements OnInit, OnDestroy {
    private _activatedRoute = inject(ActivatedRoute);
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private _router = inject(Router);
    private _mediaWatcherService = inject(FuseMediaWatcherService);
    private _dialog = inject(MatDialog);
    private triageService = inject(TriageService)
    private pharmacyService = inject(PharmacyService)
    private laboratoryService = inject(LaboratoryService)
    private consultationNoteService = inject(ConsultationNoteService)
    private ancillaryService = inject(AncillaryServiceService)
    private dentalConsultationService = inject(DentalConsultationService)
    private optometristService = inject(OptometristService)
    private ophthalmologistService = inject(OphthalmologistService)


    drawerMode!: 'side' | 'over';
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    id: any;
    hasPending = signal<boolean>(false);
    protected client = signal<Patient | null>(null);
    readonly matDrawer = viewChild.required<MatDrawer>('matDrawer');
    tutorials: any[] = [];

    consultationDone = signal(false)
    dentalConsultationDone = signal(false)
    ancillaryServiceDone = signal(false)
    triageDone = signal(false)

    optometristDone = signal(false);
    ophthalmologistDone = signal(false);

    triage = signal<Triage[] | []>([])
    consultations = signal<ConsultationDto[] | []>([])
    dentalConsultations = signal<DentalConsultationDto[] | []>([])
    prescriptions = signal<PharmacyPrescription[]>([]);
    tests = signal<LaboratoryDto[]>([]);
    ancillaryServices = signal<AncillaryServiceDto[] | []>([])

    optometrists = signal<OptometristDto[] | []>([])
    ophthalmologists = signal<OphthalmologistDto[] | []>([])

    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }
    ngOnInit(): void {
        this.id = this._activatedRoute.snapshot.params['id'];

        this.hasPendingPrescriptions()
        this.loadPrescriptions();
        this.loadTests()

        this.loadConsultations()
        this.loadDentalConsultations()
        this.loadAncillaryService()
        this.loadTriage()

        this.loadOphthalmologist()
        this.loadOptometrist()

        this._activatedRoute.data.subscribe(({client}) => {
            if (client) {
                this.client.set(client);
                // @ts-ignore
                console.log(this.client())
            }
        });
        this._mediaWatcherService.onMediaQueryChange$('(min-width: 1440px)')
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((state) => {
                // Calculate the drawer mode
                this.drawerMode = state.matches ? 'side' : 'over';

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this.matDrawer().openedChange.subscribe(opened => {
            if (!opened) {
                // this.searchTutorial('');
            }
        })
        this._changeDetectorRef.markForCheck()

    }

    onBackdropClicked(): void {
        // Go back to the list
        this._router.navigate(['./'], {relativeTo: this._activatedRoute});

        // Mark for check
        this._changeDetectorRef.markForCheck();
    }

    openTriageDialog() {
        this._dialog.open(TriageComponent, { width: '700px' });
    }

    openLaboratory() {
        this._dialog.open(LaboratoryComponent, { width: '600px' });
    }

    openConsultationNote() {
        this._dialog.open(ConsultationNoteComponent, { width: '1200px' });
    }

    openPharmacyDialog() {
        this._dialog.open(PharmacyComponent, { width: '1200px' });
    }

    openLaboratoryResultForm (){
        this._dialog.open(LaboratoryComponent, { width: '1200px' });
    }

    hasPendingPrescriptions () {
        this.pharmacyService.hasPendingPrescriptions(this.id).subscribe({
            next: (hasPending) => {
                this.hasPending.set(hasPending)
            },
            error: (err) => {
                console.log("Error")
            }
        });
    }

    loadPrescriptions(): void {
        this.pharmacyService.getByPatientId(this.id).subscribe({
            next: data => this.prescriptions.set(data),
            error: err => console.error('Failed to load prescriptions', err),
            complete: () => {}
        });
    }

    loadConsultations(){
        this.consultationNoteService.getByPatientId(this.id).subscribe({
            next: data => {
                if(data.length > 0){
                    this.consultationDone.set(true)
                    this.consultations.set(data)
                }
            },
            error: err => console.error('Failed to load consultations:', err)
        });
    }

    loadDentalConsultations(){
        this.dentalConsultationService.getByPatientId(this.id).subscribe({
            next: data => {
                if(data.length > 0){
                    this.dentalConsultationDone.set(true)
                    this.dentalConsultations.set(data)
                }
            },
            error: err => console.error('Failed to load consultations:', err)
        });
    }

    loadAncillaryService(){
        this.ancillaryService.getByPatientId(this.id).subscribe({
            next: data => {
                if(data.length > 0){
                    this.ancillaryServiceDone.set(true)
                    this.ancillaryServices.set(data)
                }
            },
            error: err => console.error('Failed to load consultations:', err)
        });
    }

    loadTriage(){
        this.triageService.getTriagesByPatientId(this.id).subscribe({
            next: data => {
                if(data.length > 0){
                    this.triageDone.set(true)
                    this.triage.set(data)
                }
            },
            error: err => console.error('Failed to load consultations:', err)
        });
    }

    loadTests() {
        this.laboratoryService.getByPatientId(this.id).subscribe({
            next: data => this.tests.set(data),
            error: err => console.error('Failed to load tests', err),
            complete: () => {}
        })
    }

    loadOptometrist(){
        this.optometristService.findByPatient(this.id).subscribe({
            next: data => {
                if(data.length > 0){
                    this.optometrists.set(data)
                    this.optometristDone.set(true)
                    console.log("Opt *****", this.optometrists().length)
                }
            },
            error: err => console.error('Failed to load tests', err),
            complete: () => {}
        })
    }

    loadOphthalmologist(){
        this.ophthalmologistService.findByPatient(this.id).subscribe({
            next: data => {
                if(data.length > 0) {
                    this.ophthalmologists.set(data);
                    this.ophthalmologistDone.set(true)
                    console.log("Oph ***** ", this.ophthalmologists().length)
                }
            },
            error: err => console.error('Failed to load tests', err),
            complete: () => {}
        })
    }

    onEditConsultation(){
        this._router.navigate(['consultation', this.consultations()[0]?.id], { relativeTo: this._activatedRoute });
    }
    onEditTriage(){
        this._router.navigate(['triage', this.triage()[0]?.id], { relativeTo: this._activatedRoute });
    }
    onEditDentalConsultation(){
        this._router.navigate(['dental-consultation', this.dentalConsultations()[0]?.id], { relativeTo: this._activatedRoute });
    }

    onEditOphthalmologists(){
        this._router.navigate(['eye-service', this.ophthalmologists()[0]?.id], { relativeTo: this._activatedRoute });
    }


    protected readonly parent = parent;
}
