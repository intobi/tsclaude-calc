import {
  Component,
  DestroyRef,
  ElementRef,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { History } from '../history/history';

type Operation = 'add' | 'subtract' | 'multiply' | 'divide';

const SYMBOLS: Record<Operation, string> = { add: '+', subtract: '−', multiply: '×', divide: '÷' };

type KeyKind = 'digit' | 'op' | 'eq' | 'clear' | 'backspace';

interface Key {
  label: string;
  kind: KeyKind;
  ariaLabel?: string;
  op?: Operation;
  /** Physical keyboard keys (KeyboardEvent.key values) that press this button. */
  shortcuts: string[];
  /** Spans two keypad columns. */
  wide?: boolean;
}

// Tailwind classes per key kind; `data-[pressed=true]` mirrors hover while a keyboard shortcut is held.
const KEY_KIND_CLASSES: Record<KeyKind, string> = {
  digit:
    'border-field-border bg-field-bg text-title hover:bg-field-hover data-[pressed=true]:bg-field-hover',
  op: 'border-outline bg-transparent text-outline hover:bg-outline-hover data-[pressed=true]:bg-outline-hover',
  backspace:
    'border-outline bg-transparent text-outline hover:bg-outline-hover data-[pressed=true]:bg-outline-hover',
  clear:
    'border-outline bg-transparent text-base font-semibold text-outline hover:bg-outline-hover data-[pressed=true]:bg-outline-hover',
  eq: 'border-primary bg-primary text-on-primary hover:border-primary-hover hover:bg-primary-hover data-[pressed=true]:border-primary-hover data-[pressed=true]:bg-primary-hover',
};

const SHORTCUTS: Record<Operation, string> = {
  add: '+',
  subtract: '-',
  multiply: '*',
  divide: '/',
};

const digit = (label: string): Key => ({
  label,
  kind: 'digit',
  shortcuts: label === '.' ? ['.', ','] : [label],
});
const op = (o: Operation, ariaLabel: string): Key => ({
  label: SYMBOLS[o],
  kind: 'op',
  op: o,
  ariaLabel,
  shortcuts: [SHORTCUTS[o]],
});

// Row-major order of the 4-column keypad.
const KEYS: Key[] = [
  {
    label: 'C',
    kind: 'clear',
    ariaLabel: 'Clear',
    shortcuts: ['Escape', 'Delete', 'c', 'C'],
    wide: true,
  },
  { label: '⌫', kind: 'backspace', ariaLabel: 'Backspace', shortcuts: ['Backspace'] },
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
  { ...digit('0'), wide: true },
  digit('.'),
  { label: '=', kind: 'eq', ariaLabel: 'Equals', shortcuts: ['Enter', '='] },
];

const KEY_BY_SHORTCUT = new Map(KEYS.flatMap((key) => key.shortcuts.map((s) => [s, key] as const)));

/** How long a keypad button stays highlighted after its keyboard shortcut is pressed. */
const PRESS_FEEDBACK_MS = 150;

const isEditable = (target: EventTarget | null): boolean =>
  target instanceof HTMLElement &&
  (target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName));

@Component({
  selector: 'app-calculator',
  imports: [History],
  templateUrl: './calculator.html',
  host: {
    class: 'flex w-full flex-col items-center gap-6',
    '(document:keydown)': 'onKeydown($event)',
  },
})
export class Calculator {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly display = signal('0');
  protected readonly expression = signal('');
  protected readonly error = signal('');
  protected readonly keys = KEYS;
  protected readonly email = this.authService.email;
  protected readonly historyOpen = signal(false);
  protected readonly historyVersion = signal(0);
  protected readonly pressedKey = signal<Key | null>(null);
  private readonly keypad = viewChild<ElementRef<HTMLElement>>('keypad');
  protected readonly barActions = computed(() => [
    {
      label: this.historyOpen() ? 'Калькулятор' : 'History',
      controls: 'calc-flip',
      run: () => this.toggleHistory(),
    },
    { label: 'Вийти', controls: undefined, run: () => this.logout() },
  ]);

  private first: number | null = null;
  private pending: Operation | null = null;
  private fresh = true;
  private pressTimer?: ReturnType<typeof setTimeout>;

  constructor() {
    inject(DestroyRef).onDestroy(() => clearTimeout(this.pressTimer));
  }

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
      case 'backspace':
        return this.backspace();
    }
  }

  protected keyClasses(key: Key): string {
    return `${KEY_KIND_CLASSES[key.kind]} ${key.wide ? 'col-span-2 aspect-auto' : 'aspect-square'}`;
  }

  protected onKeydown(event: KeyboardEvent) {
    if (this.historyOpen() || event.defaultPrevented || isEditable(event.target)) return;
    if (event.ctrlKey || event.metaKey || event.altKey) return;

    // Enter on a focused link or button outside the keypad keeps its usual meaning.
    const target = event.target;
    const focusedOutsideKeypad =
      target instanceof Element &&
      target !== document.body &&
      !this.keypad()?.nativeElement.contains(target);
    if (event.key === 'Enter' && focusedOutsideKeypad) return;

    const key = KEY_BY_SHORTCUT.get(event.key);
    if (!key) return;
    // Stops a focused keypad button from also being clicked by Enter, '/' from opening quick find
    // and Backspace from navigating back in some browsers.
    event.preventDefault();
    this.flash(key);
    this.onKey(key);
  }

  protected toggleHistory() {
    this.historyOpen.update((open) => !open);
    if (this.historyOpen()) {
      // Reload so the back side shows calculations made since the last flip.
      this.historyVersion.update((v) => v + 1);
    }
  }

  protected logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
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

  private backspace() {
    if (this.fresh) return;
    this.error.set('');
    const next = this.display().slice(0, -1);
    this.display.set(next === '' || next === '-' ? '0' : next);
  }

  private flash(key: Key) {
    clearTimeout(this.pressTimer);
    this.pressedKey.set(key);
    this.pressTimer = setTimeout(() => this.pressedKey.set(null), PRESS_FEEDBACK_MS);
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
