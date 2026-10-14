import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
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
