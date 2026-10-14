import {
  Component,
  DestroyRef,
  ElementRef,
  InjectionToken,
  afterNextRender,
  computed,
  inject,
  viewChild,
} from '@angular/core';
import {
  Particle,
  Point,
  SPHERE_RADIUS,
  Sphere,
  WindowFrame,
  bridgeControl,
  screenToClient,
  simulateParticles,
  smoothToward,
  windowCentre,
} from './orbit-geometry';
import { TabSync } from './tab-sync';

/** Creates the tab channel; tests replace it with a fake. */
export const TAB_SYNC_FACTORY = new InjectionToken<() => TabSync>('TAB_SYNC_FACTORY', {
  providedIn: 'root',
  factory: () => () => new TabSync(),
});

/** Shared by every tab so they all draw the same particles. */
const PARTICLE_SEED = 20261014;
const PARTICLE_COUNT = 360;
/** Remote spheres glide toward the positions other tabs report. */
const SPHERE_HALF_LIFE_MS = 90;
/** When a tab opens or closes, particles fly to their new sphere for this long. */
const REGROUP_MS = 1500;
const REGROUP_HALF_LIFE_MS = 300;
/** Faint trails: each frame only partly covers the previous one. */
const TRAIL_FADE = 0.3;
const MAX_PIXEL_RATIO = 2;

/** Colors come from the `@theme` tokens in tailwind.css. */
const COLOR_TOKENS = {
  space: '--color-space',
  orb: '--color-orb',
  glow: '--color-orb-glow',
  particle: '--color-particle',
} as const;
type Palette = Record<keyof typeof COLOR_TOKENS, string>;

function readPalette(): Palette {
  const style = getComputedStyle(document.documentElement);
  const entries = Object.entries(COLOR_TOKENS).map(([key, token]) => [
    key,
    style.getPropertyValue(token).trim(),
  ]);
  return Object.fromEntries(entries) as Palette;
}

function readFrame(): WindowFrame {
  const { screenX, screenY, outerWidth, outerHeight, innerWidth, innerHeight } = window;
  return { screenX, screenY, outerWidth, outerHeight, innerWidth, innerHeight };
}

const byId = (a: Sphere, b: Sphere) => (a.id < b.id ? -1 : a.id > b.id ? 1 : 0);

/**
 * Every open tab of this page is a glowing sphere at the centre of its window. Tabs find each
 * other through a BroadcastChannel, so the spheres line up across windows on the screen, a bridge
 * joins them and particles orbit them like planets.
 */
@Component({
  selector: 'app-orbits',
  templateUrl: './orbits.html',
  host: { class: 'flex w-full flex-1 flex-col items-center justify-end' },
})
export class Orbits {
  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('scene');
  private readonly sync = inject(TAB_SYNC_FACTORY)();
  private readonly reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');

  /** How many other windows are connected right now. */
  protected readonly peerCount = computed(() => this.sync.peers().length);

  private frameId = 0;
  private destroyed = false;
  private lastFrameAt = 0;
  private palette: Palette | null = null;
  private readonly rendered = new Map<string, Point>();
  private sphereKey = '';
  private regroupStartedAt = -Infinity;
  private previousParticles: Particle[] = [];

  constructor() {
    afterNextRender(() => this.schedule());
    inject(DestroyRef).onDestroy(() => {
      this.destroyed = true;
      cancelAnimationFrame(this.frameId);
      this.sync.destroy();
    });
  }

  private schedule() {
    if (!this.destroyed) this.frameId = requestAnimationFrame((now) => this.frame(now));
  }

  private frame(now: number) {
    const dt = this.lastFrameAt ? now - this.lastFrameAt : 0;
    this.lastFrameAt = now;
    const still = this.reducedMotion.matches;

    // Moving a window fires no event, so its position is read on every frame.
    const frame = readFrame();
    const own = { id: this.sync.id, ...windowCentre(frame) };
    this.sync.setPosition(own);

    const spheres = [own, ...this.glide(this.sync.peers(), dt, still)].sort(byId);
    const particles = this.regroup(
      simulateParticles(PARTICLE_SEED, still ? 0 : Date.now(), spheres, PARTICLE_COUNT),
      spheres,
      now,
      dt,
      still,
    );

    const context = this.prepareCanvas();
    if (context) {
      const toClient = (point: Point) => screenToClient(point, frame);
      this.draw(
        context,
        spheres.map((sphere) => ({ ...sphere, ...toClient(sphere) })),
        particles.map((particle) => ({ ...particle, ...toClient(particle) })),
        still,
        now,
      );
    }
    this.schedule();
  }

