import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LanAccessStateService {
    token: string | null = null;
    expiresAtUtc: string | null = null;

    get isUnlocked(): boolean {
        return !!this.token && !!this.expiresAtUtc && new Date(this.expiresAtUtc) > new Date();
    }

    applyUnlock(token: string, expiresAtUtc: string): void {
        this.token = token;
        this.expiresAtUtc = expiresAtUtc;
    }

    clear(): void {
        this.token = null;
        this.expiresAtUtc = null;
    }
}
