import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
    private readonly snackBar = inject(MatSnackBar);

    showSuccess(message: string, durationMs = 4000): void {
        this.snackBar.open(message, 'Close', {
            duration: durationMs,
            panelClass: ['snackbar-success']
        });
    }

    showError(message: string, durationMs = 6000): void {
        this.snackBar.open(message, 'Close', {
            duration: durationMs,
            panelClass: ['snackbar-error']
        });
    }
}
