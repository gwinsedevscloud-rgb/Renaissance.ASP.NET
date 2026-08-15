import { Injectable } from '@angular/core';
import { User } from '@mattae/angular-shared';
import { Observable, of, ReplaySubject } from 'rxjs';

@Injectable()
export class MockAccountService {
    private userIdentity: User | null = null;
    private authenticationState = new ReplaySubject<User | null>(1);

    set user(value: User) {
        this.authenticationState.next(value);
    }

    get user$(): Observable<User | null> {
        return this.authenticationState.asObservable();
    }

    authenticate(user: User | null): void {
        this.userIdentity = user;
        this.authenticationState.next(this.userIdentity);
    }

    hasAnyAuthority(_authorities: string[] | string): boolean {
        return true;
    }

    identity(_force?: boolean): Observable<User | null> {
        return of({ login: 'dev', firstName: 'Dev', lastName: 'User' } as User);
    }

    isAuthenticated(): boolean {
        return true;
    }

    getAuthenticationState(): ReplaySubject<User | null> {
        return this.authenticationState;
    }
}
