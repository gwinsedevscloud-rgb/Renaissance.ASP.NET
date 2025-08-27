import { Injectable } from '@angular/core';
import { Observable, of, ReplaySubject } from 'rxjs';
import { User } from '@mattae/angular-shared';

@Injectable({providedIn: 'root'})
export class MockAccountService {
    private userIdentity: User | null = null;
    private authenticationState = new ReplaySubject<User | null>(1);
    private accountCache$?: Observable<User | null>;

    /**
     * Setter & getter for user
     *
     * @param value
     */
    set user(value: User) {
        // Store the value
        this.authenticationState.next(value);
    }

    get user$(): Observable<User | null> {
        return this.authenticationState.asObservable();
    }


    authenticate(user: User | null): void {
        this.userIdentity = user;
        this.authenticationState.next(this.userIdentity);
    }

    hasAnyAuthority(authorities: string[] | string): boolean {
        return true;
    }

    identity(force?: boolean): Observable<User | null> {
        return of({})
    }

    isAuthenticated(): boolean {
        return true;
    }

    getAuthenticationState() {
        return this.authenticationState;
    }
}
