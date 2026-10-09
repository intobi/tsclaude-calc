import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { Header } from './header';

const SESSION_KEY = 'calc_auth';

function signIn(email = 'user@example.com') {
  const expiresAtUtc = new Date(Date.now() + 60 * 60 * 1000).toISOString();
  localStorage.setItem(SESSION_KEY, JSON.stringify({ token: 'test-token', email, expiresAtUtc }));
}

describe('Header', () => {
  let root: HTMLElement;
  let detectChanges: () => void;

  const button = (label: string) =>
    Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find(
      (b) => b.textContent?.trim() === label,
    )!;

  async function render() {
    await TestBed.configureTestingModule({
      imports: [Header],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    const fixture = TestBed.createComponent(Header);
    fixture.detectChanges();
    root = fixture.nativeElement;
    detectChanges = () => fixture.detectChanges();
  }

  afterEach(() => localStorage.removeItem(SESSION_KEY));

  it('shows only the sign-in link when signed out', async () => {
    await render();
    expect(root.querySelector('nav')).toBeNull();
    expect(root.querySelector('a[href="/login"]')!.textContent!.trim()).toBe('Увійти');
  });

  describe('when signed in', () => {
    beforeEach(async () => {
      signIn('user@example.com');
      await render();
    });

    it('links to the calculator and the history', () => {
      const links = Array.from(root.querySelectorAll<HTMLAnchorElement>('nav a')).map((a) => [
        a.textContent!.trim(),
        a.getAttribute('href'),
      ]);
      expect(links).toEqual([
        ['Калькулятор', '/'],
        ['Історія', '/history'],
      ]);
    });

    it('opens the keyboard shortcuts cheat sheet and closes it with Escape', () => {
      const sheet = root.querySelector<HTMLElement>('#submenu-shortcuts')!;
      expect(sheet.hidden).toBe(true);

      button('Гарячі клавіші').click();
      detectChanges();
      expect(sheet.hidden).toBe(false);
      expect(sheet.textContent).toContain('Backspace');
      expect(sheet.textContent).toContain('Стерти символ');

      document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
      detectChanges();
      expect(sheet.hidden).toBe(true);
    });

    it('shows the account email and signs out from its menu', () => {
      const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);

      button('user@example.com').click();
      detectChanges();
      expect(root.querySelector<HTMLElement>('#submenu-account')!.hidden).toBe(false);

      const logout = root.querySelector<HTMLButtonElement>('#submenu-account button')!;
      logout.click();
      detectChanges();

      expect(localStorage.getItem(SESSION_KEY)).toBeNull();
      expect(navigate).toHaveBeenCalledWith('/login');
      expect(root.querySelector('nav')).toBeNull();
    });

    it('toggles the mobile menu with the burger button', () => {
      const burger = root.querySelector<HTMLButtonElement>('button[aria-label="Меню"]')!;
      burger.click();
      detectChanges();
      expect(root.querySelector('nav')!.getAttribute('data-open')).toBe('true');
      expect(burger.getAttribute('aria-expanded')).toBe('true');
    });
  });
});
