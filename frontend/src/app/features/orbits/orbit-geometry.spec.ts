import {
  MAX_PARTICLES,
  ORBIT_MIN_RADIUS,
  WindowFrame,
  chromeOffset,
  clientToScreen,
  orbitalAngularSpeed,
  screenToClient,
  simulateParticles,
  smoothToward,
  windowCentre,
} from './orbit-geometry';

// A 1200×800 window at (100, 50) with 8px side borders and 80px of chrome on top.
const frame: WindowFrame = {
  screenX: 100,
  screenY: 50,
  outerWidth: 1216,
  outerHeight: 888,
  innerWidth: 1200,
  innerHeight: 800,
};

const spheres = [
  { id: 'b', x: 900, y: 400 },
  { id: 'a', x: 300, y: 400 },
];

describe('orbit geometry', () => {
  it('measures the browser chrome around the page', () => {
    expect(chromeOffset(frame)).toEqual({ x: 8, y: 80 });
  });

  it('converts between page and screen coordinates both ways', () => {
    const point = { x: 15, y: 25 };
    expect(clientToScreen(point, frame)).toEqual({ x: 123, y: 155 });
    expect(screenToClient(clientToScreen(point, frame), frame)).toEqual(point);
  });

  it('puts the sphere in the middle of the page area', () => {
    expect(windowCentre(frame)).toEqual({ x: 708, y: 530 });
  });

  it('covers half the distance after one half-life', () => {
    expect(smoothToward({ x: 0, y: 0 }, { x: 100, y: -40 }, 200, 200)).toEqual({ x: 50, y: -20 });
    expect(smoothToward({ x: 7, y: 7 }, { x: 100, y: 100 }, 0, 200)).toEqual({ x: 7, y: 7 });
  });

  it('slows down outer orbits like planets', () => {
    expect(orbitalAngularSpeed(2 * ORBIT_MIN_RADIUS)).toBeLessThan(
      orbitalAngularSpeed(ORBIT_MIN_RADIUS),
    );
  });

  it('gives every tab the same particles for the same spheres and time', () => {
    const first = simulateParticles(42, 1000, spheres, 200);
    const second = simulateParticles(42, 1000, [...spheres].reverse(), 200);
    expect(second).toEqual(first);
    expect(simulateParticles(42, 1500, spheres, 200)).not.toEqual(first);
  });

  it('draws nothing without spheres and caps the particle count', () => {
    expect(simulateParticles(1, 0, [], 100)).toEqual([]);
    expect(simulateParticles(1, 0, spheres, 10_000)).toHaveLength(MAX_PARTICLES);
  });
});
