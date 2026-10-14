import { Component, ElementRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { APP_PATHS, NavLink, injectCurrentPath, viewToggleFor } from '../../core/navigation';
import { KEYBOARD_SHORTCUTS } from '../../features/calculator/keyboard-shortcuts';

type MenuId = 'shortcuts' | 'account';

interface MenuLink extends NavLink {
  /** The calculator/history switch; shown with a ⇄ icon. */
  toggle?: boolean;
}

/** Pages that always appear in the menu, after the calculator/history switch. */
const NAV_LINKS: NavLink[] = [
  { label: 'Books', path: APP_PATHS.books },
  { label: 'Authors', path: APP_PATHS.authors },
];

/** Burger icon lines; when the menu is open they morph into a cross. */
const BURGER_LINES = [
  'group-aria-expanded:translate-y-1.75 group-aria-expanded:rotate-45',
  'group-aria-expanded:opacity-0',
  'group-aria-expanded:-translate-y-1.75 group-aria-expanded:-rotate-45',
];

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  host: {
    class: 'block w-full p-4',
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'closeAll()',
  },
})
export class Header {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  private readonly currentPath = injectCurrentPath();

  protected readonly paths = APP_PATHS;
  /**
   * Menu items: first one switch between the calculator and the history
   * (same as the switch above the card), then the other pages.
   */
  protected readonly links = computed<MenuLink[]>(() => [
    { ...viewToggleFor(this.currentPath()), toggle: true },
    ...NAV_LINKS,
  ]);
  protected readonly shortcuts = KEYBOARD_SHORTCUTS;
  protected readonly burgerLines = BURGER_LINES;
  protected readonly email = this.authService.email;
  protected readonly loggedIn = computed(() => this.authService.isAuthenticated());
  protected readonly openMenu = signal<MenuId | null>(null);
  protected readonly mobileOpen = signal(false);

  constructor() {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.closeAll());
  }

  protected toggleMenu(id: MenuId) {
    this.openMenu.update((open) => (open === id ? null : id));
  }

  protected toggleMobile() {
    this.mobileOpen.update((open) => !open);
    this.openMenu.set(null);
  }

  protected closeAll() {
    this.openMenu.set(null);
    this.mobileOpen.set(false);
  }

  protected logout() {
    this.authService.logout();
    this.router.navigateByUrl(APP_PATHS.login);
  }

  protected onDocumentClick(event: MouseEvent) {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.closeAll();
    }
  }
}
