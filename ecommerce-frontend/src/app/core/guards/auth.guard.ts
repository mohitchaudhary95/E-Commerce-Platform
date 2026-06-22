import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Protects routes that require login.
 * Usage in route config: canActivate: [authGuard]
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) return true;

  // Redirect to login, preserving the intended URL
  return router.createUrlTree(['/auth/login']);
};

/**
 * Protects admin-only routes.
 * Usage in route config: canActivate: [adminGuard]
 */
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAdmin()) return true;

  // Not admin — redirect to home
  return router.createUrlTree(['/']);
};

/**
 * Redirects already-logged-in users away from auth pages.
 * Usage: canActivate: [guestGuard] on /auth/login and /auth/register
 */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) return true;
  return router.createUrlTree(['/products']);
};
