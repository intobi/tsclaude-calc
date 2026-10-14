import { HttpErrorResponse } from '@angular/common/http';

/** The API's own message: `{ error }` for business errors, `{ errors }` for validation problems. */
export function apiErrorMessage(response: HttpErrorResponse, fallback: string): string {
  const body = response.error;
  const validation = body?.errors ? Object.values<string[]>(body.errors)[0]?.[0] : undefined;
  return body?.error ?? validation ?? fallback;
}
