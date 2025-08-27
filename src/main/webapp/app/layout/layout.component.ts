import { Component, inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { ClassyLayoutComponent } from './layouts/vertical/classy/classy.component';

@Component({
    selector: 'layout',
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.scss'],
    encapsulation: ViewEncapsulation.None,
    imports: [
        ClassyLayoutComponent
    ],

})
export class LayoutComponent implements OnInit, OnDestroy {
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);

    layout!: string;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {


        // Subscribe to NavigationEnd event
        this._router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            takeUntil(this._unsubscribeAll),
        ).subscribe(() => {
            // Update the layout
            this._updateLayout();
        });
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Private methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Update the selected layout
     */
    private _updateLayout(): void {
        // Get the current activated route
        let route = this._activatedRoute;
        while (route.firstChild) {
            route = route.firstChild;
        }

        const paths = route.pathFromRoot;
        paths.forEach((path) => {

            // Check if there is a 'layout' data
            if (path.routeConfig && path.routeConfig.data && path.routeConfig.data.layout) {
                // Set the layout
                this.layout = path.routeConfig.data.layout;
            }
        });
    }
}
