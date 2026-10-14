import { Signal, computed, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs';

/** In-app URLs used by the router config, the header menu and the calculator. */
export const APP_PATHS = {
  calculator: '/',
  history: '/history',
  books: '/books',
  authors: '/authors',
  orbits: '/orbits',
  login: '/login',
} as const;

export interface NavLink {
  label: string;
  path: string;
}

/**
 * The one-click switch between the calculator and its history: it always points at the other side.
 * From any other page it leads back to the calculator.
 */
export function viewToggleFor(path: string): NavLink {
  return path === APP_PATHS.calculator
    ? { label: 'History', path: APP_PATHS.history }
    : { label: 'Calculator', path: APP_PATHS.calculator };
}

/** The current URL path (without query or fragment), updated after every navigation. Call in an injection context. */
export function injectCurrentPath(): Signal<string> {
  const router = inject(Router);
  const url = toSignal(
    router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map(() => router.url),
      takeUntilDestroyed(),
    ),
    { initialValue: router.url },
  );
  return computed(() => url().split(/[?#]/)[0]);
}
