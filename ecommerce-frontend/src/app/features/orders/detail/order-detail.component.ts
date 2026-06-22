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
          <p>Loading order...</p>
        } @else if (order()) {
          <div class="order-detail">

            <!-- Header -->
            <div class="detail-header">
              <div>
                <h2>Order Details</h2>
                <p class="order-id">ID: {{ order()!.id }}</p>
                <p class="order-date">Placed on {{ order()!.createdAt | date:'dd MMM yyyy, HH:mm' }}</p>
              </div>
              <span class="status-badge" [class]="'status-' + order()!.status.toLowerCase()">
                {{ order()!.statusLabel }}
              </span>
            </div>

            <!-- Payment Status -->
            @if (payment()) {
              <div class="payment-card" [class]="'payment-' + payment()!.status.toLowerCase()">
                <h3>Payment Status</h3>
                <div class="payment-row">
                  <span>Status</span>
                  <strong>{{ payment()!.statusLabel }}</strong>
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
                <p>⏳ Processing payment... please wait.</p>
              </div>
            }

            <!-- Items -->
            <div class="section">
              <h3>Items Ordered</h3>
              <table class="items-table">
                <thead>
                  <tr><th>Product</th><th>Unit Price</th><th>Qty</th><th>Total</th></tr>
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
                    <td><strong>₹{{ order()!.totalAmount | number:'1.0-0' }}</strong></td>
                  </tr>
                </tfoot>
              </table>
            </div>

            <!-- Shipping -->
            <div class="section">
              <h3>Shipping Address</h3>
              <p class="address">{{ order()!.shippingAddress }}</p>
            </div>

            <!-- Cancel -->
            @if (order()!.status === 'Pending') {
              <button class="btn-cancel" (click)="cancelOrder()" [disabled]="cancelling()">
                {{ cancelling() ? 'Cancelling...' : 'Cancel Order' }}
              </button>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 800px; margin: 0 auto; padding: 0 16px; }
    .back-link { color: #2e86c1; text-decoration: none; font-size: 0.9rem; display: inline-block; margin-bottom: 16px; }
    .order-detail { background: #fff; border-radius: 10px; padding: 28px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .detail-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; padding-bottom: 20px; border-bottom: 1px solid #f0f0f0; }
    h2 { margin: 0 0 4px; color: #1a5276; }
    .order-id, .order-date { color: #888; font-size: 0.85rem; margin: 2px 0; }
    .status-badge { padding: 6px 14px; border-radius: 12px; font-size: 0.85rem; font-weight: 600; }
    .status-pending { background: #fef9e7; color: #b7770d; }
    .status-processing { background: #d6eaf8; color: #1a5276; }
    .status-completed { background: #d5f5e3; color: #1e8449; }
    .status-cancelled { background: #fadbd8; color: #922b21; }
    .payment-card { background: #f8f9fa; border-radius: 8px; padding: 16px; margin-bottom: 24px; border-left: 4px solid #aaa; }
    .payment-success { border-left-color: #1e8449; background: #d5f5e3; }
    .payment-failed { border-left-color: #922b21; background: #fadbd8; }
    .payment-pending { border-left-color: #b7770d; background: #fef9e7; }
    .payment-card h3 { margin: 0 0 12px; font-size: 0.95rem; }
    .payment-row { display: flex; justify-content: space-between; padding: 4px 0; font-size: 0.9rem; }
    .failure-reason { margin-top: 8px; color: #922b21; font-size: 0.9rem; }
    .section { margin-bottom: 24px; padding-bottom: 24px; border-bottom: 1px solid #f0f0f0; }
    .section h3 { color: #1a5276; margin: 0 0 12px; font-size: 0.95rem; }
    .items-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
    .items-table th { text-align: left; padding: 8px; border-bottom: 2px solid #f0f0f0; color: #555; font-weight: 600; }
    .items-table td { padding: 10px 8px; border-bottom: 1px solid #f8f8f8; }
    .items-table tfoot td { padding-top: 12px; font-size: 1rem; }
    .address { color: #444; line-height: 1.6; white-space: pre-wrap; }
    .btn-cancel { padding: 10px 24px; background: transparent; border: 1px solid #e74c3c; color: #e74c3c; border-radius: 6px; cursor: pointer; font-size: 0.9rem; transition: all 0.2s; }
    .btn-cancel:hover:not(:disabled) { background: #e74c3c; color: #fff; }
    .btn-cancel:disabled { opacity: 0.5; cursor: not-allowed; }
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

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.orderService.getById(id).subscribe({
      next: res => {
        if (res.success) {
          this.order.set(res.data);
          // Poll for payment status after placing order
          this.pollPaymentStatus(res.data!.id);
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
        if (res.success) { this.order.set(res.data); this.toast.success('Order cancelled.'); }
        this.cancelling.set(false);
      },
      error: () => this.cancelling.set(false)
    });
  }

  ngOnDestroy(): void { clearTimeout(this.pollTimer); }

  // Poll for payment status — RabbitMQ processing takes a few seconds
  private pollPaymentStatus(orderId: string): void {
    this.pollingPayment.set(true);
    this.paymentService.getByOrderId(orderId).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.payment.set(res.data);
          this.pollingPayment.set(false);
        } else if (this.pollCount < 8) {
          // Payment not ready yet — retry after 2 seconds
          this.pollCount++;
          this.pollTimer = setTimeout(() => this.pollPaymentStatus(orderId), 2000);
        } else {
          this.pollingPayment.set(false);
        }
      },
      error: () => {
        if (this.pollCount < 8) {
          this.pollCount++;
          this.pollTimer = setTimeout(() => this.pollPaymentStatus(orderId), 2000);
        } else {
          this.pollingPayment.set(false);
        }
      }
    });
  }
}
