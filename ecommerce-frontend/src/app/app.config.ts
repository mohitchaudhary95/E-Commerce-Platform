import { ApplicationConfig } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

/**
 * Standalone application configuration (Angular 17+ style).
 * No AppModule needed — everything is configured here.
 *
 * Key providers:
 *  - provideRouter: sets up routing with component input binding
 *    (allows route params to be bound directly as component inputs)
 *  - provideHttpClient: enables HttpClient with the auth interceptor
 *    withInterceptors([authInterceptor]) attaches JWT to every request
 *  - provideAnimations: enables Angular animations
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
    provideAnimations()
  ]
};
