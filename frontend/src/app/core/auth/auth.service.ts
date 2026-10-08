import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthSession, LoginRequest, LoginResponse } from './auth.models';

const STORAGE_KEY = 'calc_auth';

function readSession(): AuthSession | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthSession;
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly session = signal<AuthSession | null>(readSession());
  readonly email = signal(this.session()?.email ?? null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap((response) => {
        const session: AuthSession = response;
        localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        this.session.set(session);
        this.email.set(session.email);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
    this.email.set(null);
  }

  getToken(): string | null {
    return this.session()?.token ?? null;
  }

  isAuthenticated(): boolean {
    const session = this.session();
    if (!session) return false;
    return new Date(session.expiresAtUtc).getTime() > Date.now();
  }
}
