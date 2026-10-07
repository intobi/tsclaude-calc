import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

type Operation = 'add' | 'subtract' | 'multiply' | 'divide';

const SYMBOLS: Record<Operation, string> = { add: '+', subtract: '−', multiply: '×', divide: '÷' };

@Component({
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  private readonly http = inject(HttpClient);

  protected readonly display = signal('0');
  protected readonly expression = signal('');
  protected readonly error = signal('');
  protected readonly digits = ['7', '8', '9', '4', '5', '6', '1', '2', '3'];
  protected readonly ops: { op: Operation; symbol: string }[] = (
    Object.keys(SYMBOLS) as Operation[]
  ).map((op) => ({ op, symbol: SYMBOLS[op] }));

  private first: number | null = null;
  private pending: Operation | null = null;
  private fresh = true;

  protected press(d: string) {
    this.error.set('');
    if (this.fresh) {
      this.display.set(d === '.' ? '0.' : d);
      this.fresh = false;
    } else if (d !== '.' || !this.display().includes('.')) {
      this.display.update((v) => (v === '0' && d !== '.' ? d : v + d));
    }
  }

  protected setOp(op: Operation) {
    if (this.pending && !this.fresh) {
      this.equals(op);
      return;
    }
    this.first = Number(this.display());
    this.pending = op;
    this.fresh = true;
    this.expression.set(`${this.display()} ${SYMBOLS[op]}`);
  }

  protected equals(next?: Operation) {
    if (this.pending === null || this.first === null) return;
    const op = this.pending;
    const a = this.first;
    const b = Number(this.display());
    this.http.post<{ result: number }>('/api/calculate', { a, b, operation: op }).subscribe({
      next: ({ result }) => {
        this.display.set(String(result));
        this.first = result;
        this.fresh = true;
        if (next) {
          this.pending = next;
          this.expression.set(`${result} ${SYMBOLS[next]}`);
        } else {
          this.pending = null;
          this.expression.set(`${a} ${SYMBOLS[op]} ${b} =`);
        }
      },
      error: (e) => {
        this.error.set(e.error?.error ?? 'Server error');
        this.clear();
      },
    });
  }

  protected clear() {
    this.display.set('0');
    this.expression.set('');
    this.first = null;
    this.pending = null;
    this.fresh = true;
  }
}
