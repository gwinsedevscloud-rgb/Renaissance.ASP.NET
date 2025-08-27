import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    viewChild,
    inject,
    signal
} from '@angular/core';
import {ActivatedRoute, Router, RouterLink, RouterOutlet} from '@angular/router';
import {Patient, TutorialService} from '../../tutorial.service';
import { Subject, takeUntil } from 'rxjs';
import {MatDrawer, MatSidenavModule} from '@angular/material/sidenav';
import {FuseMediaWatcherService, HasAnyAuthorityDirective} from '@mattae/angular-shared';
import { TranslocoModule, TranslocoPipe } from "@jsverse/transloco";
import {MatButtonModule} from "@angular/material/button";
import {MatIconModule} from "@angular/material/icon";
import {MatInputModule} from "@angular/material/input";

@Component({
    selector: 'tutorial-list',
    templateUrl: './tutorial-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        MatSidenavModule,
        RouterOutlet,
        MatButtonModule,
        RouterLink,
        MatIconModule,
        MatInputModule,
        TranslocoPipe,
        HasAnyAuthorityDirective
    ],
})
export class TutorialListComponent implements OnInit, OnDestroy {
    private _tutorialService = inject(TutorialService);
    protected _activatedRoute = inject(ActivatedRoute);
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private _router = inject(Router);
    private _mediaWatcherService = inject(FuseMediaWatcherService);

    readonly matDrawer = viewChild.required<MatDrawer>('matDrawer');
    patients = signal<Patient[]>([]);
    keyword = '';
    pageSize = 10;
    drawerMode!: 'side' | 'over';
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    ngOnInit(): void {
        this._activatedRoute.data.subscribe(({patients}) => {
            this.patients.set(patients);
            console.log("patient", this.patients())
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
                this.searchTutorial('');
            }
        })
    }

    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    filterByQuery(query: string): any {
        this.searchTutorial(query)
    }

    onBackdropClicked(): void {
        // Go back to the list
        this._router.navigate(['./'], {relativeTo: this._activatedRoute});

        // Mark for check
        this._changeDetectorRef.markForCheck();
    }

    searchTutorial(title: any): void {
        this._tutorialService.getByClientNumber(title).subscribe(res => {
            this.patients.set(res);
        });
    }

    getAllPatients() {
        this._tutorialService.getAll().subscribe(res =>{
            this.patients.set(res)
        })
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }
}
