import { Routes } from '@angular/router';
import { authGuard, adminGuard, guestGuard } from './core/guards/auth.guard';

/**
 * Lazy loading: each route loads its component bundle only when navigated to.
 * Smaller initial bundle → faster first load.
 *
 * Guards:
 *   guestGuard  → redirects logged-in users away from /auth pages
 *   authGuard   → redirects guests away from protected pages
 *   adminGuard  → redirects non-admins away from /admin pages
 */
export const routes: Routes = [
  // Default redirect
  { path: '', redirectTo: '/products', pathMatch: 'full' },

  // Auth — guests only
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },

  // Products — public
  {
    path: 'products',
    loadComponent: () => import('./features/products/listing/product-listing.component').then(m => m.ProductListingComponent)
  },
  {
    path: 'products/:id',
    loadComponent: () => import('./features/products/detail/product-detail.component').then(m => m.ProductDetailComponent)
  },

  // Cart — requires login
  {
    path: 'cart',
    canActivate: [authGuard],
    loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent)
  },

  // Checkout — requires login
  {
    path: 'checkout',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent)
  },

  // Orders — requires login
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/history/order-history.component').then(m => m.OrderHistoryComponent)
  },
  {
    path: 'orders/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/detail/order-detail.component').then(m => m.OrderDetailComponent)
  },

  // Admin — requires Admin role
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },

  // Catch-all
  { path: '**', redirectTo: '/products' }
];
