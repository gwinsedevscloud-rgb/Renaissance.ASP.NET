import { inject, isDevMode, provideEnvironmentInitializer } from '@angular/core';
import { StylesheetService } from '@mattae/angular-shared';
import { Routes } from '@angular/router';
import {defaults} from "ngx-editor/lib/Locals";

export const routes: Routes = [
    {
        path: '',
        providers: [
            provideEnvironmentInitializer(() => {
                inject(StylesheetService).loadStylesheet(isDevMode() ? 'http://localhost:7463/styles.css':'/js/renaissance-plugin/styles.css')
            })
        ],
        children: [
            {
                path: 'clients',
                loadChildren: () => import('./tutorial.routing').then(r => r.default),
            },
            {
                path: 'client-dashboard',
                loadChildren: () => import('./components/client-dashboard/client-dashboard.routing').then(r => r.default)
            }
        ]
    }
];

export default routes;
