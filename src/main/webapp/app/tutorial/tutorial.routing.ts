import { Component, inject, isDevMode, provideEnvironmentInitializer } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterOutlet, RouterStateSnapshot, Routes, UrlTree } from '@angular/router';
import { TutorialListComponent } from './components/list/tutorial-list.component';
import { TutorialService } from './tutorial.service';
import { TutorialDetailsComponent } from './components/details/tutorial.details.component';
import { catchError, EMPTY, mergeMap, Observable, of } from 'rxjs';
import { StylesheetService } from "@mattae/angular-shared";
import {ClientDashboardComponent} from "./components/client-dashboard/client-dashboard.component";


const tutorialResolve = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any> => {
    const id = route.params['id'] ? route.params['id'] : null;
    const router = inject(Router);
    const service = inject(TutorialService);
    // @ts-ignore
    return service.getById(id).pipe(
        catchError((err) => {
            router.navigateByUrl('/clients');
            return EMPTY;
        }),
        mergeMap((res: any) => {
            return of(res);
        })
    );
}

const tutorialsResolve = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any[]> | Promise<any[]> | any[] => {
    return inject(TutorialService).getAll();
}

const canDeactivateTutorialDetails = (
    component: TutorialDetailsComponent,
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
    if (!nextState.url.includes('/clients')) {
        // Let it navigate
        return true;
    }

    // If we are navigating to another plugin...
    if (nextState.url.includes('/details')) {
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
    selector: 'tutorial-manager',
    template: '<router-outlet></router-outlet>',
    imports: [
        RouterOutlet
    ],
    })
export class TutorialManagerComponent {

}

export default [
    {
        path: '',
        component: TutorialManagerComponent,
        providers: [
            TutorialService,
            provideEnvironmentInitializer(() => {
                inject(StylesheetService).loadStylesheet(isDevMode() ? 'http://localhost:7463/styles.css' : '/js/renaissance-plugin/styles.css')
            })
        ],
        children: [
            {
                path: '',
                component: TutorialListComponent , //ClientDashboardComponent,
                resolve: {
                    patients: tutorialsResolve
                },
                data: {
                    title: 'PLUGINS.TUTORIAL.TITLES.TUTORIALS'
                },
                children: [
                    {
                        path: 'details/:id',
                        component: TutorialDetailsComponent,
                        resolve: {
                            tutorial: tutorialResolve
                        },
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'PLUGINS.TUTORIAL.TITLES.TUTORIAL_DETAILS'
                        },
                        canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'details/:id/:view',
                        component: TutorialDetailsComponent,
                        resolve: {
                            tutorial: tutorialResolve
                        },
                        data: {
                            authorities: ['ROLE_USER'],
                            title: 'PLUGINS.TUTORIAL.TITLES.TUTORIAL_DETAILS'
                        },
                        canDeactivate: [canDeactivateTutorialDetails]
                    },
                    {
                        path: 'details',
                        component: TutorialDetailsComponent,
                        data: {
                            title: 'PLUGINS.TUTORIAL.TITLES.NEW_TUTORIAL'
                        },
                        canDeactivate: [canDeactivateTutorialDetails]
                    },
                ]
            }
        ]
    }
] as Routes;

