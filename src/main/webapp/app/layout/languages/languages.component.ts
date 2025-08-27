import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { LangDefinition, TranslocoService } from '@jsverse/transloco';
import { MatMenuModule } from '@angular/material/menu';
import { NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'languages',
    templateUrl: './languages.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    exportAs: 'languages',
    imports: [
        MatMenuModule,
        NgTemplateOutlet,
        MatButtonModule
    ],

})
export class LanguagesComponent implements OnInit, OnDestroy {
    private _translocoService = inject(TranslocoService);

    availableLangs!: LangDefinition[];
    activeLang!: string;
    flagCodes: any;

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        // Get the available languages from transloco
        this.availableLangs = this._translocoService.getAvailableLangs() as unknown as LangDefinition[];

        // Subscribe to language changes
        this._translocoService.langChanges$.subscribe((activeLang) => {

            // Get the active lang
            this.activeLang = activeLang;
        });

        // Set the country iso codes for languages for flags
        this.flagCodes = {
            'en': 'gb-eng',
            'fr': 'fr',
            'pt': 'pt',
            'es': 'es',
            'sw': 'ke',
            'ha': 'ng',
            'yo': 'ng',
            'ig': 'ng'
        };
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Set the active lang
     *
     * @param lang
     */
    setActiveLang(lang: string): void {
        // Set the active lang
        this._translocoService.setActiveLang(lang);
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any {
        return item.id || index;
    }
}
