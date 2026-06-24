import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { OrderService, PaymentService } from '../../../core/services/api.services';
import { ToastService } from '../../../core/services/toast.service';
import { OrderDto, PaymentDto } from '../../../core/models/models';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="container">

        <a routerLink="/orders" class="back-link">← Back to Orders</a>

        @if (loading()) {
          <p class="loading-msg">Loading order...</p>
        } @else if (order()) {
          <div class="order-detail">

            <!-- Header -->
            <div class="detail-header">
              <div>
                <h2>Order Details</h2>
                <p class="order-id">ID: {{ order()!.id }}</p>
                <p class="order-date">
                  Placed on {{ order()!.createdAt | date:'dd MMM yyyy, HH:mm' }}
                </p>
              </div>
              <!-- FIX: getStatusLabel() safely converts both string and number status -->
              <span class="status-badge"
                [class]="'status-' + getStatusClass(order()!.status)">
                {{ getStatusLabel(order()!.status) }}
              </span>
            </div>

            <!-- Payment Status -->
            @if (payment()) {
              <div class="payment-card"
                [class]="'payment-' + (payment()!.status?.toString()?.toLowerCase() || 'unknown')">
                <h3>Payment Status</h3>
                <div class="payment-row">
                  <span>Status</span>
                  <strong>{{ payment()!.statusLabel || payment()!.status }}</strong>
                </div>
                <div class="payment-row">
                  <span>Amount</span>
                  <strong>₹{{ payment()!.amount | number:'1.0-0' }}</strong>
                </div>
                @if (payment()!.cardLastFour) {
                  <div class="payment-row">
                    <span>Card</span>
                    <strong>•••• {{ payment()!.cardLastFour }}</strong>
                  </div>
                }
                @if (payment()!.failureReason) {
                  <div class="failure-reason">
                    ❌ {{ payment()!.failureReason }}
                  </div>
                }
              </div>
            } @else if (pollingPayment()) {
              <div class="payment-card payment-pending">
                <h3>Payment Status</h3>
                <p>⏳ Payment is being processed... please wait.</p>
              </div>
            } @else {
              <div class="payment-card payment-unknown">
                <h3>Payment Status</h3>
                <p>Payment information not yet available.</p>
              </div>
            }

            <!-- Items -->
            <div class="section">
              <h3>Items Ordered</h3>
              @if (order()!.items && order()!.items.length > 0) {
                <table class="items-table">
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>Unit Price</th>
                      <th>Qty</th>
                      <th>Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (item of order()!.items; track item.id) {
                      <tr>
                        <td>{{ item.productName }}</td>
                        <td>₹{{ item.unitPrice | number:'1.0-0' }}</td>
                        <td>{{ item.quantity }}</td>
                        <td><strong>₹{{ item.totalPrice | number:'1.0-0' }}</strong></td>
                      </tr>
                    }
                  </tbody>
                  <tfoot>
                    <tr>
                      <td colspan="3"><strong>Total</strong></td>
                      <td>
                        <strong>₹{{ order()!.totalAmount | number:'1.0-0' }}</strong>
                      </td>
                    </tr>
                  </tfoot>
                </table>
              } @else {
                <p class="no-items">No items found.</p>
              }
            </div>

            <!-- Shipping -->
            <div class="section">
              <h3>Shipping Address</h3>
              <p class="address">{{ order()!.shippingAddress }}</p>
            </div>

            <!-- Cancel -->
            @if (isPending(order()!.status)) {
              <button class="btn-cancel"
                (click)="cancelOrder()"
                [disabled]="cancelling()">
                {{ cancelling() ? 'Cancelling...' : 'Cancel Order' }}
              </button>
            }

          </div>
        } @else {
          <div class="not-found">
            <p>Order not found.</p>
            <a routerLink="/orders" class="btn-back">Back to Orders</a>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 800px; margin: 0 auto; padding: 0 16px; }
    .back-link { color: #2e86c1; text-decoration: none; font-size: 0.9rem;
      display: inline-block; margin-bottom: 16px; }
    .loading-msg { color: #666; padding: 40px; text-align: center; }
    .order-detail { background: #fff; border-radius: 10px; padding: 28px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .detail-header { display: flex; justify-content: space-between;
      align-items: flex-start; margin-bottom: 24px;
      padding-bottom: 20px; border-bottom: 1px solid #f0f0f0; }
    h2 { margin: 0 0 4px; color: #1a5276; }
    .order-id, .order-date { color: #888; font-size: 0.85rem; margin: 2px 0; }
    .status-badge { padding: 6px 14px; border-radius: 12px;
      font-size: 0.85rem; font-weight: 600; white-space: nowrap; }
    .status-pending    { background: #fef9e7; color: #b7770d; }
    .status-processing { background: #d6eaf8; color: #1a5276; }
    .status-completed  { background: #d5f5e3; color: #1e8449; }
    .status-cancelled  { background: #fadbd8; color: #922b21; }
    .status-unknown    { background: #f0f0f0; color: #666; }
    .payment-card { background: #f8f9fa; border-radius: 8px; padding: 16px;
      margin-bottom: 24px; border-left: 4px solid #aaa; }
    .payment-success { border-left-color: #1e8449; background: #d5f5e3; }
    .payment-failed  { border-left-color: #922b21; background: #fadbd8; }
    .payment-pending { border-left-color: #b7770d; background: #fef9e7; }
    .payment-unknown { border-left-color: #aaa; background: #f8f9fa; }
    .payment-card h3 { margin: 0 0 12px; font-size: 0.95rem; }
    .payment-card p  { margin: 0; color: #666; }
    .payment-row { display: flex; justify-content: space-between;
      padding: 4px 0; font-size: 0.9rem; }
    .failure-reason { margin-top: 8px; color: #922b21; font-size: 0.9rem; }
    .section { margin-bottom: 24px; padding-bottom: 24px;
      border-bottom: 1px solid #f0f0f0; }
    .section h3 { color: #1a5276; margin: 0 0 12px; font-size: 0.95rem; }
    .items-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
    .items-table th { text-align: left; padding: 8px;
      border-bottom: 2px solid #f0f0f0; color: #555; font-weight: 600; }
    .items-table td { padding: 10px 8px; border-bottom: 1px solid #f8f8f8; }
    .items-table tfoot td { padding-top: 12px; font-size: 1rem; }
    .no-items { color: #888; font-style: italic; }
    .address { color: #444; line-height: 1.6; white-space: pre-wrap; }
    .btn-cancel { padding: 10px 24px; background: transparent;
      border: 1px solid #e74c3c; color: #e74c3c; border-radius: 6px;
      cursor: pointer; font-size: 0.9rem; transition: all 0.2s; }
    .btn-cancel:hover:not(:disabled) { background: #e74c3c; color: #fff; }
    .btn-cancel:disabled { opacity: 0.5; cursor: not-allowed; }
    .not-found { text-align: center; padding: 60px; color: #666; }
    .btn-back { display: inline-block; margin-top: 12px; padding: 8px 20px;
      background: #1a5276; color: #fff; text-decoration: none;
      border-radius: 6px; }
  `]
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  private readonly orderService = inject(OrderService);
  private readonly paymentService = inject(PaymentService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  readonly order = signal<OrderDto | null>(null);
  readonly payment = signal<PaymentDto | null>(null);
  readonly loading = signal(true);
  readonly cancelling = signal(false);
  readonly pollingPayment = signal(false);

  private pollTimer?: ReturnType<typeof setTimeout>;
  private pollCount = 0;
  private readonly MAX_POLLS = 20;  // poll up to 20 times = ~60 seconds

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.loading.set(false); return; }

    this.orderService.getById(id).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.order.set(res.data);
          this.startPaymentPolling(res.data.id);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  cancelOrder(): void {
    const id = this.order()?.id;
    if (!id) return;
    this.cancelling.set(true);
    this.orderService.cancel(id).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.order.set(res.data);
          this.toast.success('Order cancelled.');
        }
        this.cancelling.set(false);
      },
      error: () => this.cancelling.set(false)
    });
  }

  ngOnDestroy(): void {
    if (this.pollTimer) clearTimeout(this.pollTimer);
  }

  // ── Status helpers — safely handle both string and numeric enum ─────────────

  /**
   * Backend may return status as string ("Pending") OR number (0).
   * This normalises both to a lowercase string for CSS class binding.
   */
  getStatusClass(status: any): string {
    if (status == null) return 'unknown';
    const map: Record<number, string> = {
      0: 'pending', 1: 'processing', 2: 'completed', 3: 'cancelled'
    };
    if (typeof status === 'number') return map[status] ?? 'unknown';
    return String(status).toLowerCase();
  }

  getStatusLabel(status: any): string {
    if (status == null) return 'Unknown';
    const map: Record<number, string> = {
      0: 'Pending', 1: 'Processing', 2: 'Completed', 3: 'Cancelled'
    };
    if (typeof status === 'number') return map[status] ?? 'Unknown';
    // Capitalise first letter if it's already a string
    const s = String(status);
    return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
  }

  isPending(status: any): boolean {
    if (status == null) return false;
    if (typeof status === 'number') return status === 0;
    return String(status).toLowerCase() === 'pending';
  }

  // ── Payment polling ─────────────────────────────────────────────────────────

  private startPaymentPolling(orderId: string): void {
    this.pollCount = 0;
    this.pollingPayment.set(true);
    this.pollOnce(orderId);
  }

  private pollOnce(orderId: string): void {
    this.paymentService.getByOrderId(orderId).subscribe({
      next: res => {
        if (res.success && res.data) {
          // Payment found — stop polling
          this.payment.set(res.data);
          this.pollingPayment.set(false);
        } else {
          // success:false means payment not yet processed — keep polling
          this.scheduleNextPoll(orderId);
        }
      },
      error: (err) => {
        // Network errors or server errors — retry a few times
        // Stop only on auth errors (401/403)
        if (err?.status === 401 || err?.status === 403) {
          this.pollingPayment.set(false);
        } else {
          this.scheduleNextPoll(orderId);
        }
      }
    });
  }

  private scheduleNextPoll(orderId: string): void {
    this.pollCount++;
    if (this.pollCount >= this.MAX_POLLS) {
      // Gave up after ~20 seconds
      this.pollingPayment.set(false);
      return;
    }
    // Wait 3 seconds then try again
    this.pollTimer = setTimeout(() => this.pollOnce(orderId), 3000);
  }
}