import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Rejects text made only of whitespace (the backend does the same). */
export const notBlank = (control: AbstractControl<string>): ValidationErrors | null =>
  control.value.trim() ? null : { blank: true };
