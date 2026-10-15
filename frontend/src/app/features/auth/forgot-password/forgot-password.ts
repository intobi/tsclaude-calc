import { Component, DOCUMENT, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../core/http/api-error';
import { APP_PATHS } from '../../../core/navigation';
import { PasswordResetApi } from '../password-reset/password-reset-api';
import { ResetLinkParams, buildResetLink } from '../password-reset/password-reset.models';

/** What the page shows after a request: the simulated email, with or without a link. */
interface SentResult {
  email: string;
  /** `null` when no account uses the email. */
  link: { params: ResetLinkParams; url: string; expiresAtUtc: string } | null;
}

/** How long the "Copied" confirmation stays visible. */
const COPIED_FEEDBACK_MS = 2000;

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './forgot-password.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class ForgotPassword {
  private readonly api = inject(PasswordResetApi);
  private readonly document = inject(DOCUMENT);

  protected readonly paths = APP_PATHS;
  protected readonly form = new FormGroup({
    // Prefilled when coming from the sign-in page.
    email: new FormControl(inject(ActivatedRoute).snapshot.queryParamMap.get('email') ?? '', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
  });
  protected readonly email = this.form.controls.email;
  protected readonly sending = signal(false);
  protected readonly error = signal('');
  protected readonly result = signal<SentResult | null>(null);
  protected readonly copyStatus = signal<'idle' | 'copied' | 'manual'>('idle');
  private copiedTimer?: ReturnType<typeof setTimeout>;

  protected submit() {
    if (this.email.invalid || this.sending()) {
      this.email.markAsTouched();
      return;
    }
    const email = this.email.value.trim();
    this.sending.set(true);
    this.error.set('');
    this.api.requestReset({ email }).subscribe({
      next: ({ resetToken, expiresAtUtc }) => {
        this.sending.set(false);
        this.copyStatus.set('idle');
        const params = resetToken ? { email, token: resetToken } : null;
        this.result.set({
          email,
          link:
            params && expiresAtUtc
              ? {
                  params,
                  url: buildResetLink(this.document.location.origin, params),
                  expiresAtUtc,
                }
              : null,
        });
      },
      error: (e: HttpErrorResponse) => {
        this.sending.set(false);
        this.error.set(apiErrorMessage(e, 'Could not create a reset link'));
      },
    });
  }

  protected async copy(input: HTMLInputElement) {
    clearTimeout(this.copiedTimer);
    try {
      await navigator.clipboard.writeText(input.value);
      this.copyStatus.set('copied');
      this.copiedTimer = setTimeout(() => this.copyStatus.set('idle'), COPIED_FEEDBACK_MS);
    } catch {
      // Clipboard access can be blocked (permissions, insecure context): let the user copy it by hand.
      input.select();
      this.copyStatus.set('manual');
    }
  }

  protected startOver() {
    this.result.set(null);
    this.copyStatus.set('idle');
  }
}
