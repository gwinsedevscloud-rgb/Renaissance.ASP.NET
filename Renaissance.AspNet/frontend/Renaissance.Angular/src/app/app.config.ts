import { APP_INITIALIZER, ApplicationConfig, inject, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideLuxonDateAdapter } from '@angular/material-luxon-adapter';
import { AccountService } from '@mattae/angular-shared';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { provideIcons } from './core/providers/icons.provider';
import { provideFuse } from './core/providers/fuse.provider';
import { RenaissanceAccountService } from './core/services/renaissance-account.service';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { lanAccessInterceptor } from './core/interceptors/lan-access.interceptor';
import { AuthStateService } from './core/services/auth-state.service';
import { RenaissanceApiService } from './core/services/renaissance-api.service';
import { HospitalModuleStateService } from './core/services/hospital-module-state.service';

function initializeAuth(): () => Promise<void> {
    return () => {
        const auth = inject(AuthStateService);
        const api = inject(RenaissanceApiService);
        const modules = inject(HospitalModuleStateService);

        auth.restoreFromStorage();
        if (!auth.token) {
            return Promise.resolve();
        }

        return firstValueFrom(api.getMe()).then(user => {
            if (!user) {
                auth.signOut();
                return;
            }

            auth.signIn(auth.token!, user);
            return firstValueFrom(api.getActiveHospitalModules())
                .then(state => modules.apply(state))
                .catch(() => modules.apply({ enabledModules: [], referralLinks: [] }));
        });
    };
}

export const appConfig: ApplicationConfig = {
    providers: [
        { provide: AccountService, useClass: RenaissanceAccountService },
        { provide: APP_INITIALIZER, useFactory: initializeAuth, multi: true },
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideAnimationsAsync(),
        provideRouter(routes),
        provideHttpClient(withInterceptors([authInterceptor, lanAccessInterceptor])),
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
