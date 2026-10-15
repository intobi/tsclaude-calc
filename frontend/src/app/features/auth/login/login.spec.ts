import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Login } from './login';

describe('Login', () => {
  let harness: RouterTestingHarness;
  let root: HTMLElement;

  const forgotLink = () =>
    Array.from(root.querySelectorAll('a')).find(
      (a) => a.textContent!.trim() === 'Forgot password?',
    )!;

  const query = (a: HTMLAnchorElement) => new URL(a.href, 'http://localhost').searchParams;

  async function open(url: string) {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'login', component: Login },
          { path: '**', children: [] },
        ]),
      ],
    });
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    // ngModel writes prefilled values asynchronously.
    await harness.fixture.whenStable();
    harness.detectChanges();
    root = harness.routeNativeElement!;
  }

  it('links to the forgot password page with the typed email', async () => {
    await open('/login');
    expect(forgotLink().getAttribute('href')).toBe('/forgot-password');

    const email = root.querySelector<HTMLInputElement>('input[type="email"]')!;
    email.value = 'andrew@example.com';
    email.dispatchEvent(new Event('input'));
    harness.detectChanges();

    expect(query(forgotLink()).get('email')).toBe('andrew@example.com');
  });

  it('confirms a password change and prefills the email', async () => {
    await open('/login?email=andrew%40example.com&passwordChanged=true');

    expect(root.querySelector('[role="status"]')!.textContent).toContain(
      'Your password has been changed.',
    );
    expect(root.querySelector<HTMLInputElement>('input[type="email"]')!.value).toBe(
      'andrew@example.com',
    );
  });

  it('shows no confirmation on a normal visit', async () => {
    await open('/login');

    expect(root.querySelector('[role="status"]')).toBeNull();
  });
});
