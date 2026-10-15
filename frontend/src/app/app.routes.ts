import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password').then((m) => m.ResetPassword),
  },
  {
    path: 'books',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/books').then((m) => m.Books),
  },
  {
    path: 'authors',
    canActivate: [authGuard],
    loadComponent: () => import('./features/authors/authors').then((m) => m.Authors),
  },
  {
    path: 'orbits',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orbits/orbits').then((m) => m.Orbits),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/calculator/calculator').then((m) => m.Calculator),
    // Both views live in one Calculator instance, so switching between them flips the card
    // instead of re-creating the component.
    children: [
      { path: '', children: [] },
      { path: 'history', children: [] },
    ],
  },
  { path: '**', redirectTo: '' },
];
