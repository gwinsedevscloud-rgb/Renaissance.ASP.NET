import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthStateService } from '../services/auth-state.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthStateService);
    if (auth.token) {
        req = req.clone({
            setHeaders: { Authorization: `Bearer ${auth.token}` }
        });
    }

    return next(req);
};
