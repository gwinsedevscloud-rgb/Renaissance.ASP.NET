import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AppModule } from '../models/auth.models';
import { AuthStateService } from '../services/auth-state.service';

export const moduleGuard = (module: AppModule): CanActivateFn => () => {
    const auth = inject(AuthStateService);
    const router = inject(Router);

    if (auth.hasModule(module)) {
        return true;
    }

    return router.createUrlTree(['/access-denied']);
};
