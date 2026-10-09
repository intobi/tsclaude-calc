import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Calculator } from './calculator';

describe('Calculator', () => {
  let fixture: ComponentFixture<Calculator>;
  let root: HTMLElement;
  let http: HttpTestingController;

  const click = (label: string) => {
    const button = Array.from(root.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === label,
    );
    button!.click();
    fixture.detectChanges();
  };
  const type = (...keys: string[]) => {
    for (const key of keys) {
      document.dispatchEvent(new KeyboardEvent('keydown', { key, cancelable: true }));
    }
    fixture.detectChanges();
  };
  const display = () => root.querySelector('output')!.textContent!.trim();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Calculator],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(Calculator);
    fixture.detectChanges();
    root = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
  });

  it('renders all keypad buttons', () => {
    expect(root.querySelectorAll('button[aria-keyshortcuts]').length).toBe(18);
  });

  it('shows typed digits', () => {
    click('1');
    click('2');
    expect(display()).toBe('12');
  });

  it('sends the calculation to the API and shows the result', () => {
    click('6');
    click('×');
    click('7');
    click('=');
    const req = http.expectOne('/api/calculate');
    expect(req.request.body).toEqual({ a: 6, b: 7, operation: 'multiply' });
    req.flush({ result: 42 });
    fixture.detectChanges();
    expect(display()).toBe('42');
  });

  it('flips to the history and back with the toggle button', () => {
    const calc = () => root.querySelector('section[aria-label="Калькулятор"]')!;
    const history = () => root.querySelector('app-history')!;
    expect(calc().hasAttribute('inert')).toBe(false);
    expect(history().hasAttribute('inert')).toBe(true);

    click('History');
    expect(calc().hasAttribute('inert')).toBe(true);
    expect(history().hasAttribute('inert')).toBe(false);
    http.match('/api/history').forEach((req) => req.flush([]));

    click('Калькулятор');
    expect(calc().hasAttribute('inert')).toBe(false);
    expect(history().hasAttribute('inert')).toBe(true);
  });

  it('calculates from the physical keyboard', () => {
    type('1', '2', '+', '3', 'Enter');
    const req = http.expectOne('/api/calculate');
    expect(req.request.body).toEqual({ a: 12, b: 3, operation: 'add' });
    req.flush({ result: 15 });
    fixture.detectChanges();
    expect(display()).toBe('15');
  });

  it('supports comma as decimal point, Backspace and Escape', () => {
    type('4', ',', '5', '6');
    expect(display()).toBe('4.56');
    type('Backspace');
    expect(display()).toBe('4.5');
    type('Escape');
    expect(display()).toBe('0');
  });

  it('highlights the keypad button pressed on the keyboard', () => {
    type('7');
    const pressed = root.querySelector('[data-pressed="true"]');
    expect(pressed?.textContent?.trim()).toBe('7');
  });

  it('ignores the keyboard while the history side is shown', () => {
    click('History');
    type('9');
    click('Калькулятор');
    expect(display()).toBe('0');
  });

  it('ignores shortcuts combined with Ctrl or Cmd', () => {
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'c', ctrlKey: true }));
    document.dispatchEvent(new KeyboardEvent('keydown', { key: '5', metaKey: true }));
    fixture.detectChanges();
    expect(display()).toBe('0');
  });

  it('deletes the last typed character with the ⌫ button', () => {
    click('1');
    click('2');
    click('3');
    root.querySelector<HTMLButtonElement>('button[aria-label="Backspace"]')!.click();
    fixture.detectChanges();
    expect(display()).toBe('12');
  });

  it('resets to 0 when the last character is deleted', () => {
    click('7');
    root.querySelector<HTMLButtonElement>('button[aria-label="Backspace"]')!.click();
    fixture.detectChanges();
    expect(display()).toBe('0');
  });

  it('does not edit a calculated result with ⌫', () => {
    type('2', '+', '3', 'Enter');
    http.expectOne('/api/calculate').flush({ result: 5 });
    fixture.detectChanges();
    type('Backspace');
    expect(display()).toBe('5');
  });

  it('presses equals on Enter even when a keypad button has focus', () => {
    click('1');
    click('+');
    click('2');
    const backspace = root.querySelector<HTMLButtonElement>('button[aria-label="Backspace"]')!;
    backspace.focus();
    backspace.dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true }),
    );
    fixture.detectChanges();
    expect(http.expectOne('/api/calculate').request.body).toEqual({ a: 1, b: 2, operation: 'add' });
  });

  it('keeps Enter for buttons outside the keypad', () => {
    const logout = Array.from(root.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === 'Вийти',
    )!;
    const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true });
    logout.dispatchEvent(event);
    expect(event.defaultPrevented).toBe(false);
  });
});
