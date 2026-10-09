import { Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { HistoryItem, HistoryOperation } from './history.models';

const SYMBOLS: Record<HistoryOperation, string> = {
  add: '+',
  subtract: '−',
  multiply: '×',
  divide: '÷',
};

@Component({
  selector: 'app-history',
  imports: [DatePipe],
  styleUrl: './history.css',
  templateUrl: './history.html',
})
export class History {
  private readonly http = inject(HttpClient);

  /** Bump this value to reload the history (e.g. after a new calculation). */
  readonly refreshKey = input(0);

  protected readonly items = signal<HistoryItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  constructor() {
    effect(() => {
      this.refreshKey();
      this.load();
    });
  }

  private load() {
    this.loading.set(true);
    this.error.set('');
    this.http.get<HistoryItem[]>('/api/history').subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (e) => {
        this.error.set(e.error?.error ?? 'Не вдалося завантажити історію');
        this.loading.set(false);
      },
    });
  }

  protected symbol(operation: HistoryOperation): string {
    return SYMBOLS[operation] ?? operation;
  }
}
