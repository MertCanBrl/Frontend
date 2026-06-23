import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.models';

const AUTH_KEY = 'mudek_auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, credentials)
      .pipe(
        tap((response) => {
          localStorage.setItem(AUTH_KEY, JSON.stringify(response));
        })
      );
  }

  logout(): void {
    localStorage.removeItem(AUTH_KEY);
    this.router.navigate(['/login']);
  }

  private getAuthData(): LoginResponse | null {
    const raw = localStorage.getItem(AUTH_KEY);
    return raw ? JSON.parse(raw) as LoginResponse : null;
  }

  getToken(): string | null {
    return this.getAuthData()?.token ?? null;
  }

  getRole(): string | null {
    return this.getAuthData()?.role ?? null;
  }

  getFullName(): string | null {
    return this.getAuthData()?.fullName ?? null;
  }

  getUserId(): number | null {
    return this.getAuthData()?.userId ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
