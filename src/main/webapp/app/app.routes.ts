import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { HomeComponent } from './home/home.component';

export const appRoutes: Routes = [
    {
        path: '',
        component: LayoutComponent,
        data: {
            layout: 'classy'
        },
        children: [
            {
                path: '',
                component: HomeComponent,
                pathMatch: 'full'
            },
            {
                path: '',
                loadChildren: () => import('./tutorial/plugin.routing')
            }
        ]
    }
];
