import { Routes } from '@angular/router';

import { LayoutComponent } from './layout/layout.component';

import { authGuard, guestGuard } from './core/guards/auth.guard';

import { moduleGuard } from './core/guards/module.guard';

import { AppModule } from './core/models/auth.models';



export const routes: Routes = [

    {

        path: 'login',

        canActivate: [guestGuard],

        loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)

    },

    {

        path: '',

        component: LayoutComponent,

        canActivate: [authGuard],

        children: [

            {

                path: '',

                loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)

            },

            // Patients (Blazor paths + client aliases)

            {

                path: 'patients',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/clients-list.component').then(m => m.ClientsListComponent)

            },

            {

                path: 'patients/create',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/client-form.component').then(m => m.ClientFormComponent)

            },

            {

                path: 'patients/edit/:id',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/client-form.component').then(m => m.ClientFormComponent)

            },

            {

                path: 'patients/details/:id',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/client-detail.component').then(m => m.ClientDetailComponent)

            },

            {

                path: 'clients',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/clients-list.component').then(m => m.ClientsListComponent)

            },

            {

                path: 'clients/new',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/client-form.component').then(m => m.ClientFormComponent)

            },

            {

                path: 'clients/:id/edit',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/client-form.component').then(m => m.ClientFormComponent)

            },

            {

                path: 'clients/:id',

                canActivate: [moduleGuard(AppModule.Clients)],

                loadComponent: () => import('./features/clients/client-detail.component').then(m => m.ClientDetailComponent)

            },

            {

                path: 'client-dashboard/:id',

                canActivate: [moduleGuard(AppModule.ClientDashboard)],

                loadComponent: () => import('./features/dashboard/client-dashboard.component').then(m => m.ClientDashboardComponent)

            },

            {

                path: 'patient-dashboard/:id',

                canActivate: [moduleGuard(AppModule.ClientDashboard)],

                loadComponent: () => import('./features/dashboard/client-dashboard.component').then(m => m.ClientDashboardComponent)

            },

            // Stakeholders

            {

                path: 'stakeholders',

                canActivate: [moduleGuard(AppModule.Stakeholders)],

                loadComponent: () => import('./features/stakeholders/stakeholders-dashboard.component').then(m => m.StakeholdersDashboardComponent)

            },

            // Triage

            {

                path: 'triage',

                canActivate: [moduleGuard(AppModule.Triage)],

                loadComponent: () => import('./features/triage/triage-list.component').then(m => m.TriageListComponent)

            },

            {

                path: 'triage/create/:patientId',

                canActivate: [moduleGuard(AppModule.Triage)],

                loadComponent: () => import('./features/triage/triage-create.component').then(m => m.TriageCreateComponent)

            },

            {

                path: 'triage/edit/:id',

                canActivate: [moduleGuard(AppModule.Triage)],

                loadComponent: () => import('./features/triage/triage-edit.component').then(m => m.TriageEditComponent)

            },

            // Consultations

            {

                path: 'consultations',

                canActivate: [moduleGuard(AppModule.Consultations)],

                loadComponent: () => import('./features/consultations/consultations-list.component').then(m => m.ConsultationsListComponent)

            },

            {

                path: 'consultations/create/:patientId',

                canActivate: [moduleGuard(AppModule.Consultations)],

                loadComponent: () => import('./features/consultations/consultation-create.component').then(m => m.ConsultationCreateComponent)

            },

            {

                path: 'consultations/edit/:id',

                canActivate: [moduleGuard(AppModule.Consultations)],

                loadComponent: () => import('./features/consultations/consultation-edit.component').then(m => m.ConsultationEditComponent)

            },

            // Pharmacy

            {

                path: 'pharmacy',

                canActivate: [moduleGuard(AppModule.Pharmacy)],

                loadComponent: () => import('./features/pharmacy/pharmacy-list.component').then(m => m.PharmacyListComponent)

            },

            {

                path: 'pharmacy/create/:patientId',

                canActivate: [moduleGuard(AppModule.Pharmacy)],

                loadComponent: () => import('./features/pharmacy/pharmacy-create.component').then(m => m.PharmacyCreateComponent)

            },

            {

                path: 'pharmacy/dispense/:patientId',

                canActivate: [moduleGuard(AppModule.Pharmacy)],

                loadComponent: () => import('./features/pharmacy/pharmacy-dispense.component').then(m => m.PharmacyDispenseComponent)

            },

            // Laboratory

            {

                path: 'laboratory',

                canActivate: [moduleGuard(AppModule.Laboratory)],

                loadComponent: () => import('./features/laboratory/laboratory-list.component').then(m => m.LaboratoryListComponent)

            },

            {

                path: 'laboratory/create/:patientId',

                canActivate: [moduleGuard(AppModule.Laboratory)],

                loadComponent: () => import('./features/laboratory/laboratory-create.component').then(m => m.LaboratoryCreateComponent)

            },

            {

                path: 'laboratory/edit/:id',

                canActivate: [moduleGuard(AppModule.Laboratory)],

                loadComponent: () => import('./features/laboratory/laboratory-edit.component').then(m => m.LaboratoryEditComponent)

            },

            // Dental

            {

                path: 'dental',

                canActivate: [moduleGuard(AppModule.Dental)],

                loadComponent: () => import('./features/dental/dental-list.component').then(m => m.DentalListComponent)

            },

            {

                path: 'dental/create/:patientId',

                canActivate: [moduleGuard(AppModule.Dental)],

                loadComponent: () => import('./features/dental/dental-create.component').then(m => m.DentalCreateComponent)

            },

            {

                path: 'dental/edit/:id',

                canActivate: [moduleGuard(AppModule.Dental)],

                loadComponent: () => import('./features/dental/dental-edit.component').then(m => m.DentalEditComponent)

            },

            // Ancillary

            {

                path: 'ancillary',

                canActivate: [moduleGuard(AppModule.Ancillary)],

                loadComponent: () => import('./features/ancillary/ancillary-list.component').then(m => m.AncillaryListComponent)

            },

            {

                path: 'ancillary/create/:patientId',

                canActivate: [moduleGuard(AppModule.Ancillary)],

                loadComponent: () => import('./features/ancillary/ancillary-create.component').then(m => m.AncillaryCreateComponent)

            },

            {

                path: 'ancillary/edit/:id',

                canActivate: [moduleGuard(AppModule.Ancillary)],

                loadComponent: () => import('./features/ancillary/ancillary-edit.component').then(m => m.AncillaryEditComponent)

            },

            // Optometrists

            {

                path: 'optometrists',

                canActivate: [moduleGuard(AppModule.Optometrists)],

                loadComponent: () => import('./features/optometrists/optometrists-list.component').then(m => m.OptometristsListComponent)

            },

            {

                path: 'optometrists/create/:patientId',

                canActivate: [moduleGuard(AppModule.Optometrists)],

                loadComponent: () => import('./features/optometrists/optometrist-create.component').then(m => m.OptometristCreateComponent)

            },

            {

                path: 'optometrists/edit/:id',

                canActivate: [moduleGuard(AppModule.Optometrists)],

                loadComponent: () => import('./features/optometrists/optometrist-edit.component').then(m => m.OptometristEditComponent)

            },

            // Ophthalmologists

            {

                path: 'ophthalmologists',

                canActivate: [moduleGuard(AppModule.Ophthalmologists)],

                loadComponent: () => import('./features/ophthalmologists/ophthalmologists-list.component').then(m => m.OphthalmologistsListComponent)

            },

            {

                path: 'ophthalmologists/create/:patientId',

                canActivate: [moduleGuard(AppModule.Ophthalmologists)],

                loadComponent: () => import('./features/ophthalmologists/ophthalmologist-create.component').then(m => m.OphthalmologistCreateComponent)

            },

            {

                path: 'ophthalmologists/edit/:id',

                canActivate: [moduleGuard(AppModule.Ophthalmologists)],

                loadComponent: () => import('./features/ophthalmologists/ophthalmologist-edit.component').then(m => m.OphthalmologistEditComponent)

            },

            // Admin

            {

                path: 'admin/users',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/users.component').then(m => m.UsersComponent)

            },

            {

                path: 'admin/users/create',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/user-edit.component').then(m => m.UserEditComponent)

            },

            {

                path: 'admin/users/edit/:id',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/user-edit.component').then(m => m.UserEditComponent)

            },

            {

                path: 'admin/roles',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/roles.component').then(m => m.RolesComponent)

            },

            {

                path: 'admin/roles/create',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/role-edit.component').then(m => m.RoleEditComponent)

            },

            {

                path: 'admin/roles/edit/:id',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/role-edit.component').then(m => m.RoleEditComponent)

            },

            {

                path: 'admin/settings',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/settings.component').then(m => m.SettingsComponent)

            },

            {

                path: 'admin/backup',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/backup.component').then(m => m.BackupComponent)

            },

            {

                path: 'admin/export',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/export.component').then(m => m.ExportComponent)

            },

            {

                path: 'admin/lan-access',

                canActivate: [moduleGuard(AppModule.Administration)],

                loadComponent: () => import('./features/admin/lan-access.component').then(m => m.LanAccessComponent)

            },

            {

                path: 'access-denied',

                loadComponent: () => import('./features/errors/access-denied.component').then(m => m.AccessDeniedComponent)

            },

            {

                path: 'not-found',

                loadComponent: () => import('./features/errors/not-found.component').then(m => m.NotFoundComponent)

            }

        ]

    },

    { path: '**', redirectTo: 'not-found' }

];

