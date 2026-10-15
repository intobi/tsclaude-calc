import { APP_PATHS } from '../../../core/navigation';

/** Shortest allowed password; matches the backend (Identity) rule. */
export const PASSWORD_MIN_LENGTH = 4;

/** Longest allowed password; matches the backend validation. */
export const PASSWORD_MAX_LENGTH = 100;

/** Query parameter that tells the sign-in page the password was just changed. */
export const PASSWORD_CHANGED_PARAM = 'passwordChanged';

export interface ForgotPasswordRequest {
  email: string;
}

/**
 * Email delivery is simulated: the API returns the token a real system would email.
 * Both values are `null` when no account uses the email.
 */
export interface ForgotPasswordResponse {
  resetToken: string | null;
  expiresAtUtc: string | null;
}

/** The email and token carried by a reset link. */
export interface ResetLinkParams {
  email: string;
  token: string;
}

export interface ResetPasswordRequest extends ResetLinkParams {
  newPassword: string;
}

/** Absolute URL of the reset page for this email and token, as it would appear in the email. */
export function buildResetLink(origin: string, params: ResetLinkParams): string {
  const query = new URLSearchParams({ email: params.email, token: params.token });
  return `${origin}${APP_PATHS.resetPassword}?${query}`;
}
