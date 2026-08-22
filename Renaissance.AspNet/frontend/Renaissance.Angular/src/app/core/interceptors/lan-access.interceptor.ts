import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LanAccessStateService } from '../services/lan-access-state.service';

export const lanAccessInterceptor: HttpInterceptorFn = (req, next) => {
    const lan = inject(LanAccessStateService);
    const path = req.url;

    if (lan.isUnlocked && lan.token && (path.includes('/settings/deployment') || path.includes('/settings/lan'))) {
        req = req.clone({
            setHeaders: { 'X-Lan-Access': lan.token }
        });
    }

    return next(req);
};
