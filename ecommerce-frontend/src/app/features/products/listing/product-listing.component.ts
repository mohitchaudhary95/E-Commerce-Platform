import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CartService } from '../../../core/services/api.services';
import { ToastService } from '../../../core/services/toast.service';
import { ProductDto, CategoryDto, PagedResult } from '../../../core/models/models';

@Component({
  selector: 'app-product-listing',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="page">
      <div class="container">

        <!-- Filters -->
        <aside class="sidebar">
          <h3>Filter Products</h3>

          <div class="filter-group">
            <label>Search</label>
            <input [(ngModel)]="searchTerm" (ngModelChange)="onFilterChange()" placeholder="Search products..." />
          </div>

          <div class="filter-group">
            <label>Category</label>
            <select [(ngModel)]="selectedCategory" (ngModelChange)="onFilterChange()">
              <option value="">All Categories</option>
              @for (cat of categories(); track cat.id) {
                <option [value]="cat.id">{{ cat.name }} ({{ cat.productCount }})</option>
              }
            </select>
          </div>

          <div class="filter-group">
            <label>Price Range</label>
            <div class="price-range">
              <input type="number" [(ngModel)]="minPrice" (ngModelChange)="onFilterChange()" placeholder="Min ₹" />
              <span>–</span>
              <input type="number" [(ngModel)]="maxPrice" (ngModelChange)="onFilterChange()" placeholder="Max ₹" />
            </div>
          </div>

          <div class="filter-group">
            <label>Sort By</label>
            <select [(ngModel)]="sortBy" (ngModelChange)="onFilterChange()">
              <option value="">Newest First</option>
              <option value="price">Price: Low to High</option>
              <option value="price-desc">Price: High to Low</option>
              <option value="name">Name A-Z</option>
            </select>
          </div>

          <button class="btn-clear" (click)="clearFilters()">Clear Filters</button>
        </aside>

        <!-- Product Grid -->
        <main class="product-area">
          <div class="results-bar">
            <span>{{ paged()?.totalCount ?? 0 }} products found</span>
          </div>

          @if (loading()) {
            <div class="loading-grid">
              @for (i of [1,2,3,4,5,6]; track i) {
                <div class="skeleton-card"></div>
              }
            </div>
          } @else if (products().length === 0) {
            <div class="empty-state">
              <p>😕 No products found. Try adjusting your filters.</p>
            </div>
          } @else {
            <div class="product-grid">
              @for (product of products(); track product.id) {
                <div class="product-card">
                  <a [routerLink]="['/products', product.id]">
                    <img [src]="product.imageUrl || 'https://placehold.co/300x200'" [alt]="product.name" />
                    <div class="card-body">
                      <span class="category-badge">{{ product.categoryName }}</span>
                      <h4>{{ product.name }}</h4>
                      <p class="description">{{ product.description | slice:0:80 }}...</p>
                      <div class="card-footer">
                        <span class="price">₹{{ product.price | number:'1.0-0' }}</span>
                        <button class="btn-cart" (click)="addToCart($event, product)">
                          + Cart
                        </button>
                      </div>
                    </div>
                  </a>
                </div>
              }
            </div>

            <!-- Pagination -->
            @if (paged() && paged()!.totalPages > 1) {
              <div class="pagination">
                <button [disabled]="!paged()!.hasPreviousPage" (click)="changePage(currentPage() - 1)">‹ Prev</button>
                @for (p of getPageNumbers(); track p) {
                  <button [class.active]="p === currentPage()" (click)="changePage(p)">{{ p }}</button>
                }
                <button [disabled]="!paged()!.hasNextPage" (click)="changePage(currentPage() + 1)">Next ›</button>
              </div>
            }
          }
        </main>
      </div>
    </div>
  `,
  styles: [`
    .page { background: #f4f6f8; min-height: 100vh; padding: 24px 0; }
    .container { max-width: 1200px; margin: 0 auto; padding: 0 16px; display: grid; grid-template-columns: 240px 1fr; gap: 24px; }
    .sidebar { background: #fff; border-radius: 10px; padding: 20px; height: fit-content; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .sidebar h3 { margin: 0 0 16px; color: #1a5276; font-size: 1rem; }
    .filter-group { margin-bottom: 16px; }
    .filter-group label { display: block; font-size: 0.85rem; font-weight: 500; color: #555; margin-bottom: 6px; }
    .filter-group input, .filter-group select { width: 100%; padding: 8px 10px; border: 1px solid #ddd; border-radius: 6px; font-size: 0.9rem; box-sizing: border-box; }
    .price-range { display: flex; align-items: center; gap: 6px; }
    .price-range input { width: 0; flex: 1; }
    .btn-clear { width: 100%; padding: 8px; background: transparent; border: 1px solid #ddd; border-radius: 6px; cursor: pointer; color: #666; font-size: 0.9rem; }
    .btn-clear:hover { border-color: #1a5276; color: #1a5276; }
    .results-bar { margin-bottom: 16px; color: #666; font-size: 0.9rem; }
    .product-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
    .product-card { background: #fff; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.06); transition: transform 0.2s, box-shadow 0.2s; }
    .product-card:hover { transform: translateY(-3px); box-shadow: 0 6px 20px rgba(0,0,0,0.1); }
    .product-card a { text-decoration: none; color: inherit; display: block; }
    .product-card img { width: 100%; height: 160px; object-fit: cover; }
    .card-body { padding: 12px; }
    .category-badge { background: #d6eaf8; color: #1a5276; font-size: 0.75rem; padding: 2px 8px; border-radius: 12px; }
    .card-body h4 { margin: 8px 0 4px; font-size: 0.95rem; color: #222; }
    .description { font-size: 0.82rem; color: #666; margin: 0 0 10px; }
    .card-footer { display: flex; align-items: center; justify-content: space-between; }
    .price { font-size: 1.1rem; font-weight: 700; color: #1a5276; }
    .btn-cart { background: #1a5276; color: #fff; border: none; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 0.85rem; transition: background 0.2s; }
    .btn-cart:hover { background: #2e86c1; }
    .pagination { display: flex; justify-content: center; gap: 6px; margin-top: 24px; }
    .pagination button { padding: 8px 14px; border: 1px solid #ddd; background: #fff; border-radius: 6px; cursor: pointer; }
    .pagination button.active { background: #1a5276; color: #fff; border-color: #1a5276; }
    .pagination button:disabled { opacity: 0.4; cursor: not-allowed; }
    .skeleton-card { background: #e0e0e0; border-radius: 10px; height: 260px; animation: pulse 1.5s infinite; }
    .loading-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
    .empty-state { text-align: center; padding: 60px; color: #666; }
    @keyframes pulse { 0%,100% { opacity: 1; } 50% { opacity: 0.5; } }
  `]
})
export class ProductListingComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly toast = inject(ToastService);

  readonly products = signal<ProductDto[]>([]);
  readonly categories = signal<CategoryDto[]>([]);
  readonly paged = signal<PagedResult<ProductDto> | null>(null);
  readonly loading = signal(true);
  readonly currentPage = signal(1);

  searchTerm = '';
  selectedCategory = '';
  minPrice?: number;
  maxPrice?: number;
  sortBy = '';

  private debounceTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  onFilterChange(): void {
    // Debounce search to avoid spamming API
    clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => {
      this.currentPage.set(1);
      this.loadProducts();
    }, 400);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.minPrice = undefined;
    this.maxPrice = undefined;
    this.sortBy = '';
    this.currentPage.set(1);
    this.loadProducts();
  }

  changePage(page: number): void {
    this.currentPage.set(page);
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  addToCart(event: Event, product: ProductDto): void {
    event.preventDefault(); // Stop routerLink navigation
    event.stopPropagation();
    this.cartService.addItem({ productId: product.id, quantity: 1 }).subscribe({
      next: () => this.toast.success(`${product.name} added to cart!`)
    });
  }

  getPageNumbers(): number[] {
    const total = this.paged()?.totalPages ?? 1;
    const current = this.currentPage();
    const pages: number[] = [];
    for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) {
      pages.push(i);
    }
    return pages;
  }

  private loadProducts(): void {
    this.loading.set(true);
    const [sortField, sortDir] = this.parseSortBy();

    this.productService.getProducts({
      searchTerm: this.searchTerm || undefined,
      categoryId: this.selectedCategory || undefined,
      minPrice: this.minPrice,
      maxPrice: this.maxPrice,
      sortBy: sortField,
      sortDescending: sortDir,
      pageNumber: this.currentPage(),
      pageSize: 12
    }).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.products.set(res.data.items);
          this.paged.set(res.data);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: res => { if (res.success && res.data) this.categories.set(res.data); }
    });
  }

  private parseSortBy(): [string | undefined, boolean] {
    if (this.sortBy === 'price') return ['price', false];
    if (this.sortBy === 'price-desc') return ['price', true];
    if (this.sortBy === 'name') return ['name', false];
    return [undefined, false];
  }
}
