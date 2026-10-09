import { formatNumber } from './operations';

describe('formatNumber', () => {
  it('hides floating-point noise', () => {
    expect(formatNumber(0.1 + 0.2)).toBe('0.3');
    expect(formatNumber(1.1 * 3)).toBe('3.3');
  });

  it('keeps integers and regular decimals unchanged', () => {
    expect(formatNumber(42)).toBe('42');
    expect(formatNumber(-12.5)).toBe('-12.5');
  });

  it('uses exponent notation for very large numbers', () => {
    expect(formatNumber(1e21)).toBe('1e+21');
  });
});
