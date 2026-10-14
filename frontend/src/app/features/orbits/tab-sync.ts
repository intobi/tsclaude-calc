import { Signal, signal } from '@angular/core';
import { Point } from './orbit-geometry';

/** Every open /orbits page on this machine (same origin) talks on this channel. */
export const ORBITS_CHANNEL = 'orbits';

/** How often a tab re-announces its sphere, even when it has not moved. */
export const HEARTBEAT_MS = 100;

/**
 * A peer that stays silent this long is treated as closed. Generous on purpose: browsers
 * throttle timers in background tabs to about once per second.
 */
export const PEER_TIMEOUT_MS = 3000;

/** A sphere owned by another tab, in screen coordinates. */
export interface Peer extends Point {
  id: string;
}

/** The part of `BroadcastChannel` that `TabSync` uses (tests pass a fake). */
export interface TabChannel {
  postMessage(message: unknown): void;
  close(): void;
  onmessage: ((event: MessageEvent) => void) | null;
}

/** Where `pagehide` and `visibilitychange` are listened to (the window in the app). */
export type PageEvents = Pick<Window, 'addEventListener' | 'removeEventListener'>;

type TabMessage = { type: 'state'; id: string; x: number; y: number } | { type: 'bye'; id: string };

function isTabMessage(data: unknown): data is TabMessage {
  if (typeof data !== 'object' || data === null) return false;
  const message = data as Record<string, unknown>;
  if (typeof message['id'] !== 'string') return false;
  if (message['type'] === 'bye') return true;
  return (
    message['type'] === 'state' && Number.isFinite(message['x']) && Number.isFinite(message['y'])
  );
}

/**
 * Lets the open tabs of the same page find each other: each tab announces where its sphere is
 * (on every move and on a heartbeat) and keeps the list of the other tabs' spheres.
 */
export class TabSync {
  private readonly peerMap = new Map<string, Peer & { seenAt: number }>();
  private readonly peerList = signal<Peer[]>([]);
  private readonly heartbeat: ReturnType<typeof setInterval>;
  private position: Point | null = null;
  private closed = false;

  /** The spheres of the other open tabs. */
  readonly peers: Signal<Peer[]> = this.peerList.asReadonly();

  constructor(
    private readonly channel: TabChannel = new BroadcastChannel(ORBITS_CHANNEL),
    private readonly page: PageEvents = window,
    readonly id: string = crypto.randomUUID(),
  ) {
    this.channel.onmessage = (event) => this.receive(event.data);
    this.heartbeat = setInterval(() => this.tick(), HEARTBEAT_MS);
    this.page.addEventListener('pagehide', this.onPageHide);
    this.page.addEventListener('visibilitychange', this.onVisibilityChange);
  }

  /** Moves this tab's sphere; other tabs hear about it right away. */
  setPosition(position: Point) {
    const moved = this.position?.x !== position.x || this.position?.y !== position.y;
    this.position = { x: position.x, y: position.y };
    if (moved) this.announce();
  }

  /** Says goodbye to the other tabs and stops listening. Safe to call more than once. */
  destroy() {
    if (this.closed) return;
    this.post({ type: 'bye', id: this.id });
    this.closed = true;
    clearInterval(this.heartbeat);
    this.page.removeEventListener('pagehide', this.onPageHide);
    this.page.removeEventListener('visibilitychange', this.onVisibilityChange);
    this.channel.onmessage = null;
    this.channel.close();
  }

  private readonly onPageHide = () => this.destroy();
  private readonly onVisibilityChange = () => this.announce();

  private tick() {
    this.announce();
    const oldest = Date.now() - PEER_TIMEOUT_MS;
    let pruned = false;
    for (const [id, peer] of this.peerMap) {
      if (peer.seenAt < oldest) {
        this.peerMap.delete(id);
        pruned = true;
      }
    }
    if (pruned) this.publish();
  }

  private announce() {
    if (this.position) this.post({ type: 'state', id: this.id, ...this.position });
  }

  private post(message: TabMessage) {
    if (!this.closed) this.channel.postMessage(message);
  }

  private receive(data: unknown) {
    if (!isTabMessage(data) || data.id === this.id) return;
    if (data.type === 'bye') {
      if (this.peerMap.delete(data.id)) this.publish();
      return;
    }
    const isNew = !this.peerMap.has(data.id);
    this.peerMap.set(data.id, { id: data.id, x: data.x, y: data.y, seenAt: Date.now() });
    this.publish();
    // A newcomer learns about this tab immediately instead of waiting for the next heartbeat.
    if (isNew) this.announce();
  }

  private publish() {
    this.peerList.set([...this.peerMap.values()].map(({ id, x, y }) => ({ id, x, y })));
  }
}
