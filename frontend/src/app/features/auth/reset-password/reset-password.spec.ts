import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AuthService } from '../../../core/auth/auth.service';
import { ResetPassword } from './reset-password';

const LINK = '/reset-password?email=andrew%40example.com&token=tok%2Fen';

describe('ResetPassword', () => {
  let harness: RouterTestingHarness;
  let root: HTMLElement;
  let http: HttpTestingController;

  const field = (label: string) =>
    Array.from(root.querySelectorAll('label'))
      .find((l) => l.textContent!.trim().startsWith(label))!
      .querySelector('input')!;
  const type = (label: string, value: string) => {
    const input = field(label);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    harness.detectChanges();
  };
  const submit = () => {
    root.querySelector('form')!.dispatchEvent(new Event('submit'));
    harness.detectChanges();
  };

  async function open(url: string, validation: 'ok' | 'invalid' | 'none' = 'ok') {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'reset-password', component: ResetPassword },
          { path: '**', children: [] },
        ]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    root = harness.routeNativeElement!;
    if (validation !== 'none') {
      const req = http.expectOne({ method: 'POST', url: '/api/auth/reset-password/validate' });
      expect(req.request.body).toEqual({ email: 'andrew@example.com', token: 'tok/en' });
      if (validation === 'ok') {
        req.flush(null, { status: 204, statusText: 'No Content' });
      } else {
        req.flush(
          { error: 'This reset link is invalid or has expired. Request a new one.' },
          { status: 400, statusText: 'Bad Request' },
        );
      }
      harness.detectChanges();
    }
  }

  afterEach(() => http.verify());

  it('rejects an incomplete link without calling the API', async () => {
    await open('/reset-password?email=andrew%40example.com', 'none');

    expect(root.textContent).toContain('This reset link is incomplete.');
    expect(root.querySelector('form')).toBeNull();
  });

  it('explains an invalid or expired link and offers a new one', async () => {
    await open(LINK, 'invalid');

    expect(root.querySelector('[role="alert"]')!.textContent).toContain('invalid or has expired');
    const requestNew = Array.from(root.querySelectorAll('a')).find(
      (a) => a.textContent!.trim() === 'Request a new link',
    )!;
    const target = new URL(requestNew.href, 'http://localhost');
    expect([target.pathname, target.searchParams.get('email')]).toEqual([
      '/forgot-password',
      'andrew@example.com',
    ]);
  });

  it('shows the account email and validates the new password', async () => {
    await open(LINK);
    expect(field('Email').value).toBe('andrew@example.com');

    submit();
    expect(root.textContent).toContain('Enter a new password.');
    expect(root.textContent).toContain('Repeat the new password.');

    type('New password', 'abc');
    expect(root.textContent).toContain('at least 4 characters.');

    type('New password', 'new-pass');
    type('Confirm new password', 'new-pasx');
    submit();
    expect(root.textContent).toContain('The passwords do not match.');
    http.expectNone('/api/auth/reset-password');
  });

  it('changes the password, signs out and returns to sign in', async () => {
    await open(LINK);
    const logout = vi.spyOn(TestBed.inject(AuthService), 'logout');
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

    type('New password', 'new-pass');
    type('Confirm new password', 'new-pass');
    submit();
    const req = http.expectOne({ method: 'POST', url: '/api/auth/reset-password' });
    expect(req.request.body).toEqual({
      email: 'andrew@example.com',
      token: 'tok/en',
      newPassword: 'new-pass',
    });
    req.flush(null, { status: 204, statusText: 'No Content' });

    expect(logout).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { email: 'andrew@example.com', passwordChanged: true },
    });
  });

  it('shows why the API rejected the new password', async () => {
    await open(LINK);
    type('New password', '1111');
    type('Confirm new password', '1111');
    submit();
    http
      .expectOne('/api/auth/reset-password')
      .flush(
        { error: 'The new password must be different from the current one.' },
        { status: 400, statusText: 'Bad Request' },
      );
    harness.detectChanges();

    expect(root.textContent).toContain('must be different from the current one');
  });
});
