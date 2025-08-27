// data-sync.service.ts
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DataSyncService {
    private refreshListSubject = new Subject<void>();
    refreshList$ = this.refreshListSubject.asObservable();

    triggerRefresh() {
        this.refreshListSubject.next();
    }
}
