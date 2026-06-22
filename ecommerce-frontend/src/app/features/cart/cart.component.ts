import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/api.services';
import { ToastService } from '../../core/services/toast.service';
import { CartDto, CartItemDto } from '../../core/models/models';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="container">
        <h2>Shopping Cart</h2>

        @if (loading()) {
          <p>Loading your cart...</p>
        } @else if (!cart() || cart()!.items.length === 0) {
          <div class="empty-cart">
            <p>🛒 Your cart is empty</p>
            <a routerLink="/products" class="btn-primary">Browse Products</a>
          </div>
        } @else {
          <div class="cart-layout">
            <!-- Items -->
            <div class="cart-items">
              @for (item of cart()!.items; track item.id) {
                <div class="cart-item">
                  <img [src]="item.productImageUrl || 'https://placehold.co/80x80'" [alt]="item.productName" />
                  <div class="item-info">
                    <h4>{{ item.productName }}</h4>
                    <p class="unit-price">₹{{ item.unitPrice | number:'1.0-0' }} each</p>
                  </div>
                  <div class="qty-control">
                    <button (click)="updateQty(item, item.quantity - 1)" [disabled]="item.quantity <= 1">−</button>
                    <span>{{ item.quantity }}</span>
                    <button (click)="updateQty(item, item.quantity + 1)">+</button>
                  </div>
                  <div class="item-total">₹{{ item.totalPrice | number:'1.0-0' }}</div>
                  <button class="btn-remove" (click)="removeItem(item)">✕</button>
                </div>
              }
            </div>

            <!-- Summary -->
            <div class="cart-summary">
              <h3>Order Summary</h3>
              <div class="summary-row">
                <span>Items ({{ cart()!.totalItems }})</span>
                <span>₹{{ cart()!.totalAmount | number:'1.0-0' }}</span>
              </div>
              <div class="summary-row">
                <span>Shipping</span>
                <span class="free">FREE</span>
              </div>
              <div class="summary-row total">
                <strong>Total</strong>
                <strong>₹{{ cart()!.totalAmount | number:'1.0-0' }}</strong>
              </div>
              <a routerLink="/checkout" class="btn-checkout">Proceed to Checkout</a>
              <button class="btn-clear" (click)="clearCart()">Clear Cart</button>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 1000px; margin: 0 auto; padding: 0 16px; }
    h2 { color: #1a5276; margin-bottom: 24px; }
    .empty-cart { text-align: center; padding: 60px; background: #fff; border-radius: 10px; }
    .empty-cart p { font-size: 1.2rem; color: #666; margin-bottom: 16px; }
    .cart-layout { display: grid; grid-template-columns: 1fr 300px; gap: 24px; }
    .cart-items { display: flex; flex-direction: column; gap: 12px; }
    .cart-item { background: #fff; border-radius: 10px; padding: 16px; display: flex; align-items: center; gap: 16px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .cart-item img { width: 80px; height: 80px; object-fit: cover; border-radius: 6px; }
    .item-info { flex: 1; }
    .item-info h4 { margin: 0 0 4px; font-size: 0.95rem; }
    .unit-price { color: #666; font-size: 0.85rem; margin: 0; }
    .qty-control { display: flex; align-items: center; gap: 8px; }
    .qty-control button { width: 28px; height: 28px; border: 1px solid #ddd; background: #fff; border-radius: 4px; cursor: pointer; font-size: 1rem; }
    .qty-control button:disabled { opacity: 0.4; }
    .qty-control span { font-weight: 600; min-width: 24px; text-align: center; }
    .item-total { font-weight: 700; color: #1a5276; min-width: 80px; text-align: right; }
    .btn-remove { background: none; border: none; color: #e74c3c; cursor: pointer; font-size: 1rem; padding: 4px 8px; }
    .cart-summary { background: #fff; border-radius: 10px; padding: 20px; height: fit-content; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .cart-summary h3 { margin: 0 0 16px; color: #1a5276; }
    .summary-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #f0f0f0; font-size: 0.9rem; }
    .summary-row.total { font-size: 1rem; border-bottom: none; margin-top: 4px; }
    .free { color: #1e8449; }
    .btn-checkout { display: block; width: 100%; padding: 12px; background: #1a5276; color: #fff; text-align: center; text-decoration: none; border-radius: 6px; font-weight: 600; margin-top: 16px; }
    .btn-checkout:hover { background: #2e86c1; }
    .btn-primary { display: inline-block; padding: 10px 24px; background: #1a5276; color: #fff; text-decoration: none; border-radius: 6px; }
    .btn-clear { width: 100%; padding: 8px; background: transparent; border: 1px solid #ddd; border-radius: 6px; cursor: pointer; color: #666; margin-top: 8px; font-size: 0.9rem; }
  `]
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly toast = inject(ToastService);

  readonly cart = signal<CartDto | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.cartService.getCart().subscribe({
      next: res => { if (res.success) this.cart.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  updateQty(item: CartItemDto, qty: number): void {
    this.cartService.updateQuantity(item.id, { quantity: qty }).subscribe({
      next: res => { if (res.success) this.cart.set(res.data); }
    });
  }

  removeItem(item: CartItemDto): void {
    this.cartService.removeItem(item.id).subscribe({
      next: res => { if (res.success) { this.cart.set(res.data); this.toast.success('Item removed'); } }
    });
  }

  clearCart(): void {
    this.cartService.clearCart().subscribe({
      next: () => { this.cart.set(null); this.toast.info('Cart cleared'); }
    });
  }
}
