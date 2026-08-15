import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideLuxonDateAdapter } from '@angular/material-luxon-adapter';
import { AccountService } from '@mattae/angular-shared';
import { routes } from './app.routes';
import { provideIcons } from './core/providers/icons.provider';
import { provideFuse } from './core/providers/fuse.provider';
import { MockAccountService } from './core/services/mock-account.service';

export const appConfig: ApplicationConfig = {
    providers: [
        { provide: AccountService, useClass: MockAccountService },
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideAnimationsAsync(),
        provideRouter(routes),
        provideHttpClient(),
        provideIcons(),
        provideLuxonDateAdapter(),
        provideFuse({
            fuse: {
                layout: 'classy',
                scheme: 'light',
                screens: { sm: '600px', md: '960px', lg: '1280px', xl: '1440px' }
            }
        })
    ]
};