  /** Smooths the other tabs' spheres between their updates and forgets closed tabs. */
  private glide(peers: Sphere[], dt: number, still: boolean): Sphere[] {
    const ids = new Set(peers.map((peer) => peer.id));
    for (const id of this.rendered.keys()) if (!ids.has(id)) this.rendered.delete(id);

    return peers.map((peer) => {
      const previous = this.rendered.get(peer.id);
      const position =
        previous && !still ? smoothToward(previous, peer, dt, SPHERE_HALF_LIFE_MS) : peer;
      this.rendered.set(peer.id, position);
      return { id: peer.id, ...position };
    });
  }

  /**
   * When a tab opens or closes, particles are reassigned to spheres; instead of jumping they are
   * pulled to their new place, and the pull fades out over REGROUP_MS.
   */
  private regroup(
    particles: Particle[],
    spheres: Sphere[],
    now: number,
    dt: number,
    still: boolean,
  ): Particle[] {
    const key = spheres.map((sphere) => sphere.id).join('|');
    if (key !== this.sphereKey) {
      if (this.sphereKey) this.regroupStartedAt = now;
      this.sphereKey = key;
    }
    const elapsed = now - this.regroupStartedAt;
    const previous = this.previousParticles;
    let result = particles;
    if (!still && elapsed < REGROUP_MS && previous.length === particles.length) {
      const halfLife = REGROUP_HALF_LIFE_MS * (1 - elapsed / REGROUP_MS);
      result = particles.map((particle, i) => ({
        ...particle,
        ...smoothToward(previous[i], particle, dt, halfLife),
      }));
    }
    this.previousParticles = result;
    return result;
  }

  /** Matches the canvas backing store to its CSS size; returns null where 2D is unavailable. */
  private prepareCanvas(): CanvasRenderingContext2D | null {
    const canvas = this.canvas().nativeElement;
    const ratio = Math.min(devicePixelRatio || 1, MAX_PIXEL_RATIO);
    const width = Math.round(canvas.clientWidth * ratio);
    const height = Math.round(canvas.clientHeight * ratio);
    if (canvas.width !== width || canvas.height !== height) {
      canvas.width = width;
      canvas.height = height;
    }
    const context = canvas.getContext('2d');
    context?.setTransform(ratio, 0, 0, ratio, 0, 0);
    return context;
  }

  private draw(
    context: CanvasRenderingContext2D,
    spheres: Sphere[],
    particles: Particle[],
    still: boolean,
    now: number,
  ) {
    const palette = (this.palette ??= readPalette());
    const { clientWidth: width, clientHeight: height } = context.canvas;

    context.globalCompositeOperation = 'source-over';
    context.shadowBlur = 0;
    context.globalAlpha = still ? 1 : TRAIL_FADE;
    context.fillStyle = palette.space;
    context.fillRect(0, 0, width, height);

    context.globalCompositeOperation = 'lighter';
    this.drawBridges(context, spheres, palette);

    context.fillStyle = palette.particle;
    for (const particle of particles) {
      context.globalAlpha = particle.alpha;
      context.beginPath();
      context.arc(particle.x, particle.y, particle.size, 0, 2 * Math.PI);
      context.fill();
    }

    const pulse = still ? 1 : 1 + Math.sin(now / 600) * 0.04;
    for (const sphere of spheres) this.drawSphere(context, sphere, SPHERE_RADIUS * pulse, palette);
    context.globalAlpha = 1;
  }

  private drawBridges(context: CanvasRenderingContext2D, spheres: Sphere[], palette: Palette) {
    if (spheres.length < 2) return;
    // Two spheres share one bridge; three or more form a ring, like the particles' paths.
    const pairs = spheres.length === 2 ? 1 : spheres.length;
    context.shadowColor = palette.glow;
    for (let i = 0; i < pairs; i++) {
      const from = spheres[i];
      const to = spheres[(i + 1) % spheres.length];
      const control = bridgeControl(from, to, 0);
      for (const [color, width, alpha, blur] of [
        [palette.glow, 6, 0.45, 28],
        [palette.orb, 1.5, 0.7, 8],
      ] as const) {
        context.strokeStyle = color;
        context.lineWidth = width;
        context.globalAlpha = alpha;
        context.shadowBlur = blur;
        context.beginPath();
        context.moveTo(from.x, from.y);
        context.quadraticCurveTo(control.x, control.y, to.x, to.y);
        context.stroke();
      }
    }
    context.shadowBlur = 0;
  }

  private drawSphere(
    context: CanvasRenderingContext2D,
    sphere: Point,
    radius: number,
    palette: Palette,
  ) {
    const halo = radius * 3;
    const gradient = context.createRadialGradient(sphere.x, sphere.y, 0, sphere.x, sphere.y, halo);
    gradient.addColorStop(0, palette.orb);
    gradient.addColorStop(0.28, palette.orb);
    gradient.addColorStop(0.36, palette.glow);
    gradient.addColorStop(1, 'transparent');
    context.globalAlpha = 1;
    context.fillStyle = gradient;
    context.beginPath();
    context.arc(sphere.x, sphere.y, halo, 0, 2 * Math.PI);
    context.fill();
  }
}
