import { Component, inject, isDevMode, provideEnvironmentInitializer } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterOutlet, RouterStateSnapshot, Routes, UrlTree } from '@angular/router';
import { catchError, EMPTY, mergeMap, Observable, of } from 'rxjs';
import { StylesheetService } from "@mattae/angular-shared";
import {PharmacyComponent} from "../pharmacy/pharmacy.component";
import {ClientDashboardComponent} from "./client-dashboard.component";
import {TutorialService} from "../../tutorial.service";
import {TriageComponent} from "../triage/triage.component";
import {ConsultationNoteComponent} from "../consultation-note/consultation-note.component";
import {LaboratoryComponent} from "../laboratory/laboratory.component";
import {DispenseComponent} from "../pharmacy/dispense/dispense.component";
import {DentalConsultationComponent} from "../dental-consultation/dental-consultation.component";
import {AncillaryServiceComponent} from "../ancillary-service/ancillary-service.component";
import {OptometristComponent} from "../eye-care/optometrist/optometrist.component";
import {OphthalmologistComponent} from "../eye-care/ophthalmologist/ophthalmologist.component";
import {OptometristListComponent} from "../eye-care/optometrist/optometrist-list/optometrist-list.component";


const clientDashboardResolve = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any> => {
    const id = route.params['id'] ? route.params['id'] : null;
    const router = inject(Router);
    const service = inject(TutorialService);
    // @ts-ignore
    return service.getById(id).pipe(
        catchError((err) => {
            router.navigateByUrl('/clients');
            console.log("Error")
            return EMPTY;
        }),
        mergeMap((res: any) => {
            return of(res);
        })
    );
}

/*const tutorialsResolve = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any[]> | Promise<any[]> | any[] => {
    return inject(TutorialService).getAll();
}*/

const canDeactivatePharmacy = (
    component: PharmacyComponent,
    currentRoute: ActivatedRouteSnapshot,
    currentState: RouterStateSnapshot,
    nextState: RouterStateSnapshot
): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree => {
    // Get the next route
    let nextRoute: ActivatedRouteSnapshot = nextState.root;
    while (nextRoute.firstChild) {
        nextRoute = nextRoute.firstChild;
    }

    // If the next state doesn't contain '/plugins'
    // it means we are navigating away from the
    // plugin manager app
    if (!nextState.url.includes('/tutorials')) {
        // Let it navigate
        return true;
    }

    // If we are navigating to another plugin...
    if (nextState.url.includes('/new')) {
        // Just navigate
        return true;
    }
    // Otherwise...
    else {
        // Close the drawer first, and then navigate
        return component.closeDrawer().then(() => true);
    }
}

@Component({
    selector: 'client-dashboard-manager',
    template: '<router-outlet></router-outlet>',
    imports: [
        RouterOutlet
    ],
    })
export class ClientDashboardManagerComponent {

}

export default [
    {
        path: '',
        component: ClientDashboardManagerComponent,
        providers: [
            TutorialService,
            provideEnvironmentInitializer(() => {
                inject(StylesheetService).loadStylesheet(isDevMode() ? 'http://localhost:25715/styles.css' : '/js/renaissance-plugin/styles.css')
            })
        ],
        children: [
            {
                path: ':id',
                component: ClientDashboardComponent,
                resolve: {
                    client: clientDashboardResolve
                },
                data: {
                    title: 'Client Information'
                },
                children: [
                    {
                        path: 'pharmacy',
                        component: PharmacyComponent,
                        /*resolve: {
                            tutorial: tutorialResolve
                        },*/
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Pharmacy'
                        },
                        // canDeactivate: [canDeactivatePharmacy]
                    },
                    {
                        path: 'triage/:triageId',
                        component: TriageComponent,
                        data: {
                            title: 'Triage'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'triage',
                        component: TriageComponent,
                        data: {
                            title: 'Triage'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'consultation/:consultationId',
                        component: ConsultationNoteComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Consultation'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'consultation',
                        component: ConsultationNoteComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Consultation'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'laboratory/:type',
                        component: LaboratoryComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Laboratory'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'drug-dispense',
                        component: DispenseComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Drug dispense'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'dental-consultation/:consultationId',
                        component: DentalConsultationComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Dental Consultation'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'dental-consultation',
                        component: DentalConsultationComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Dental Consultation'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'ancillary-service',
                        component: AncillaryServiceComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Drug dispense'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'optometrist/:optId',
                        component: OptometristComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Optometrist'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'optometrist',
                        component: OptometristComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Optometrist'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'eye-service/:ophId',
                        component: OphthalmologistComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Eye Service'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'eye-service',
                        component: OphthalmologistComponent,
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'Eye Service'
                        },
                        // canDeactivate: [canDeactivateTutorialDetails]
                    }
                ]
            }
        ]
    }
] as Routes;

