import { buildResetLink } from './password-reset.models';

describe('buildResetLink', () => {
  it('builds an absolute link to the reset page with the email and token encoded', () => {
    const link = buildResetLink('http://localhost:4200', {
      email: 'jane+test@example.com',
      token: 'a/b+c==',
    });

    expect(link).toBe(
      'http://localhost:4200/reset-password?email=jane%2Btest%40example.com&token=a%2Fb%2Bc%3D%3D',
    );
    const query = new URL(link).searchParams;
    expect([query.get('email'), query.get('token')]).toEqual(['jane+test@example.com', 'a/b+c==']);
  });
});
