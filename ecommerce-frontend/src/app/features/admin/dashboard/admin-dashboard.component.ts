import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InventoryService, ProductService } from '../../../core/services/api.services';
import { ToastService } from '../../../core/services/toast.service';
import { InventoryDto, CategoryDto } from '../../../core/models/models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  template: `
    <div class="page">
      <div class="container">
        <h2>Admin Dashboard</h2>

        <!-- Tab Nav -->
        <div class="tabs">
          <button [class.active]="activeTab() === 'inventory'" (click)="activeTab.set('inventory')">Inventory</button>
          <button [class.active]="activeTab() === 'lowstock'" (click)="loadLowStock(); activeTab.set('lowstock')">
            Low Stock Alerts
            @if (lowStockCount() > 0) { <span class="alert-badge">{{ lowStockCount() }}</span> }
          </button>
          <button [class.active]="activeTab() === 'setstock'" (click)="activeTab.set('setstock')">Set Stock</button>
        </div>

        <!-- Inventory Tab -->
        @if (activeTab() === 'inventory') {
          <div class="tab-content">
            @if (loading()) { <p>Loading inventory...</p> }
            @else {
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Stock</th>
                    <th>Reserved</th>
                    <th>Available</th>
                    <th>Threshold</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of inventory(); track item.id) {
                    <tr>
                      <td><strong>{{ item.productName }}</strong></td>
                      <td>{{ item.stockQuantity }}</td>
                      <td>{{ item.reservedQuantity }}</td>
                      <td>{{ item.availableQuantity }}</td>
                      <td>{{ item.lowStockThreshold }}</td>
                      <td>
                        <span class="stock-badge" [class.low]="item.isLowStock">
                          {{ item.isLowStock ? '⚠️ Low' : '✅ OK' }}
                        </span>
                      </td>
                      <td>
                        <button class="btn-adjust" (click)="openAdjust(item)">Adjust</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        }

        <!-- Low Stock Tab -->
        @if (activeTab() === 'lowstock') {
          <div class="tab-content">
            @if (lowStock().length === 0) {
              <div class="empty-state">✅ All products are well-stocked!</div>
            } @else {
              <div class="alert-cards">
                @for (item of lowStock(); track item.id) {
                  <div class="alert-card">
                    <h4>{{ item.productName }}</h4>
                    <p>Available: <strong>{{ item.availableQuantity }}</strong> / Threshold: {{ item.lowStockThreshold }}</p>
                    <button class="btn-adjust" (click)="openAdjust(item)">Restock</button>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- Set Stock Tab -->
        @if (activeTab() === 'setstock') {
          <div class="tab-content form-tab">
            <h3>Set Product Stock</h3>
            <form [formGroup]="setStockForm" (ngSubmit)="setStock()">
              <div class="form-group">
                <label>Product ID</label>
                <input formControlName="productId" placeholder="Paste product GUID here" />
              </div>
              <div class="form-group">
                <label>Product Name</label>
                <input formControlName="productName" placeholder="Product name" />
              </div>
              <div class="form-group">
                <label>Initial Stock Quantity</label>
                <input type="number" formControlName="quantity" />
              </div>
              <div class="form-group">
                <label>Low Stock Threshold</label>
                <input type="number" formControlName="lowStockThreshold" />
              </div>
              <button type="submit" class="btn-primary" [disabled]="settingStock()">
                {{ settingStock() ? 'Setting...' : 'Set Stock' }}
              </button>
            </form>
          </div>
        }

        <!-- Adjust Stock Modal -->
        @if (adjustTarget()) {
          <div class="modal-overlay" (click)="adjustTarget.set(null)">
            <div class="modal" (click)="$event.stopPropagation()">
              <h3>Adjust Stock: {{ adjustTarget()!.productName }}</h3>
              <p>Current stock: <strong>{{ adjustTarget()!.stockQuantity }}</strong></p>
              <form [formGroup]="adjustForm" (ngSubmit)="submitAdjust()">
                <div class="form-group">
                  <label>Adjustment (+/−)</label>
                  <input type="number" formControlName="quantity" placeholder="e.g. 50 or -10" />
                </div>
                <div class="form-group">
                  <label>Reason</label>
                  <input formControlName="reason" placeholder="e.g. Restocked from supplier" />
                </div>
                <div class="modal-actions">
                  <button type="button" class="btn-cancel" (click)="adjustTarget.set(null)">Cancel</button>
                  <button type="submit" class="btn-primary" [disabled]="adjusting()">
                    {{ adjusting() ? 'Saving...' : 'Apply' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }

      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 1100px; margin: 0 auto; padding: 0 16px; }
    h2 { color: #1a5276; margin-bottom: 20px; }
    .tabs { display: flex; gap: 4px; margin-bottom: 20px; background: #fff; padding: 6px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); width: fit-content; }
    .tabs button { padding: 8px 20px; border: none; background: transparent; border-radius: 6px; cursor: pointer; font-size: 0.9rem; color: #666; position: relative; }
    .tabs button.active { background: #1a5276; color: #fff; }
    .alert-badge { background: #e74c3c; color: #fff; border-radius: 50%; width: 18px; height: 18px; font-size: 11px; display: inline-flex; align-items: center; justify-content: center; margin-left: 6px; font-weight: 700; }
    .tab-content { background: #fff; border-radius: 10px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .data-table { width: 100%; border-collapse: collapse; font-size: 0.88rem; }
    .data-table th { text-align: left; padding: 10px 12px; border-bottom: 2px solid #f0f0f0; color: #555; font-weight: 600; }
    .data-table td { padding: 10px 12px; border-bottom: 1px solid #f8f8f8; }
    .data-table tr:hover td { background: #fafbfc; }
    .stock-badge { padding: 3px 8px; border-radius: 12px; font-size: 0.8rem; background: #d5f5e3; color: #1e8449; }
    .stock-badge.low { background: #fef9e7; color: #b7770d; }
    .btn-adjust { padding: 4px 12px; background: #2e86c1; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 0.82rem; }
    .alert-cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 12px; }
    .alert-card { background: #fef9e7; border: 1px solid #f0c27f; border-radius: 8px; padding: 14px; }
    .alert-card h4 { margin: 0 0 6px; font-size: 0.9rem; color: #333; }
    .alert-card p { margin: 0 0 10px; font-size: 0.85rem; color: #666; }
    .form-tab { max-width: 480px; }
    .form-group { margin-bottom: 14px; }
    label { display: block; font-size: 0.85rem; font-weight: 500; color: #333; margin-bottom: 5px; }
    input { width: 100%; padding: 9px 12px; border: 1px solid #ddd; border-radius: 6px; font-size: 0.9rem; box-sizing: border-box; }
    .btn-primary { padding: 10px 24px; background: #1a5276; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-size: 0.9rem; font-weight: 600; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .empty-state { text-align: center; padding: 40px; color: #1e8449; font-size: 1.1rem; }
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 100; display: flex; align-items: center; justify-content: center; }
    .modal { background: #fff; border-radius: 12px; padding: 28px; width: 100%; max-width: 420px; }
    .modal h3 { margin: 0 0 6px; color: #1a5276; }
    .modal p { color: #666; margin-bottom: 16px; font-size: 0.9rem; }
    .modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
    .btn-cancel { padding: 8px 16px; background: transparent; border: 1px solid #ddd; border-radius: 6px; cursor: pointer; }
  `]
})
export class AdminDashboardComponent implements OnInit {
  private readonly inventoryService = inject(InventoryService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly activeTab = signal<'inventory' | 'lowstock' | 'setstock'>('inventory');
  readonly inventory = signal<InventoryDto[]>([]);
  readonly lowStock = signal<InventoryDto[]>([]);
  readonly lowStockCount = signal(0);
  readonly loading = signal(true);
  readonly adjustTarget = signal<InventoryDto | null>(null);
  readonly adjusting = signal(false);
  readonly settingStock = signal(false);

  readonly setStockForm = this.fb.group({
    productId: ['', Validators.required],
    productName: ['', Validators.required],
    quantity: [0, [Validators.required, Validators.min(0)]],
    lowStockThreshold: [10, Validators.required]
  });

  readonly adjustForm = this.fb.group({
    quantity: [0, Validators.required],
    reason: ['', Validators.required]
  });

  ngOnInit(): void {
    this.loadInventory();
    this.loadLowStockCount();
  }

  openAdjust(item: InventoryDto): void {
    this.adjustTarget.set(item);
    this.adjustForm.reset({ quantity: 0, reason: '' });
  }

  submitAdjust(): void {
    const target = this.adjustTarget();
    if (!target || this.adjustForm.invalid) return;
    this.adjusting.set(true);
    const { quantity, reason } = this.adjustForm.value;
    this.inventoryService.adjustStock(target.productId, { quantity: quantity!, reason: reason! }).subscribe({
      next: res => {
        if (res.success) {
          this.toast.success('Stock adjusted successfully!');
          this.adjustTarget.set(null);
          this.loadInventory();
        }
        this.adjusting.set(false);
      },
      error: () => this.adjusting.set(false)
    });
  }

  setStock(): void {
    if (this.setStockForm.invalid) return;
    this.settingStock.set(true);
    const v = this.setStockForm.value;
    this.inventoryService.setStock({
      productId: v.productId!, productName: v.productName!,
      quantity: v.quantity!, lowStockThreshold: v.lowStockThreshold!
    }).subscribe({
      next: res => {
        if (res.success) { this.toast.success('Stock set!'); this.setStockForm.reset(); this.loadInventory(); }
        this.settingStock.set(false);
      },
      error: () => this.settingStock.set(false)
    });
  }

  loadLowStock(): void {
    this.inventoryService.getLowStock().subscribe({
      next: res => { if (res.success && res.data) { this.lowStock.set(res.data); this.lowStockCount.set(res.data.length); } }
    });
  }

  private loadInventory(): void {
    this.loading.set(true);
    this.inventoryService.getAll().subscribe({
      next: res => { if (res.success && res.data) this.inventory.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  private loadLowStockCount(): void {
    this.inventoryService.getLowStock().subscribe({
      next: res => { if (res.success && res.data) this.lowStockCount.set(res.data.length); }
    });
  }
}
