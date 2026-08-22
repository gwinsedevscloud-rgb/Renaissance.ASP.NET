import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AppModule, CurrentUserDto } from '../models/auth.models';

const TOKEN_KEY = 'renaissance.auth.token';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
    private readonly _ready$ = new BehaviorSubject<boolean>(false);
    private readonly _user$ = new BehaviorSubject<CurrentUserDto | null>(null);
    private _token: string | null = null;

    readonly ready$ = this._ready$.asObservable();
    readonly user$ = this._user$.asObservable();

    get token(): string | null {
        return this._token;
    }

    get user(): CurrentUserDto | null {
        return this._user$.value;
    }

    get isAuthenticated(): boolean {
        return !!this._token && !!this.user;
    }

    get isReady(): boolean {
        return this._ready$.value;
    }

    hasModule(module: AppModule): boolean {
        const user = this.user;
        return !!user && (user.isAdministrator || user.modules.includes(module));
    }

    restoreFromStorage(): void {
        this._token = sessionStorage.getItem(TOKEN_KEY);
        this._ready$.next(true);
    }

    signIn(token: string, user: CurrentUserDto): void {
        this._token = token;
        this._user$.next(user);
        sessionStorage.setItem(TOKEN_KEY, token);
        this._ready$.next(true);
    }

    setUser(user: CurrentUserDto | null): void {
        this._user$.next(user);
    }

    signOut(): void {
        this._token = null;
        this._user$.next(null);
        sessionStorage.removeItem(TOKEN_KEY);
        this._ready$.next(true);
    }
}
