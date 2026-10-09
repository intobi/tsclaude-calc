export interface KeyboardShortcut {
  keys: string[];
  action: string;
}

/** Human-readable cheat sheet of the calculator's keyboard shortcuts (shown in the header menu). */
export const KEYBOARD_SHORTCUTS: KeyboardShortcut[] = [
  { keys: ['0–9'], action: 'Цифри' },
  { keys: ['.', ','], action: 'Десяткова крапка' },
  { keys: ['+', '-', '*', '/'], action: 'Операції' },
  { keys: ['Enter', '='], action: 'Дорівнює' },
  { keys: ['Backspace'], action: 'Стерти символ' },
  { keys: ['Esc', 'Delete', 'C'], action: 'Очистити' },
];
