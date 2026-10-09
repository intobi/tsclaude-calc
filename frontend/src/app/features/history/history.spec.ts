import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { History } from './history';

describe('History', () => {
  let fixture: ComponentFixture<History>;
  let root: HTMLElement;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [History],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    fixture = TestBed.createComponent(History);
    fixture.detectChanges();
    root = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('renders history items returned by the API', () => {
    http.expectOne('/api/history').flush([
      {
        id: 2,
        operation: 'multiply',
        operandA: 6,
        operandB: 7,
        result: 42,
        createdAtUtc: '2026-10-09T13:05:13Z',
      },
      {
        id: 1,
        operation: 'add',
        operandA: 1,
        operandB: 2,
        result: 3,
        createdAtUtc: '2026-10-08T09:17:29Z',
      },
    ]);
    fixture.detectChanges();

    const items = Array.from(root.querySelectorAll('li > span')).map((e) =>
      e.textContent!.replace(/\s+/g, ' ').trim(),
    );
    expect(items).toEqual(['6 × 7 = 42', '1 + 2 = 3']);
  });

  it('shows an empty state', () => {
    http.expectOne('/api/history').flush([]);
    fixture.detectChanges();
    expect(root.querySelector('p')!.textContent!.trim()).toBe('Історія порожня');
  });

  it('reloads when refreshKey changes', () => {
    http.expectOne('/api/history').flush([]);
    fixture.componentRef.setInput('refreshKey', 1);
    fixture.detectChanges();
    http.expectOne('/api/history').flush([]);
  });
});
