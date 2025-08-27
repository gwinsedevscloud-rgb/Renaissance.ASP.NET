import { EnvironmentProviders, inject, provideEnvironmentInitializer, Provider } from "@angular/core";
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry } from '@angular/material/icon';

export const provideIcons = (): Array<Provider | EnvironmentProviders> => {
    return [
        provideEnvironmentInitializer(() => {
            const domSanitizer = inject(DomSanitizer);
            const matIconRegistry = inject(MatIconRegistry);

            // Register icon sets
            matIconRegistry.addSvgIconSet(domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/material-twotone.svg'));
            matIconRegistry.addSvgIconSetInNamespace('mat_outline', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/material-outline.svg'));
            matIconRegistry.addSvgIconSetInNamespace('mat_solid', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/material-solid.svg'));
            matIconRegistry.addSvgIconSetInNamespace('feather', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/feather.svg'));
            matIconRegistry.addSvgIconSetInNamespace('heroicons_outline', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/heroicons-outline.svg'));
            matIconRegistry.addSvgIconSetInNamespace('heroicons_solid', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/heroicons-solid.svg'));
            matIconRegistry.addSvgIconSetInNamespace('heroicons_mini', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/heroicons-mini.svg'));
        })
    ];
};
