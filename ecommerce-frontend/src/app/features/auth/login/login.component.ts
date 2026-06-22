import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Welcome Back</h2>
        <p class="subtitle">Sign in to your account</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">

          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" placeholder="you@example.com"
              [class.error]="isInvalid('email')" />
            @if (isInvalid('email')) {
              <span class="error-msg">Enter a valid email address</span>
            }
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="Your password"
              [class.error]="isInvalid('password')" />
            @if (isInvalid('password')) {
              <span class="error-msg">Password is required</span>
            }
          </div>

          <button type="submit" class="btn-primary" [disabled]="loading()">
            {{ loading() ? 'Signing in...' : 'Sign In' }}
          </button>

        </form>

        <p class="auth-link">
          Don't have an account? <a routerLink="/auth/register">Register here</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f4f6f8; }
    .auth-card { background: #fff; padding: 40px; border-radius: 12px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); width: 100%; max-width: 420px; }
    h2 { margin: 0 0 4px; color: #1a5276; font-size: 1.8rem; }
    .subtitle { color: #666; margin: 0 0 28px; }
    .form-group { margin-bottom: 18px; }
    label { display: block; font-size: 0.9rem; font-weight: 500; color: #333; margin-bottom: 6px; }
    input { width: 100%; padding: 10px 14px; border: 1px solid #ddd; border-radius: 6px; font-size: 1rem; box-sizing: border-box; transition: border 0.2s; }
    input:focus { outline: none; border-color: #2e86c1; box-shadow: 0 0 0 3px rgba(46,134,193,0.1); }
    input.error { border-color: #e74c3c; }
    .error-msg { color: #e74c3c; font-size: 0.8rem; margin-top: 4px; display: block; }
    .btn-primary { width: 100%; padding: 12px; background: #1a5276; color: #fff; border: none; border-radius: 6px; font-size: 1rem; font-weight: 600; cursor: pointer; transition: background 0.2s; margin-top: 8px; }
    .btn-primary:hover:not(:disabled) { background: #2e86c1; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .auth-link { text-align: center; margin-top: 20px; color: #666; font-size: 0.9rem; }
    .auth-link a { color: #2e86c1; text-decoration: none; font-weight: 500; }
  `]
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading.set(true);
    const { email, password } = this.form.value;

    this.authService.login({ email: email!, password: password! }).subscribe({
      next: res => {
        if (res.success) {
          this.toast.success('Welcome back!');
          this.router.navigate(['/products']);
        }
      },
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false)
    });
  }
}
