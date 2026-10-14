import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginRequest } from '../../../core/auth/auth.models';

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
  imports: [FormsModule],
  templateUrl: './login.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly fields = FIELDS;
  protected readonly credentials: LoginRequest = { email: '', password: '' };
  protected readonly error = signal('');
  protected readonly loading = signal(false);

  protected submit() {
    this.error.set('');
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
