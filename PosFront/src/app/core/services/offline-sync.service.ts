import { Injectable } from '@angular/core';

/** Extension point only. No offline queue, persistence, or sync is implemented. */
@Injectable({ providedIn: 'root' })
export class OfflineSyncService {
  readonly isImplemented = false;
}
