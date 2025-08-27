import { ApplicationConfig, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { appRoutes } from './app.routes';
import { AccountService } from '@mattae/angular-shared';
import { MockAccountService } from './services/mock-account.service';
import { provideTransloco } from '@jsverse/transloco';
import { provideIcons } from './providers/icons.provider';
import { provideFuse } from './providers/fuse.provider';
import { TranslocoHttpLoader } from './providers/transloco-httpLoader';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideLuxonDateAdapter } from '@angular/material-luxon-adapter';
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
    providers: [
        {
            provide: AccountService,
            useClass: MockAccountService
        },
        provideAnimationsAsync(),
        provideExperimentalZonelessChangeDetection(),
        provideRouter(appRoutes),
        provideHttpClient(),
        provideTransloco({
            config: {
                availableLangs: [
                    {
                        id: 'en',
                        label: 'English',
                    },
                    {
                        id: 'fr',
                        label: 'Français'
                    },
                    {
                        id: 'ha',
                        label: 'Hausa'
                    },
                    {
                        id: 'ig',
                        label: 'Igbo'
                    },
                    {
                        id: 'yo',
                        label: 'Yoruba'
                    },
                    {
                        id: 'es',
                        label: 'Español'
                    },
                    {
                        id: 'pt',
                        label: 'Português'
                    }
                ],
                defaultLang: 'en',
                fallbackLang: 'en',
                reRenderOnLangChange: true,
                prodMode: true,
                missingHandler: {
                    useFallbackTranslation: true
                }
            },
            loader: TranslocoHttpLoader
        }),
        provideIcons(),
        provideLuxonDateAdapter({
            parse: {
                dateInput: 'D',
            },
            display: {
                dateInput: 'DD',
                monthYearLabel: 'LLL yyyy',
                dateA11yLabel: 'DD',
                monthYearA11yLabel: 'LLLL yyyy',
            },
        }),
        provideFuse({
            fuse: {
                layout: 'classy',
                scheme: 'light',
                screens: {
                    sm: '600px',
                    md: '960px',
                    lg: '1280px',
                    xl: '1440px',
                }
            },
        })
    ]
};
