import {
    EnvironmentProviders,
    importProvidersFrom,
    inject,
    provideEnvironmentInitializer,
    Provider
} from '@angular/core';
import { MatDialogModule } from '@angular/material/dialog';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import {
    FUSE_CONFIG,
    FuseConfig,
    FuseConfirmationService,
    FuseMediaWatcherService,
    FuseUtilsService
} from '@mattae/angular-shared';

export type FuseProviderConfig = {
    fuse?: FuseConfig
}

/**
 * Fuse provider
 */
export const provideFuse = (config: FuseProviderConfig): Array<Provider | EnvironmentProviders> => {
    // Base providers
    // Return the providers
    return [
        {
            // Use the 'fill' appearance on Angular Material form fields by default
            provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
            useValue: {
                appearance: 'fill',
            },
        },
        importProvidersFrom(MatDialogModule),
        provideEnvironmentInitializer(() => {
            inject(FuseConfirmationService)
            inject(FuseMediaWatcherService)
            inject(FuseUtilsService)
        }),
        {
            provide: FUSE_CONFIG,
            useValue: config?.fuse ?? {},
        }
    ];
};
