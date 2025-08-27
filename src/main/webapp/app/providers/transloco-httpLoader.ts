import { Translation, TranslocoLoader } from "@jsverse/transloco";
import { inject, Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { forkJoin, Observable } from "rxjs";
import { map } from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class TranslocoHttpLoader implements TranslocoLoader {
    private _httpClient = inject(HttpClient);

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get translation
     *
     * @param lang
     */
    getTranslation(lang: string): Observable<Translation> {
        return forkJoin([
            this._httpClient.get<Translation>(`./static/i18n/${lang}.json`),
            this._httpClient.get<Translation>(`./assets/i18n/${lang}.json`)
        ]).pipe(
            map(([translations1, translations2]) => {
                return Object.assign({}, translations1, translations2)
            })
        );
    }
}
