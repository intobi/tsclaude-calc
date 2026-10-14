import {
  Component,
  DestroyRef,
  ElementRef,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import {
  OPERATION_SYMBOLS as SYMBOLS,
  Operation,
  formatNumber,
} from '../../core/calculator/operations';
import { APP_PATHS, injectCurrentPath, viewToggleFor } from '../../core/navigation';
import { History } from '../history/history';
import { CalculatorApi } from './calculator-api';

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

// Outlined keys (operations, backspace); `data-[pressed=true]` mirrors hover while a shortcut is held.
const OUTLINED_KEY =
  'border-outline bg-transparent text-outline hover:bg-outline-hover data-[pressed=true]:bg-outline-hover';

// Tailwind classes per key kind.
const KEY_KIND_CLASSES: Record<KeyKind, string> = {
  digit:
    'border-field-border bg-field-bg text-title hover:bg-field-hover data-[pressed=true]:bg-field-hover',
  op: OUTLINED_KEY,
  backspace: OUTLINED_KEY,
  clear: `${OUTLINED_KEY} text-base font-semibold`,
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
  imports: [History, RouterLink],
  templateUrl: './calculator.html',
  host: {
    class: 'flex w-full flex-col items-center gap-6',
    '(document:keydown)': 'onKeydown($event)',
  },
})
export class Calculator {
  private readonly calculatorApi = inject(CalculatorApi);
  private readonly path = injectCurrentPath();

  protected readonly display = signal('0');
  protected readonly expression = signal('');
  protected readonly error = signal('');
  protected readonly keys = KEYS;
  /** The history side is shown on the /history URL (selected from the header menu). */
  protected readonly historyOpen = computed(() => this.path() === APP_PATHS.history);
  /** Target of the switch above the card: the side that is currently hidden. */
  protected readonly viewToggle = computed(() => viewToggleFor(this.path()));
  /** A calculation request is in flight; further input waits for it (Clear cancels it). */
  protected readonly busy = signal(false);
  protected readonly pressedKey = signal<Key | null>(null);
  private readonly keypad = viewChild<ElementRef<HTMLElement>>('keypad');

  private first: number | null = null;
  private pending: Operation | null = null;
  private fresh = true;
  private pressTimer?: ReturnType<typeof setTimeout>;
  private request?: Subscription;

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      clearTimeout(this.pressTimer);
      this.request?.unsubscribe();
    });
  }

  protected onKey(key: Key) {
    if (this.busy() && key.kind !== 'clear') return;
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
    this.busy.set(true);
    this.request = this.calculatorApi.calculate(a, b, op).subscribe({
      next: (result) => {
        const shown = formatNumber(result);
        this.busy.set(false);
        this.display.set(shown);
        this.first = Number(shown);
        this.fresh = true;
        if (next) {
          this.pending = next;
          this.expression.set(`${shown} ${SYMBOLS[next]}`);
        } else {
          this.pending = null;
          this.expression.set(`${formatNumber(a)} ${SYMBOLS[op]} ${formatNumber(b)} =`);
        }
      },
      error: (e) => {
        this.clear();
        this.error.set(e.error?.error ?? 'Server error');
      },
    });
  }

  private clear() {
    this.request?.unsubscribe();
    this.busy.set(false);
    this.display.set('0');
    this.expression.set('');
    this.first = null;
    this.pending = null;
    this.fresh = true;
  }
}
