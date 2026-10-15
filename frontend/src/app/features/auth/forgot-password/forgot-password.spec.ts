import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { ForgotPassword } from './forgot-password';

describe('ForgotPassword', () => {
  let harness: RouterTestingHarness;
  let root: HTMLElement;
  let http: HttpTestingController;

  const emailInput = () => root.querySelector<HTMLInputElement>('input[type="email"]')!;
  const type = (value: string) => {
    emailInput().value = value;
    emailInput().dispatchEvent(new Event('input'));
    harness.detectChanges();
  };
  const submit = () => {
    root.querySelector('form')!.dispatchEvent(new Event('submit'));
    harness.detectChanges();
  };
  const button = (name: string) =>
    Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find(
      (b) => b.textContent!.trim() === name,
    )!;
  const link = (name: string) =>
    Array.from(root.querySelectorAll<HTMLAnchorElement>('a')).find(
      (a) => a.textContent!.trim() === name,
    )!;

  async function open(url = '/forgot-password') {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'forgot-password', component: ForgotPassword },
          { path: '**', children: [] },
        ]),
      ],
    });
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    root = harness.routeNativeElement!;
    http = TestBed.inject(HttpTestingController);
  }

  afterEach(() => http.verify());

  it('prefills the email from the sign-in page', async () => {
    await open('/forgot-password?email=andrew%40example.com');

    expect(emailInput().value).toBe('andrew@example.com');
  });

  it('validates the email before calling the API', async () => {
    await open();
    submit();
    expect(root.textContent).toContain('Enter your email.');

    type('not-an-email');
    submit();
    expect(root.textContent).toContain('Enter a valid email address.');
    http.expectNone('/api/auth/forgot-password');
  });

  it('shows a copyable reset link for a known account', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    await open();
    type(' andrew@example.com ');
    submit();

    const req = http.expectOne({ method: 'POST', url: '/api/auth/forgot-password' });
    expect(req.request.body).toEqual({ email: 'andrew@example.com' });
    req.flush({ resetToken: 'tok/en+1', expiresAtUtc: '2026-10-15T10:00:00Z' });
    harness.detectChanges();

    const linkInput = root.querySelector<HTMLInputElement>('input[readonly]')!;
    const url = new URL(linkInput.value);
    expect(url.pathname).toBe('/reset-password');
    expect(url.searchParams.get('email')).toBe('andrew@example.com');
    expect(url.searchParams.get('token')).toBe('tok/en+1');
    expect(root.textContent).toContain('does not send real emails');

    button('Copy link').click();
    await Promise.resolve();
    harness.detectChanges();
    expect(writeText).toHaveBeenCalledWith(linkInput.value);
    expect(button('Copied!')).toBeTruthy();
    expect(root.querySelector('[role="status"]')!.textContent).toContain('in your clipboard');

    const openLink = new URL(link('Open link').href, 'http://localhost');
    expect(openLink.pathname).toBe('/reset-password');
    expect(Object.fromEntries(openLink.searchParams)).toEqual({
      email: 'andrew@example.com',
      token: 'tok/en+1',
    });
  });

  it('selects the link for manual copying when the clipboard is blocked', async () => {
    const writeText = vi.fn().mockRejectedValue(new Error('denied'));
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    await open();
    type('andrew@example.com');
    submit();
    http
      .expectOne('/api/auth/forgot-password')
      .flush({ resetToken: 'token', expiresAtUtc: '2026-10-15T10:00:00Z' });
    harness.detectChanges();

    button('Copy link').click();
    await new Promise((resolve) => setTimeout(resolve));
    harness.detectChanges();

    expect(root.querySelector('[role="status"]')!.textContent).toContain('press Ctrl+C');
  });

  it('gives a neutral answer when no account uses the email', async () => {
    await open();
    type('nobody@example.com');
    submit();
    http.expectOne('/api/auth/forgot-password').flush({ resetToken: null, expiresAtUtc: null });
    harness.detectChanges();

    expect(root.textContent).toContain('If an account exists for');
    expect(root.querySelector('input[readonly]')).toBeNull();

    button('Use a different email').click();
    harness.detectChanges();
    expect(emailInput().value).toBe('nobody@example.com');
  });

  it('shows the API error, e.g. when rate limited', async () => {
    await open();
    type('andrew@example.com');
    submit();
    http
      .expectOne('/api/auth/forgot-password')
      .flush(
        { error: 'Too many attempts. Try again in a minute.' },
        { status: 429, statusText: 'Too Many Requests' },
      );
    harness.detectChanges();

    expect(root.textContent).toContain('Too many attempts.');
  });
});
