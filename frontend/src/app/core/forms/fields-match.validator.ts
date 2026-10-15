import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Group validator: `{ mismatch: true }` when the `confirm` control has a value that differs from `source`
 * (e.g. "Confirm password"). An empty confirmation is left to that control's own `required` validator.
 */
export function fieldsMatch(source: string, confirm: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const confirmation = group.get(confirm)?.value;
    return confirmation && confirmation !== group.get(source)?.value ? { mismatch: true } : null;
  };
}
