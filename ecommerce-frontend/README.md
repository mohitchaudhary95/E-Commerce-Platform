# E-Commerce Frontend (Angular 18)

Standalone-components Angular frontend for the Microservices E-Commerce Platform. Talks to the backend through the Ocelot API Gateway on `http://localhost:5000`.

## Setup

```bash
npm install
ng serve
```

App runs at `http://localhost:4200`. Make sure the backend Gateway (and all services) are running first — `proxy.conf.json` forwards `/api` calls to `http://localhost:5000` during development, so there are no CORS issues.

## Build

```bash
ng build
```

Output goes to `dist/ecommerce-frontend`.

## Structure

```
src/app/
├── core/            # Services, guards, interceptors, models (singletons)
├── features/        # One folder per page/route (auth, products, cart, checkout, orders, admin)
├── shared/           # Reusable standalone components (navbar, toast)
├── app.config.ts     # Root providers: router, HttpClient, interceptor
└── app.routes.ts     # All routes with lazy loading + guards
```
