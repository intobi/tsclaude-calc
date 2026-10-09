import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HistoryItem } from './history.models';

@Injectable({ providedIn: 'root' })
export class HistoryApi {
  private readonly http = inject(HttpClient);

  /** The signed-in user's latest calculations, newest first. */
  getLatest(): Observable<HistoryItem[]> {
    return this.http.get<HistoryItem[]>('/api/history');
  }
}
