/** Pure geometry and particle physics of the orbits scene; no DOM access, so it is easy to test. */

export interface Point {
  x: number;
  y: number;
}

/** A glowing sphere, one per open tab, in screen coordinates. */
export interface Sphere extends Point {
  id: string;
}

export interface Particle extends Point {
  /** Radius in CSS pixels. */
  size: number;
  /** Opacity from 0 to 1. */
  alpha: number;
}

/** Where a browser window is on the screen and how big its page area is. */
export interface WindowFrame {
  screenX: number;
  screenY: number;
  outerWidth: number;
  outerHeight: number;
  innerWidth: number;
  innerHeight: number;
}

/** Sphere radius in CSS pixels. */
export const SPHERE_RADIUS = 36;
/** Particles orbit between these distances from their sphere. */
export const ORBIT_MIN_RADIUS = 60;
export const ORBIT_MAX_RADIUS = 240;
/** Share of the particles that stream along the bridge when two or more spheres exist. */
export const BRIDGE_SHARE = 0.3;
/** Upper bound for the particle count, to keep every frame cheap. */
export const MAX_PARTICLES = 600;

/** Angular speed (rad/s) at the innermost orbit; outer orbits are slower (Kepler's third law). */
const INNER_ANGULAR_SPEED = 1.4;
/** One trip along a bridge takes this long at the slowest particle speed. */
const BRIDGE_TRIP_MS = 4000;

/**
 * Offset of the page area inside the window (title bar, tabs, toolbar, borders).
 * Side borders are assumed equal; everything else that is missing in height is at the top.
 */
export function chromeOffset(frame: WindowFrame): Point {
  const x = Math.max(0, (frame.outerWidth - frame.innerWidth) / 2);
  const y = Math.max(0, frame.outerHeight - frame.innerHeight - x);
  return { x, y };
}

export function clientToScreen(point: Point, frame: WindowFrame): Point {
  const chrome = chromeOffset(frame);
  return { x: frame.screenX + chrome.x + point.x, y: frame.screenY + chrome.y + point.y };
}

export function screenToClient(point: Point, frame: WindowFrame): Point {
  const chrome = chromeOffset(frame);
  return { x: point.x - frame.screenX - chrome.x, y: point.y - frame.screenY - chrome.y };
}

/** The centre of the page area in screen coordinates: where this tab's sphere lives. */
export function windowCentre(frame: WindowFrame): Point {
  return clientToScreen({ x: frame.innerWidth / 2, y: frame.innerHeight / 2 }, frame);
}

/**
 * Moves `current` toward `target` by exponential smoothing, so remote spheres glide instead of
 * jumping between the updates other tabs send. After `halfLifeMs` half the distance is covered.
 */
export function smoothToward(
  current: Point,
  target: Point,
  dtMs: number,
  halfLifeMs: number,
): Point {
  const keep = Math.pow(0.5, Math.max(0, dtMs) / halfLifeMs);
  return {
    x: target.x + (current.x - target.x) * keep,
    y: target.y + (current.y - target.y) * keep,
  };
}

/** Angular speed (rad/s) of an orbit with the given radius: ω ∝ r^-1.5. */
export function orbitalAngularSpeed(radius: number): number {
  return INNER_ANGULAR_SPEED * Math.pow(ORBIT_MIN_RADIUS / radius, 1.5);
}

/** Small seeded PRNG (mulberry32): the same seed always gives the same sequence. */
function random(seed: number): () => number {
  let state = seed >>> 0;
  return () => {
    state = (state + 0x6d2b79f5) >>> 0;
    let t = state;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

/** Point on the quadratic Bézier curve from `a` to `b` bent around `control`. */
function onCurve(a: Point, control: Point, b: Point, t: number): Point {
  const u = 1 - t;
  return {
    x: u * u * a.x + 2 * u * t * control.x + t * t * b.x,
    y: u * u * a.y + 2 * u * t * control.y + t * t * b.y,
  };
}

/** Control point that bends the bridge between two spheres sideways by `bend` × their distance. */
export function bridgeControl(a: Point, b: Point, bend: number): Point {
  return { x: (a.x + b.x) / 2 - (b.y - a.y) * bend, y: (a.y + b.y) / 2 + (b.x - a.x) * bend };
}

/**
 * Positions of every particle at `timeMs`, in screen coordinates.
 *
 * The result depends only on the arguments, so every tab computes the same particles for the same
 * spheres and clock: the scene looks like one continuous space across windows. Each particle
 * orbits a sphere on a tilted ellipse; with two or more spheres some particles are pulled from one
 * sphere to the next along the bridge, speeding up as they fall toward either end.
 */
export function simulateParticles(
  seed: number,
  timeMs: number,
  spheres: readonly Sphere[],
  count: number,
): Particle[] {
  if (spheres.length === 0) return [];
  const ordered = [...spheres].sort((a, b) => (a.id < b.id ? -1 : a.id > b.id ? 1 : 0));
  const next = random(seed);
  const seconds = timeMs / 1000;
  const particles: Particle[] = [];

  for (let i = 0; i < Math.min(count, MAX_PARTICLES); i++) {
    // Draw every random value up front so each particle's values do not depend on its branch.
    const [orbit, phase, flatten, tilt, direction, size, bridgeRoll, lane] = Array.from(
      { length: 8 },
      next,
    );
    const home = ordered[i % ordered.length];
    const particle = { size: 0.6 + size * 1.6, alpha: 0.35 + size * 0.65 };

    if (ordered.length > 1 && bridgeRoll < BRIDGE_SHARE) {
      const to = ordered[(i + 1) % ordered.length];
      const trip = BRIDGE_TRIP_MS * (1 + orbit);
      const progress = (phase + timeMs / trip) % 1;
      // Ease-in-out: slow in the middle, fast near the spheres that attract it.
      const eased = 0.5 - Math.cos(progress * Math.PI) / 2;
      const control = bridgeControl(home, to, (lane - 0.5) * 0.35);
      const fade = Math.sin(progress * Math.PI);
      particles.push({
        ...onCurve(home, control, to, eased),
        ...particle,
        alpha: particle.alpha * fade,
      });
      continue;
    }

    const radius = ORBIT_MIN_RADIUS + orbit * orbit * (ORBIT_MAX_RADIUS - ORBIT_MIN_RADIUS);
    const spin = direction < 0.85 ? 1 : -1;
    const angle = phase * 2 * Math.PI + spin * orbitalAngularSpeed(radius) * seconds;
    const minor = radius * (0.55 + flatten * 0.45);
    const rotation = tilt * Math.PI;
    const ex = Math.cos(angle) * radius;
    const ey = Math.sin(angle) * minor;
    particles.push({
      x: home.x + ex * Math.cos(rotation) - ey * Math.sin(rotation),
      y: home.y + ex * Math.sin(rotation) + ey * Math.cos(rotation),
      ...particle,
    });
  }
  return particles;
}
