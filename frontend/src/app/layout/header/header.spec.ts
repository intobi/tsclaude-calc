import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Header } from './header';

describe('Header', () => {
  let fixture: ComponentFixture<Header>;
  let root: HTMLElement;

  const button = (label: string) =>
    Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find(
      (b) => b.textContent?.trim() === label,
    )!;
  const submenu = (id: string) => root.querySelector<HTMLElement>(`#${id}`)!;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Header],
      providers: [provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(Header);
    fixture.detectChanges();
    root = fixture.nativeElement;
  });

  it('renders the navigation items from the design', () => {
    const labels = Array.from(root.querySelectorAll('nav > ul > li > :is(a, button)')).map((e) =>
      e.textContent!.trim(),
    );
    expect(labels).toEqual([
      'Company',
      'Services',
      'Industries',
      'Portfolio',
      'Tech Stack',
      'Blog',
      'Contact Us', // shown inside the menu on narrow screens
    ]);
  });

  it('opens one dropdown at a time and closes it with Escape', () => {
    button('Company').click();
    fixture.detectChanges();
    expect(submenu('submenu-company').hidden).toBe(false);
    expect(button('Company').getAttribute('aria-expanded')).toBe('true');

    button('Services').click();
    fixture.detectChanges();
    expect(submenu('submenu-company').hidden).toBe(true);
    expect(submenu('submenu-services').hidden).toBe(false);

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(submenu('submenu-services').hidden).toBe(true);
  });

  it('toggles the mobile menu with the burger button', () => {
    const burger = root.querySelector<HTMLButtonElement>('button[aria-label="Меню"]')!;
    burger.click();
    fixture.detectChanges();
    expect(root.querySelector('nav')!.getAttribute('data-open')).toBe('true');
    expect(burger.getAttribute('aria-expanded')).toBe('true');
  });
});
