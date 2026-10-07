import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

type Operation = 'add' | 'subtract' | 'multiply' | 'divide';

const SYMBOLS: Record<Operation, string> = { add: '+', subtract: '−', multiply: '×', divide: '÷' };

interface Key {
  label: string;
  kind: 'digit' | 'op' | 'eq' | 'clear';
  ariaLabel?: string;
  op?: Operation;
}

const digit = (label: string): Key => ({ label, kind: 'digit' });
const op = (o: Operation, ariaLabel: string): Key => ({
  label: SYMBOLS[o],
  kind: 'op',
  op: o,
  ariaLabel,
});

// Row-major order of the 4-column keypad.
const KEYS: Key[] = [
  { label: 'C', kind: 'clear', ariaLabel: 'Clear' },
  op('divide', 'Divide'),
  digit('7'),
  digit('8'),
  digit('9'),
  op('multiply', 'Multiply'),
  digit('4'),
  digit('5'),
  digit('6'),
  op('subtract', 'Subtract'),
  digit('1'),
  digit('2'),
  digit('3'),
  op('add', 'Add'),
  digit('0'),
  digit('.'),
  { label: '=', kind: 'eq', ariaLabel: 'Equals' },
];

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
  protected readonly keys = KEYS;

  private first: number | null = null;
  private pending: Operation | null = null;
  private fresh = true;

  protected onKey(key: Key) {
    switch (key.kind) {
      case 'digit':
        return this.press(key.label);
      case 'op':
        return this.setOp(key.op!);
      case 'eq':
        return this.equals();
      case 'clear':
        return this.clear();
    }
  }

  private press(d: string) {
    this.error.set('');
    if (this.fresh) {
      this.display.set(d === '.' ? '0.' : d);
      this.fresh = false;
    } else if (d !== '.' || !this.display().includes('.')) {
      this.display.update((v) => (v === '0' && d !== '.' ? d : v + d));
    }
  }

  private setOp(op: Operation) {
    if (this.pending && !this.fresh) {
      this.equals(op);
      return;
    }
    this.first = Number(this.display());
    this.pending = op;
    this.fresh = true;
    this.expression.set(`${this.display()} ${SYMBOLS[op]}`);
  }

  private equals(next?: Operation) {
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

  private clear() {
    this.display.set('0');
    this.expression.set('');
    this.first = null;
    this.pending = null;
    this.fresh = true;
  }
}
