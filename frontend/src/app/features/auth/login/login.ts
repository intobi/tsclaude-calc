import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  styleUrl: './login.css',
  templateUrl: './login.html',
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected emailInput = '';
  protected passwordInput = '';
  protected readonly error = signal('');
  protected readonly loading = signal(false);

  protected submit() {
    this.error.set('');
    this.loading.set(true);
    this.authService.login({ email: this.emailInput, password: this.passwordInput }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/']);
      },
      error: (e) => {
        this.loading.set(false);
        this.error.set(e.error?.error ?? 'Не вдалося увійти');
      },
    });
  }
}
