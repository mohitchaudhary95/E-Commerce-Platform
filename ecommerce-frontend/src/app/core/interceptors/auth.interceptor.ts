import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

/**
 * Functional HTTP interceptor (Angular 17+ style).
 *
 * What it does:
 *  1. Adds Authorization: Bearer {token} header to every request
 *  2. If API returns 401 → attempts token refresh once
 *  3. If refresh fails → logs user out
 *  4. If any other error → shows toast notification
 */
export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);

  const token = authService.getAccessToken();
  const authReq = token ? addToken(req, token) : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/')) {
        // Token expired — try to refresh once
        return authService.refreshToken().pipe(
          switchMap(res => {
            if (res.success && res.data) {
              return next(addToken(req, res.data.accessToken));
            }
            authService.logout();
            return throwError(() => error);
          }),
          catchError(refreshError => {
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }

      // Show error toast for non-auth errors
      if (error.status !== 401) {
        const message = error.error?.message ?? 'Something went wrong. Please try again.';
        toastService.error(message);
      }

      return throwError(() => error);
    })
  );
};

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
