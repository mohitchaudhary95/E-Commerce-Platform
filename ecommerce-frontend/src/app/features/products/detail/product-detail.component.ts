import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CartService, InventoryService } from '../../../core/services/api.services';
import { ToastService } from '../../../core/services/toast.service';
import { ProductDto, InventoryDto } from '../../../core/models/models';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="page">
      <div class="container">
        <a routerLink="/products" class="back-link">← Back to Products</a>

        @if (loading()) {
          <div class="loading">Loading product...</div>
        } @else if (product()) {
          <div class="product-layout">

            <!-- Image -->
            <div class="image-section">
              <img [src]="product()!.imageUrl || 'https://placehold.co/500x400'"
                   [alt]="product()!.name" />
            </div>

            <!-- Info -->
            <div class="info-section">
              <span class="category-badge">{{ product()!.categoryName }}</span>
              <h1>{{ product()!.name }}</h1>
              <p class="price">₹{{ product()!.price | number:'1.0-0' }}</p>
              <p class="description">{{ product()!.description }}</p>

              <!-- Stock info -->
              @if (inventory()) {
                <div class="stock-info" [class.low]="inventory()!.isLowStock">
                  @if (inventory()!.availableQuantity > 0) {
                    <span>{{ inventory()!.isLowStock ? '⚠️' : '✅' }}
                      {{ inventory()!.availableQuantity }} in stock</span>
                  } @else {
                    <span>❌ Out of stock</span>
                  }
                </div>
              }

              <!-- Quantity + Add to Cart -->
              <div class="add-to-cart">
                <div class="qty-control">
                  <button (click)="qty > 1 && (qty = qty - 1)">−</button>
                  <input type="number" [(ngModel)]="qty" min="1" max="99" />
                  <button (click)="qty = qty + 1">+</button>
                </div>
                <button class="btn-add" (click)="addToCart()"
                  [disabled]="adding() || (inventory() && inventory()!.availableQuantity === 0)">
                  {{ adding() ? 'Adding...' : '🛒 Add to Cart' }}
                </button>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 1000px; margin: 0 auto; padding: 0 16px; }
    .back-link { color: #2e86c1; text-decoration: none; font-size: 0.9rem; display: inline-block; margin-bottom: 20px; }
    .product-layout { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; background: #fff; border-radius: 12px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .image-section img { width: 100%; border-radius: 8px; object-fit: cover; }
    .category-badge { background: #d6eaf8; color: #1a5276; font-size: 0.8rem; padding: 3px 10px; border-radius: 12px; display: inline-block; margin-bottom: 10px; }
    h1 { font-size: 1.8rem; color: #1a5276; margin-bottom: 8px; }
    .price { font-size: 2rem; font-weight: 700; color: #1a5276; margin: 0 0 16px; }
    .description { color: #555; line-height: 1.7; margin-bottom: 20px; }
    .stock-info { padding: 8px 14px; border-radius: 6px; background: #d5f5e3; color: #1e8449; font-size: 0.9rem; margin-bottom: 20px; display: inline-block; }
    .stock-info.low { background: #fef9e7; color: #b7770d; }
    .add-to-cart { display: flex; align-items: center; gap: 12px; }
    .qty-control { display: flex; align-items: center; border: 1px solid #ddd; border-radius: 6px; overflow: hidden; }
    .qty-control button { width: 36px; height: 44px; border: none; background: #f4f6f8; cursor: pointer; font-size: 1.2rem; }
    .qty-control input { width: 52px; height: 44px; border: none; text-align: center; font-size: 1rem; font-weight: 600; }
    .btn-add { padding: 12px 24px; background: #1a5276; color: #fff; border: none; border-radius: 6px; font-size: 1rem; font-weight: 600; cursor: pointer; transition: background 0.2s; }
    .btn-add:hover:not(:disabled) { background: #2e86c1; }
    .btn-add:disabled { opacity: 0.5; cursor: not-allowed; }
    .loading { background: #fff; border-radius: 10px; padding: 60px; text-align: center; color: #666; }
  `]
})
export class ProductDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly inventoryService = inject(InventoryService);
  private readonly toast = inject(ToastService);

  readonly product = signal<ProductDto | null>(null);
  readonly inventory = signal<InventoryDto | null>(null);
  readonly loading = signal(true);
  readonly adding = signal(false);
  qty = 1;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.productService.getById(id).subscribe({
      next: res => {
        if (res.success) {
          this.product.set(res.data);
          // Load inventory silently — no error if not found
          this.inventoryService.getByProduct(id).subscribe({
            next: inv => { if (inv.success) this.inventory.set(inv.data); }
          });
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  addToCart(): void {
    const p = this.product();
    if (!p) return;
    this.adding.set(true);
    this.cartService.addItem({ productId: p.id, quantity: this.qty }).subscribe({
      next: () => { this.toast.success(`${p.name} added to cart!`); this.adding.set(false); },
      error: () => this.adding.set(false)
    });
  }
}
