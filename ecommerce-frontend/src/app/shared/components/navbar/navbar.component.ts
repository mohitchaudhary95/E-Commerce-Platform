import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/api.services';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="nav-container">

        <!-- Brand -->
        <a routerLink="/" class="brand">🛒 ShopNow</a>

        <!-- Nav Links -->
        <ul class="nav-links">
          <li><a routerLink="/products" routerLinkActive="active">Products</a></li>

          @if (auth.isLoggedIn()) {
            <li>
              <a routerLink="/cart" routerLinkActive="active" class="cart-link">
                Cart
                @if (cartService.itemCount() > 0) {
                  <span class="badge">{{ cartService.itemCount() }}</span>
                }
              </a>
            </li>
            <li><a routerLink="/orders" routerLinkActive="active">My Orders</a></li>

            @if (auth.isAdmin()) {
              <li><a routerLink="/admin" routerLinkActive="active" class="admin-link">Admin</a></li>
            }

            <li class="user-menu">
              <span class="user-name">{{ auth.userFullName() }}</span>
              <button (click)="auth.logout()" class="btn-logout">Logout</button>
            </li>
          } @else {
            <li><a routerLink="/auth/login" class="btn-login">Login</a></li>
            <li><a routerLink="/auth/register" class="btn-register">Register</a></li>
          }
        </ul>
      </div>
    </nav>
  `,
  styles: [`
    .navbar { background: #1a5276; padding: 0 24px; box-shadow: 0 2px 8px rgba(0,0,0,0.2); }
    .nav-container { max-width: 1200px; margin: 0 auto; display: flex; align-items: center; justify-content: space-between; height: 60px; }
    .brand { color: #fff; font-size: 1.4rem; font-weight: 700; text-decoration: none; }
    .nav-links { list-style: none; display: flex; align-items: center; gap: 8px; margin: 0; padding: 0; }
    .nav-links a { color: rgba(255,255,255,0.85); text-decoration: none; padding: 6px 12px; border-radius: 4px; transition: background 0.2s; font-size: 0.95rem; }
    .nav-links a:hover, .nav-links a.active { background: rgba(255,255,255,0.15); color: #fff; }
    .cart-link { position: relative; }
    .badge { position: absolute; top: -6px; right: -6px; background: #e74c3c; color: #fff; border-radius: 50%; width: 18px; height: 18px; font-size: 11px; display: flex; align-items: center; justify-content: center; font-weight: 700; }
    .admin-link { color: #f39c12 !important; }
    .user-menu { display: flex; align-items: center; gap: 8px; margin-left: 8px; }
    .user-name { color: rgba(255,255,255,0.7); font-size: 0.9rem; }
    .btn-logout { background: transparent; border: 1px solid rgba(255,255,255,0.4); color: #fff; padding: 4px 12px; border-radius: 4px; cursor: pointer; font-size: 0.9rem; transition: all 0.2s; }
    .btn-logout:hover { background: rgba(231,76,60,0.7); border-color: transparent; }
    .btn-login { background: rgba(255,255,255,0.15); color: #fff !important; border-radius: 4px; }
    .btn-register { background: #2e86c1; color: #fff !important; border-radius: 4px; }
  `]
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
  readonly cartService = inject(CartService);
}
