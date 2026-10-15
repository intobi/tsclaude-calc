import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ForgotPasswordRequest,
  ForgotPasswordResponse,
  ResetLinkParams,
  ResetPasswordRequest,
} from './password-reset.models';

const AUTH_URL = '/api/auth';

@Injectable({ providedIn: 'root' })
export class PasswordResetApi {
  private readonly http = inject(HttpClient);

  /** Creates a reset link for the email (returned instead of emailed in this demo). */
  requestReset(request: ForgotPasswordRequest): Observable<ForgotPasswordResponse> {
    return this.http.post<ForgotPasswordResponse>(`${AUTH_URL}/forgot-password`, request);
  }

  /** Completes when the link can still be used; errors with 400 otherwise. */
  validateLink(params: ResetLinkParams): Observable<void> {
    return this.http.post<void>(`${AUTH_URL}/reset-password/validate`, params);
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${AUTH_URL}/reset-password`, request);
  }
}
