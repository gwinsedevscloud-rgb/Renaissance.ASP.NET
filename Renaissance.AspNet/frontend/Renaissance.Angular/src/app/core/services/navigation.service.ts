import { Injectable } from '@angular/core';
import { FuseNavigationItem } from '@mattae/angular-shared';

@Injectable({ providedIn: 'root' })
export class NavigationService {
    navigations(): FuseNavigationItem[] {
        return [
            {
                title: 'Home',
                type: 'basic',
                link: '/',
                icon: 'heroicons_outline:home'
            },
            {
                title: 'Clients',
                type: 'basic',
                link: '/clients',
                icon: 'heroicons_outline:users'
            }
        ];
    }
}
