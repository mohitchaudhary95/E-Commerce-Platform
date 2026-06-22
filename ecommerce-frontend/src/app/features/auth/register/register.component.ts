import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

function passwordMatchValidator(ctrl: AbstractControl) {
  const password = ctrl.get('password')?.value;
  const confirm = ctrl.get('confirmPassword')?.value;
  return password === confirm ? null : { mismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Create Account</h2>
        <p class="subtitle">Join us and start shopping</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-row">
            <div class="form-group">
              <label>First Name</label>
              <input formControlName="firstName" [class.error]="isInvalid('firstName')" />
              @if (isInvalid('firstName')) { <span class="error-msg">Required</span> }
            </div>
            <div class="form-group">
              <label>Last Name</label>
              <input formControlName="lastName" [class.error]="isInvalid('lastName')" />
              @if (isInvalid('lastName')) { <span class="error-msg">Required</span> }
            </div>
          </div>

          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" [class.error]="isInvalid('email')" />
            @if (isInvalid('email')) { <span class="error-msg">Enter a valid email</span> }
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" [class.error]="isInvalid('password')" />
            @if (isInvalid('password')) {
              <span class="error-msg">Min 8 chars, 1 uppercase, 1 number</span>
            }
          </div>

          <div class="form-group">
            <label>Confirm Password</label>
            <input type="password" formControlName="confirmPassword"
              [class.error]="form.hasError('mismatch') && form.get('confirmPassword')?.touched" />
            @if (form.hasError('mismatch') && form.get('confirmPassword')?.touched) {
              <span class="error-msg">Passwords do not match</span>
            }
          </div>

          <button type="submit" class="btn-primary" [disabled]="loading()">
            {{ loading() ? 'Creating account...' : 'Create Account' }}
          </button>
        </form>

        <p class="auth-link">Already have an account? <a routerLink="/auth/login">Sign in</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f4f6f8; }
    .auth-card { background: #fff; padding: 40px; border-radius: 12px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); width: 100%; max-width: 480px; }
    h2 { margin: 0 0 4px; color: #1a5276; font-size: 1.8rem; }
    .subtitle { color: #666; margin: 0 0 28px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .form-group { margin-bottom: 16px; }
    label { display: block; font-size: 0.9rem; font-weight: 500; color: #333; margin-bottom: 6px; }
    input { width: 100%; padding: 10px 14px; border: 1px solid #ddd; border-radius: 6px; font-size: 1rem; box-sizing: border-box; transition: border 0.2s; }
    input:focus { outline: none; border-color: #2e86c1; box-shadow: 0 0 0 3px rgba(46,134,193,0.1); }
    input.error { border-color: #e74c3c; }
    .error-msg { color: #e74c3c; font-size: 0.8rem; margin-top: 4px; display: block; }
    .btn-primary { width: 100%; padding: 12px; background: #1a5276; color: #fff; border: none; border-radius: 6px; font-size: 1rem; font-weight: 600; cursor: pointer; margin-top: 8px; }
    .btn-primary:hover:not(:disabled) { background: #2e86c1; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .auth-link { text-align: center; margin-top: 20px; color: #666; font-size: 0.9rem; }
    .auth-link a { color: #2e86c1; text-decoration: none; font-weight: 500; }
  `]
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);

  readonly form = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[A-Z])(?=.*[0-9])/)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    const { firstName, lastName, email, password, confirmPassword } = this.form.value;
    this.authService.register({ firstName: firstName!, lastName: lastName!, email: email!, password: password!, confirmPassword: confirmPassword! }).subscribe({
      next: res => { if (res.success) { this.toast.success('Account created!'); this.router.navigate(['/products']); } },
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false)
    });
  }
}
