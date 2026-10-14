export interface KeyboardShortcut {
  keys: string[];
  action: string;
}

/** Human-readable cheat sheet of the calculator's keyboard shortcuts (shown in the header menu). */
export const KEYBOARD_SHORTCUTS: KeyboardShortcut[] = [
  { keys: ['0–9'], action: 'Digits' },
  { keys: ['.', ','], action: 'Decimal point' },
  { keys: ['+', '-', '*', '/'], action: 'Operations' },
  { keys: ['Enter', '='], action: 'Equals' },
  { keys: ['Backspace'], action: 'Delete last character' },
  { keys: ['Esc', 'Delete', 'C'], action: 'Clear' },
];
