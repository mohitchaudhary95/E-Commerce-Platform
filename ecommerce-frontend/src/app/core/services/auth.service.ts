import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginDto, RegisterDto, TokenResponseDto,
  UserDto, ApiResponse, ChangePasswordDto
} from '../models/models';

/**
 * AuthService using Angular Signals.
 *
 * Signals replace traditional BehaviorSubject/Observable patterns for state.
 * They are simpler: set() updates, computed() derives, effect() reacts.
 *
 * currentUser() — read anywhere, reactively updates template
 * isLoggedIn() — computed from currentUser
 * isAdmin()    — computed from currentUser role
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly api = `${environment.apiUrl}/auth`;

  // ── State (Signals) ──────────────────────────────────────────────────────
  private readonly _currentUser = signal<UserDto | null>(this.loadUserFromStorage());

  // Public read-only signals
  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this._currentUser() !== null);
  readonly isAdmin = computed(() => this._currentUser()?.role === 'Admin');
  readonly userFullName = computed(() => this._currentUser()?.fullName ?? '');

  // ── Auth operations ──────────────────────────────────────────────────────

  register(dto: RegisterDto): Observable<ApiResponse<TokenResponseDto>> {
    return this.http.post<ApiResponse<TokenResponseDto>>(`${this.api}/register`, dto).pipe(
      tap(res => { if (res.success && res.data) this.saveSession(res.data); })
    );
  }

  login(dto: LoginDto): Observable<ApiResponse<TokenResponseDto>> {
    return this.http.post<ApiResponse<TokenResponseDto>>(`${this.api}/login`, dto).pipe(
      tap(res => { if (res.success && res.data) this.saveSession(res.data); })
    );
  }

  logout(): void {
    this.http.post(`${this.api}/logout`, {}).subscribe();
    this.clearSession();
    this.router.navigate(['/auth/login']);
  }

  changePassword(dto: ChangePasswordDto): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.api}/change-password`, dto);
  }

  refreshToken(): Observable<ApiResponse<TokenResponseDto>> {
    const refreshToken = localStorage.getItem('refreshToken') ?? '';
    return this.http.post<ApiResponse<TokenResponseDto>>(`${this.api}/refresh`, { refreshToken }).pipe(
      tap(res => { if (res.success && res.data) this.saveSession(res.data); })
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  // ── Private helpers ──────────────────────────────────────────────────────

  private saveSession(data: TokenResponseDto): void {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    this._currentUser.set(data.user);
  }

  private clearSession(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this._currentUser.set(null);
  }

  private loadUserFromStorage(): UserDto | null {
    try {
      const raw = localStorage.getItem('user');
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
