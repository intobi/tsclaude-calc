import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { fieldsMatch } from '../../../core/forms/fields-match.validator';
import { apiErrorMessage } from '../../../core/http/api-error';
import { APP_PATHS } from '../../../core/navigation';
import { PasswordResetApi } from '../password-reset/password-reset-api';
import {
  PASSWORD_CHANGED_PARAM,
  PASSWORD_MAX_LENGTH,
  PASSWORD_MIN_LENGTH,
  ResetLinkParams,
} from '../password-reset/password-reset.models';

/**
 * - `checking`: asking the API whether the link can still be used
 * - `ready`: the new password form is shown
 * - `invalid`: the link is incomplete, wrong, already used or expired
 */
type LinkState = 'checking' | 'ready' | 'invalid';

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class ResetPassword {
  private readonly api = inject(PasswordResetApi);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly paths = APP_PATHS;
  protected readonly minLength = PASSWORD_MIN_LENGTH;
  protected readonly maxLength = PASSWORD_MAX_LENGTH;
  /** Email and token from the link; `null` when either is missing. */
  protected readonly link: ResetLinkParams | null;
  protected readonly state = signal<LinkState>('checking');
  protected readonly linkError = signal('');
  protected readonly formError = signal('');
  protected readonly saving = signal(false);
  protected readonly form = new FormGroup(
    {
      newPassword: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(PASSWORD_MIN_LENGTH),
          Validators.maxLength(PASSWORD_MAX_LENGTH),
        ],
      }),
      confirmPassword: new FormControl('', { nonNullable: true, validators: Validators.required }),
    },
    { validators: fieldsMatch('newPassword', 'confirmPassword') },
  );
  protected readonly newPassword = this.form.controls.newPassword;
  protected readonly confirmPassword = this.form.controls.confirmPassword;

  constructor() {
    const query = inject(ActivatedRoute).snapshot.queryParamMap;
    const email = query.get('email')?.trim();
    const token = query.get('token');
    this.link = email && token ? { email, token } : null;

    if (!this.link) {
      this.rejectLink('This reset link is incomplete. Copy the whole link, or request a new one.');
      return;
    }
    this.api.validateLink(this.link).subscribe({
      next: () => this.state.set('ready'),
      error: (e: HttpErrorResponse) =>
        this.rejectLink(apiErrorMessage(e, 'Could not check the reset link')),
    });
  }

  protected submit() {
    const link = this.link;
    if (!link || this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.formError.set('');
    this.api.resetPassword({ ...link, newPassword: this.newPassword.value }).subscribe({
      next: () => {
        // Any session in this browser was opened with the old password; sign in again with the new one.
        this.authService.logout();
        this.router.navigate([APP_PATHS.login], {
          queryParams: { email: link.email, [PASSWORD_CHANGED_PARAM]: true },
        });
      },
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(apiErrorMessage(e, 'Could not change the password'));
      },
    });
  }

  private rejectLink(message: string) {
    this.linkError.set(message);
    this.state.set('invalid');
  }
}
