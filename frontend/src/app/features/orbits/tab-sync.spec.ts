import { HEARTBEAT_MS, PEER_TIMEOUT_MS, PageEvents, TabChannel, TabSync } from './tab-sync';

class FakeChannel implements TabChannel {
  readonly sent: unknown[] = [];
  closed = false;
  onmessage: ((event: MessageEvent) => void) | null = null;

  postMessage(message: unknown) {
    this.sent.push(message);
  }

  close() {
    this.closed = true;
  }

  /** Delivers a message as if another tab had posted it. */
  receive(data: unknown) {
    this.onmessage?.({ data } as MessageEvent);
  }
}

describe('TabSync', () => {
  let channel: FakeChannel;
  let page: EventTarget & PageEvents;
  let sync: TabSync;

  beforeEach(() => {
    vi.useFakeTimers();
    channel = new FakeChannel();
    page = new EventTarget() as EventTarget & PageEvents;
    sync = new TabSync(channel, page, 'me');
  });

  afterEach(() => {
    sync.destroy();
    vi.useRealTimers();
  });

  it('adds a peer when another tab reports its state and removes it on bye', () => {
    channel.receive({ type: 'state', id: 'other', x: 10, y: 20 });
    expect(sync.peers()).toEqual([{ id: 'other', x: 10, y: 20 }]);

    channel.receive({ type: 'state', id: 'other', x: 30, y: 40 });
    expect(sync.peers()).toEqual([{ id: 'other', x: 30, y: 40 }]);

    channel.receive({ type: 'bye', id: 'other' });
    expect(sync.peers()).toEqual([]);
  });

  it('ignores its own echo and malformed messages', () => {
    channel.receive({ type: 'state', id: 'me', x: 1, y: 1 });
    channel.receive({ type: 'state', id: 'x', x: 'left', y: 1 });
    channel.receive('hello');
    channel.receive(null);
    expect(sync.peers()).toEqual([]);
  });

  it('forgets a peer that stays silent too long', () => {
    channel.receive({ type: 'state', id: 'other', x: 1, y: 2 });
    vi.advanceTimersByTime(PEER_TIMEOUT_MS - HEARTBEAT_MS);
    expect(sync.peers()).toHaveLength(1);

    vi.advanceTimersByTime(2 * HEARTBEAT_MS);
    expect(sync.peers()).toEqual([]);
  });

  it('announces a move at once and repeats its state on every heartbeat', () => {
    sync.setPosition({ x: 5, y: 6 });
    expect(channel.sent).toEqual([{ type: 'state', id: 'me', x: 5, y: 6 }]);

    vi.advanceTimersByTime(2 * HEARTBEAT_MS);
    expect(channel.sent).toHaveLength(3);
    expect(HEARTBEAT_MS).toBeLessThanOrEqual(200);
  });

  it('answers a newcomer right away', () => {
    sync.setPosition({ x: 5, y: 6 });
    channel.receive({ type: 'state', id: 'new', x: 0, y: 0 });
    expect(channel.sent).toHaveLength(2);
  });

  it('says bye and closes the channel when the page is hidden for good', () => {
    page.dispatchEvent(new Event('pagehide'));
    expect(channel.sent).toEqual([{ type: 'bye', id: 'me' }]);
    expect(channel.closed).toBe(true);
  });

  it('stops posting after destroy', () => {
    sync.setPosition({ x: 1, y: 1 });
    sync.destroy();
    const sent = channel.sent.length;

    vi.advanceTimersByTime(10 * HEARTBEAT_MS);
    sync.setPosition({ x: 2, y: 2 });
    expect(channel.sent).toHaveLength(sent);
    expect(channel.closed).toBe(true);
  });
});
