import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Calculator } from './calculator';

describe('Calculator', () => {
  let harness: RouterTestingHarness;
  let root: HTMLElement;
  let http: HttpTestingController;
  // Minimal stand-in for `fixture` so the helpers below read naturally.
  const fixture = { detectChanges: () => harness.detectChanges() };

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
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          {
            path: '',
            component: Calculator,
            children: [
              { path: '', children: [] },
              { path: 'history', children: [] },
            ],
          },
        ]),
      ],
    }).compileComponents();
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/', Calculator);
    root = harness.routeNativeElement!;
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

  it('flips to the history on /history and back on /', async () => {
    const calc = () => root.querySelector('section[aria-label="Калькулятор"]')!;
    const history = () => root.querySelector('app-history')!;
    expect(calc().hasAttribute('inert')).toBe(false);
    expect(history().hasAttribute('inert')).toBe(true);
    const instance = harness.routeDebugElement!.componentInstance;

    await harness.navigateByUrl('/history');
    // The same component instance stays on screen, which is what lets the card animate.
    expect(harness.routeDebugElement!.componentInstance).toBe(instance);
    expect(calc().hasAttribute('inert')).toBe(true);
    expect(history().hasAttribute('inert')).toBe(false);
    http.match('/api/history').forEach((req) => req.flush([]));

    await harness.navigateByUrl('/');
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

  it('ignores the keyboard while the history side is shown', async () => {
    await harness.navigateByUrl('/history');
    type('9');
    await harness.navigateByUrl('/');
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
    const outside = document.createElement('button');
    document.body.appendChild(outside);
    const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true });
    outside.dispatchEvent(event);
    outside.remove();
    expect(event.defaultPrevented).toBe(false);
  });

  it('does not load the history while it is hidden', () => {
    http.expectNone('/api/history');
  });

  it('sends only one request when = is pressed repeatedly', () => {
    type('2', '+', '3', 'Enter', 'Enter', '=');
    const requests = http.match('/api/calculate');
    expect(requests.length).toBe(1);
    expect(root.querySelector('section[aria-label="Калькулятор"]')!.getAttribute('aria-busy')).toBe(
      'true',
    );
    requests[0].flush({ result: 5 });
    fixture.detectChanges();
    expect(display()).toBe('5');
  });

  it('ignores typing while a calculation is in flight', () => {
    type('2', '+', '3', 'Enter', '9');
    http.expectOne('/api/calculate').flush({ result: 5 });
    fixture.detectChanges();
    expect(display()).toBe('5');
  });

  it('cancels the request in flight with Clear', () => {
    type('2', '+', '3', 'Enter');
    const req = http.expectOne('/api/calculate');
    type('Escape');
    expect(req.cancelled).toBe(true);
    expect(display()).toBe('0');
  });

  it('shows results without floating-point noise', () => {
    type('0', '.', '1', '+', '0', '.', '2', 'Enter');
    http.expectOne('/api/calculate').flush({ result: 0.1 + 0.2 });
    fixture.detectChanges();
    expect(display()).toBe('0.3');
  });
});
