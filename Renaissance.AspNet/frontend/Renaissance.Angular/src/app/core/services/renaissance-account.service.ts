import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService, User } from '@mattae/angular-shared';
import { Observable, ReplaySubject, map, of } from 'rxjs';
import { AuthStateService } from './auth-state.service';
import { RenaissanceApiService } from './renaissance-api.service';
import { HospitalModuleStateService } from './hospital-module-state.service';

@Injectable()
export class RenaissanceAccountService {
    private readonly auth = inject(AuthStateService);
    private readonly api = inject(RenaissanceApiService);
    private readonly modules = inject(HospitalModuleStateService);
    private readonly router = inject(Router);
    private readonly authenticationState = new ReplaySubject<User | null>(1);

    constructor() {
        this.auth.user$.subscribe(user => {
            if (!user) {
                this.authenticationState.next(null);
                return;
            }

            const parts = user.fullName.trim().split(/\s+/);
            this.authenticationState.next({
                login: user.userName,
                firstName: parts[0] ?? user.userName,
                lastName: parts.slice(1).join(' ') || user.roleName
            } as User);
        });
    }

    get user$(): Observable<User | null> {
        return this.authenticationState.asObservable();
    }

    authenticate(_user: User | null): void {
        // Handled by AuthStateService.signIn / signOut
    }

    hasAnyAuthority(_authorities: string[] | string): boolean {
        return this.auth.isAuthenticated;
    }

    identity(force?: boolean): Observable<User | null> {
        if (!this.auth.token) {
            return of(null);
        }

        if (this.auth.user && !force) {
            return this.user$;
        }

        return this.api.getMe().pipe(
            map(user => {
                if (user) {
                    this.auth.setUser(user);
                    this.modules.apply({ enabledModules: [], referralLinks: [] });
                    this.api.getActiveHospitalModules().subscribe(state => this.modules.apply(state));
                }
                return user ? this.toFuseUser(user.fullName, user.userName, user.roleName) : null;
            })
        );
    }

    isAuthenticated(): boolean {
        return this.auth.isAuthenticated;
    }

    getAuthenticationState(): ReplaySubject<User | null> {
        return this.authenticationState;
    }

    logout(): void {
        this.auth.signOut();
        this.router.navigate(['/login']);
    }

    private toFuseUser(fullName: string, userName: string, roleName: string): User {
        const parts = fullName.trim().split(/\s+/);
        return {
            login: userName,
            firstName: parts[0] ?? userName,
            lastName: parts.slice(1).join(' ') || roleName
        } as User;
    }
}
