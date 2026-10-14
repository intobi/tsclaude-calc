import { Component, effect, inject, input, signal, untracked } from '@angular/core';
import { DatePipe } from '@angular/common';
import { OPERATION_SYMBOLS, Operation, formatNumber } from '../../core/calculator/operations';
import { HistoryApi } from './history-api';
import { HistoryItem } from './history.models';

@Component({
  selector: 'app-history',
  imports: [DatePipe],
  templateUrl: './history.html',
  host: { class: 'block' },
})
export class History {
  private readonly historyApi = inject(HistoryApi);

  /** The history is fetched each time it becomes visible, so it is never stale and never loaded while hidden. */
  readonly active = input(false);

  protected readonly items = signal<HistoryItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly format = formatNumber;

  constructor() {
    effect(() => {
      if (this.active()) {
        untracked(() => this.load());
      }
    });
  }

  protected symbol(operation: Operation): string {
    return OPERATION_SYMBOLS[operation] ?? operation;
  }

  private load() {
    this.loading.set(true);
    this.error.set('');
    this.historyApi.getLatest().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (e) => {
        this.error.set(e.error?.error ?? 'Could not load history');
        this.loading.set(false);
      },
    });
  }
}
