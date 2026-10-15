import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginRequest } from '../../../core/auth/auth.models';
import { APP_PATHS } from '../../../core/navigation';
import { PASSWORD_CHANGED_PARAM } from '../password-reset/password-reset.models';

interface LoginField {
  name: keyof LoginRequest;
  label: string;
  type: 'email' | 'password';
  autocomplete: string;
}

const FIELDS: LoginField[] = [
  { name: 'email', label: 'Email', type: 'email', autocomplete: 'username' },
  { name: 'password', label: 'Password', type: 'password', autocomplete: 'current-password' },
];

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  private readonly query = inject(ActivatedRoute).snapshot.queryParamMap;

  protected readonly fields = FIELDS;
  protected readonly forgotPasswordPath = APP_PATHS.forgotPassword;
  /** Prefilled when coming back from the password reset pages. */
  protected readonly credentials: LoginRequest = {
    email: this.query.get('email') ?? '',
    password: '',
  };
  /** Shown after a password reset, until the next sign-in attempt. */
  protected readonly passwordChanged = signal(this.query.has(PASSWORD_CHANGED_PARAM));
  protected readonly error = signal('');
  protected readonly loading = signal(false);

  protected submit() {
    this.error.set('');
    this.passwordChanged.set(false);
    this.loading.set(true);
    this.authService.login({ ...this.credentials }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/']);
      },
      error: (e) => {
        this.loading.set(false);
        this.error.set(e.error?.error ?? 'Could not sign in');
      },
    });
  }
}
