import { FormControl, FormGroup } from '@angular/forms';
import { fieldsMatch } from './fields-match.validator';

describe('fieldsMatch', () => {
  const group = (password: string, confirm: string) =>
    new FormGroup(
      { password: new FormControl(password), confirm: new FormControl(confirm) },
      { validators: fieldsMatch('password', 'confirm') },
    );

  it('passes when the values are equal', () => {
    expect(group('secret', 'secret').errors).toBeNull();
  });

  it('reports a mismatch when the confirmation differs', () => {
    expect(group('secret', 'secrex').errors).toEqual({ mismatch: true });
  });

  it('leaves an empty confirmation to the required validator', () => {
    expect(group('secret', '').errors).toBeNull();
  });
});
