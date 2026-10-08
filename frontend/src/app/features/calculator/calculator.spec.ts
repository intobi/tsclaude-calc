import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Calculator } from './calculator';

describe('Calculator', () => {
  let fixture: ComponentFixture<Calculator>;
  let root: HTMLElement;
  let http: HttpTestingController;

  const click = (label: string) => {
    const button = Array.from(root.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === label,
    );
    button!.click();
    fixture.detectChanges();
  };
  const display = () => root.querySelector('.value')!.textContent!.trim();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Calculator],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(Calculator);
    fixture.detectChanges();
    root = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
  });

  it('renders all keypad buttons', () => {
    expect(root.querySelectorAll('.key').length).toBe(17);
  });

  it('shows typed digits', () => {
    click('1');
    click('2');
    expect(display()).toBe('12');
  });

  it('sends the calculation to the API and shows the result', () => {
    click('6');
    click('×');
    click('7');
    click('=');
    const req = http.expectOne('/api/calculate');
    expect(req.request.body).toEqual({ a: 6, b: 7, operation: 'multiply' });
    req.flush({ result: 42 });
    fixture.detectChanges();
    expect(display()).toBe('42');
  });
});
