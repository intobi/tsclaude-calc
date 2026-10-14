import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Orbits, TAB_SYNC_FACTORY } from './orbits';
import { PageEvents, TabChannel, TabSync } from './tab-sync';

class FakeChannel implements TabChannel {
  closed = false;
  onmessage: ((event: MessageEvent) => void) | null = null;
  postMessage() {}
  close() {
    this.closed = true;
  }
  receive(data: unknown) {
    this.onmessage?.({ data } as MessageEvent);
  }
}

describe('Orbits', () => {
  let fixture: ComponentFixture<Orbits>;
  let root: HTMLElement;
  let channel: FakeChannel;
  let reducedMotion: boolean;
  let frames: FrameRequestCallback[];
  const requestFrame = vi.fn((callback: FrameRequestCallback) => frames.push(callback));
  const cancelFrame = vi.fn();
  /** Runs every pending frame callback (Angular's zoneless scheduler may queue one too). */
  const runFrame = () => frames.splice(0).forEach((callback) => callback(performance.now()));

  beforeEach(async () => {
    frames = [];
    reducedMotion = false;
    channel = new FakeChannel();
    vi.stubGlobal('requestAnimationFrame', requestFrame);
    vi.stubGlobal('cancelAnimationFrame', cancelFrame);
    vi.stubGlobal('matchMedia', () => ({
      get matches() {
        return reducedMotion;
      },
    }));
    // jsdom cannot draw; the component must cope with a missing 2D context.
    vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(null);

    await TestBed.configureTestingModule({
      imports: [Orbits],
      providers: [
        {
          provide: TAB_SYNC_FACTORY,
          useValue: () => new TabSync(channel, new EventTarget() as unknown as PageEvents, 'me'),
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(Orbits);
    fixture.detectChanges();
    await fixture.whenStable();
    root = fixture.nativeElement;
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
    requestFrame.mockClear();
    cancelFrame.mockClear();
  });

  it('renders the scene as a labelled image', () => {
    const canvas = root.querySelector('canvas[role="img"]')!;
    expect(canvas.getAttribute('aria-label')).toContain('spheres');
    expect(root.querySelector('h1')!.textContent).toBe('Orbits');
  });

  it('keeps animating frame after frame', () => {
    runFrame();
    const calls = requestFrame.mock.calls.length;
    runFrame();
    expect(requestFrame.mock.calls.length).toBeGreaterThan(calls);
    expect(frames.length).toBeGreaterThan(0);
  });

  it('freezes the particles when the user prefers reduced motion', () => {
    const arcs: number[][] = [];
    // A stand-in 2D context that records where circles are drawn and ignores everything else.
    const context = new Proxy(
      { canvas: root.querySelector('canvas')! },
      {
        get: (target, name) =>
          name in target
            ? target[name as 'canvas']
            : name === 'arc'
              ? (...args: number[]) => arcs.push(args)
              : name === 'createRadialGradient'
                ? () => ({ addColorStop() {} })
                : () => undefined,
        set: () => true,
      },
    );
    vi.mocked(HTMLCanvasElement.prototype.getContext).mockReturnValue(
      context as unknown as CanvasRenderingContext2D,
    );
    reducedMotion = true;

    runFrame();
    const first = arcs.splice(0);
    vi.spyOn(Date, 'now').mockReturnValue(Date.now() + 5000);
    runFrame();

    expect(first.length).toBeGreaterThan(1);
    expect(arcs).toEqual(first);
  });

  it('shows how many other windows are connected', () => {
    const count = () => root.querySelector('[data-testid="peer-count"]')!.textContent!.trim();
    expect(count()).toBe('No other windows connected yet');

    channel.receive({ type: 'state', id: 'other', x: 1, y: 1 });
    fixture.detectChanges();
    expect(count()).toBe('1 other window connected');
  });

  it('stops the animation and closes the channel when destroyed', () => {
    runFrame();
    fixture.destroy();
    expect(cancelFrame).toHaveBeenCalled();
    expect(channel.closed).toBe(true);
    runFrame();
    expect(frames).toHaveLength(0);
  });
});
