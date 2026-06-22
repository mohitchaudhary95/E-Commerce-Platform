import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/api.services';
import { OrderDto, PagedResult } from '../../../core/models/models';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="container">
        <h2>My Orders</h2>

        @if (loading()) {
          <div class="loading">Loading your orders...</div>
        } @else if (orders().length === 0) {
          <div class="empty">
            <p>📦 You haven't placed any orders yet.</p>
            <a routerLink="/products" class="btn-primary">Start Shopping</a>
          </div>
        } @else {
          <div class="orders-list">
            @for (order of orders(); track order.id) {
              <div class="order-card">
                <div class="order-header">
                  <div>
                    <span class="order-id">Order #{{ order.id | slice:0:8 }}...</span>
                    <span class="order-date">{{ order.createdAt | date:'dd MMM yyyy, HH:mm' }}</span>
                  </div>
                  <span class="status-badge" [class]="'status-' + order.status.toLowerCase()">
                    {{ order.statusLabel }}
                  </span>
                </div>

                <div class="order-items">
                  @for (item of order.items.slice(0, 3); track item.id) {
                    <span class="item-chip">{{ item.productName }} × {{ item.quantity }}</span>
                  }
                  @if (order.items.length > 3) {
                    <span class="item-chip more">+{{ order.items.length - 3 }} more</span>
                  }
                </div>

                <div class="order-footer">
                  <span class="order-total">₹{{ order.totalAmount | number:'1.0-0' }}</span>
                  <a [routerLink]="['/orders', order.id]" class="btn-view">View Details →</a>
                </div>
              </div>
            }
          </div>

          <!-- Pagination -->
          @if (paged() && paged()!.totalPages > 1) {
            <div class="pagination">
              <button [disabled]="!paged()!.hasPreviousPage" (click)="changePage(currentPage() - 1)">‹ Prev</button>
              <span>Page {{ currentPage() }} of {{ paged()!.totalPages }}</span>
              <button [disabled]="!paged()!.hasNextPage" (click)="changePage(currentPage() + 1)">Next ›</button>
            </div>
          }
        }
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 800px; margin: 0 auto; padding: 0 16px; }
    h2 { color: #1a5276; margin-bottom: 24px; }
    .loading, .empty { background: #fff; border-radius: 10px; padding: 40px; text-align: center; color: #666; }
    .orders-list { display: flex; flex-direction: column; gap: 12px; }
    .order-card { background: #fff; border-radius: 10px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .order-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 12px; }
    .order-id { font-weight: 600; color: #1a5276; display: block; font-size: 0.95rem; }
    .order-date { color: #888; font-size: 0.82rem; }
    .status-badge { padding: 4px 12px; border-radius: 12px; font-size: 0.82rem; font-weight: 600; }
    .status-pending { background: #fef9e7; color: #b7770d; }
    .status-processing { background: #d6eaf8; color: #1a5276; }
    .status-completed { background: #d5f5e3; color: #1e8449; }
    .status-cancelled { background: #fadbd8; color: #922b21; }
    .order-items { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 12px; }
    .item-chip { background: #f4f6f8; padding: 3px 10px; border-radius: 12px; font-size: 0.82rem; color: #555; }
    .item-chip.more { color: #888; }
    .order-footer { display: flex; justify-content: space-between; align-items: center; }
    .order-total { font-size: 1.1rem; font-weight: 700; color: #1a5276; }
    .btn-view { color: #2e86c1; text-decoration: none; font-size: 0.9rem; font-weight: 500; }
    .btn-primary { display: inline-block; padding: 10px 24px; background: #1a5276; color: #fff; text-decoration: none; border-radius: 6px; margin-top: 16px; }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 24px; }
    .pagination button { padding: 8px 16px; border: 1px solid #ddd; background: #fff; border-radius: 6px; cursor: pointer; }
    .pagination button:disabled { opacity: 0.4; cursor: not-allowed; }
    .pagination span { color: #666; font-size: 0.9rem; }
  `]
})
export class OrderHistoryComponent implements OnInit {
  private readonly orderService = inject(OrderService);

  readonly orders = signal<OrderDto[]>([]);
  readonly paged = signal<PagedResult<OrderDto> | null>(null);
  readonly loading = signal(true);
  readonly currentPage = signal(1);

  ngOnInit(): void { this.loadOrders(); }

  changePage(page: number): void {
    this.currentPage.set(page);
    this.loadOrders();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private loadOrders(): void {
    this.loading.set(true);
    this.orderService.getHistory(this.currentPage(), 10).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.orders.set(res.data.items);
          this.paged.set(res.data);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
