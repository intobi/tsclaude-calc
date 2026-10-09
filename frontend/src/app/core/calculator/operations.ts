export type Operation = 'add' | 'subtract' | 'multiply' | 'divide';

/** Symbols shown on the keypad, in the expression line and in the history. */
export const OPERATION_SYMBOLS: Record<Operation, string> = {
  add: '+',
  subtract: '−',
  multiply: '×',
  divide: '÷',
};

/** Significant digits kept when showing numbers; hides binary floating-point noise. */
const DISPLAY_PRECISION = 12;

/** Formats a number for display, e.g. 0.1 + 0.2 shows as "0.3" instead of "0.30000000000000004". */
export function formatNumber(value: number): string {
  return String(Number(value.toPrecision(DISPLAY_PRECISION)));
}
