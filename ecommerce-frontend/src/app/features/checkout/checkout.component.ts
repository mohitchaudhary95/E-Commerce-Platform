import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CartService, OrderService } from '../../core/services/api.services';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { CartDto } from '../../core/models/models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page">
      <div class="container">
        <h2>Checkout</h2>

        @if (loading()) {
          <p>Loading cart...</p>
        } @else if (!cart() || cart()!.items.length === 0) {
          <div class="empty">
            <p>Your cart is empty. Add some products first.</p>
          </div>
        } @else {
          <div class="checkout-layout">

            <!-- Shipping Form -->
            <div class="form-section">
              <h3>Shipping Details</h3>
              <form [formGroup]="form">
                <div class="form-group">
                  <label>Full Shipping Address</label>
                  <textarea formControlName="shippingAddress" rows="3"
                    placeholder="123, MG Road, Sector 14, Faridabad, Haryana - 121001"
                    [class.error]="isInvalid('shippingAddress')"></textarea>
                  @if (isInvalid('shippingAddress')) {
                    <span class="error-msg">Shipping address is required</span>
                  }
                </div>
              </form>

              <!-- Order Items Preview -->
              <h3 style="margin-top: 24px">Items in This Order</h3>
              <div class="items-list">
                @for (item of cart()!.items; track item.id) {
                  <div class="order-item">
                    <span class="item-name">{{ item.productName }}</span>
                    <span class="item-qty">× {{ item.quantity }}</span>
                    <span class="item-price">₹{{ item.totalPrice | number:'1.0-0' }}</span>
                  </div>
                }
              </div>
            </div>

            <!-- Summary -->
            <div class="summary-section">
              <h3>Payment Summary</h3>
              <div class="summary-row">
                <span>Subtotal</span>
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

              <div class="payment-note">
                <p>💳 Payment is processed automatically after placing the order.</p>
              </div>

              <button class="btn-place-order" [disabled]="placing()" (click)="placeOrder()">
                {{ placing() ? 'Placing order...' : 'Place Order' }}
              </button>
            </div>

          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 960px; margin: 0 auto; padding: 0 16px; }
    h2 { color: #1a5276; margin-bottom: 24px; }
    .checkout-layout { display: grid; grid-template-columns: 1fr 300px; gap: 24px; }
    .form-section, .summary-section { background: #fff; border-radius: 10px; padding: 24px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .form-section h3, .summary-section h3 { margin: 0 0 16px; color: #1a5276; font-size: 1rem; }
    .form-group { margin-bottom: 16px; }
    label { display: block; font-size: 0.9rem; font-weight: 500; color: #333; margin-bottom: 6px; }
    textarea { width: 100%; padding: 10px 14px; border: 1px solid #ddd; border-radius: 6px; font-size: 1rem; box-sizing: border-box; resize: vertical; font-family: inherit; }
    textarea:focus { outline: none; border-color: #2e86c1; }
    textarea.error { border-color: #e74c3c; }
    .error-msg { color: #e74c3c; font-size: 0.8rem; }
    .items-list { display: flex; flex-direction: column; gap: 8px; }
    .order-item { display: flex; align-items: center; gap: 8px; padding: 8px 0; border-bottom: 1px solid #f0f0f0; font-size: 0.9rem; }
    .item-name { flex: 1; color: #333; }
    .item-qty { color: #666; min-width: 40px; }
    .item-price { font-weight: 600; color: #1a5276; min-width: 80px; text-align: right; }
    .summary-row { display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #f0f0f0; font-size: 0.9rem; }
    .summary-row.total { font-size: 1.05rem; border-bottom: none; padding-top: 12px; }
    .free { color: #1e8449; font-weight: 500; }
    .payment-note { background: #d6eaf8; border-radius: 6px; padding: 10px 12px; margin: 16px 0; font-size: 0.85rem; color: #1a5276; }
    .payment-note p { margin: 0; }
    .btn-place-order { width: 100%; padding: 14px; background: #1e8449; color: #fff; border: none; border-radius: 6px; font-size: 1rem; font-weight: 700; cursor: pointer; transition: background 0.2s; }
    .btn-place-order:hover:not(:disabled) { background: #239b56; }
    .btn-place-order:disabled { opacity: 0.6; cursor: not-allowed; }
    .empty { background: #fff; border-radius: 10px; padding: 60px; text-align: center; color: #666; }
  `]
})
export class CheckoutComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly cart = signal<CartDto | null>(null);
  readonly loading = signal(true);
  readonly placing = signal(false);

  readonly form = this.fb.group({
    shippingAddress: ['', [Validators.required, Validators.minLength(10)]]
  });

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  ngOnInit(): void {
    this.cartService.getCart().subscribe({
      next: res => { if (res.success) this.cart.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  placeOrder(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    const cart = this.cart();
    const user = this.authService.currentUser();
    if (!cart || !user) return;

    this.placing.set(true);

    const dto = {
      shippingAddress: this.form.value.shippingAddress!,
      userEmail: user.email,
      items: cart.items.map(i => ({
        productId: i.productId,
        productName: i.productName,
        unitPrice: i.unitPrice,
        quantity: i.quantity
      }))
    };

    this.orderService.placeOrder(dto).subscribe({
      next: res => {
        if (res.success && res.data) {
          // Clear the cart after successful order
          this.cartService.clearCart().subscribe();
          this.toast.success('Order placed! Payment is being processed.');
          this.router.navigate(['/orders', res.data.id]);
        }
        this.placing.set(false);
      },
      error: () => this.placing.set(false)
    });
  }
}
