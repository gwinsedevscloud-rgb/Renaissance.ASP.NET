import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';

export const routes: Routes = [
    {
        path: '',
        component: LayoutComponent,
        children: [
            {
                path: '',
                loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
            },
            {
                path: 'clients',
                loadComponent: () => import('./features/clients/clients-list.component').then(m => m.ClientsListComponent)
            },
            {
                path: 'clients/new',
                loadComponent: () => import('./features/clients/client-form.component').then(m => m.ClientFormComponent)
            },
            {
                path: 'clients/:id/edit',
                loadComponent: () => import('./features/clients/client-form.component').then(m => m.ClientFormComponent)
            },
            {
                path: 'clients/:id',
                loadComponent: () => import('./features/clients/client-detail.component').then(m => m.ClientDetailComponent)
            },
            {
                path: 'client-dashboard/:id',
                loadComponent: () => import('./features/dashboard/client-dashboard.component').then(m => m.ClientDashboardComponent)
            },
            {
                path: 'stakeholders',
                loadComponent: () => import('./features/stakeholders/stakeholders-dashboard.component').then(m => m.StakeholdersDashboardComponent)
            }
        ]
    },
    { path: '**', redirectTo: '' }
];
