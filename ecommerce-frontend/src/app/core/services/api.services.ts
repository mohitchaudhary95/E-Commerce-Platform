import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, PagedResult, ProductDto, CategoryDto,
  ProductFilter, CreateProductDto, CartDto, AddToCartDto,
  UpdateQuantityDto, OrderDto, PlaceOrderDto, PaymentDto,
  InventoryDto, SetStockDto, AdjustStockDto
} from '../models/models';

// ── Product Service ──────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/products`;
  private readonly categoryApi = `${environment.apiUrl}/categories`;

  getProducts(filter: ProductFilter): Observable<ApiResponse<PagedResult<ProductDto>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber)
      .set('pageSize', filter.pageSize);
    if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
    if (filter.categoryId) params = params.set('categoryId', filter.categoryId);
    if (filter.minPrice != null) params = params.set('minPrice', filter.minPrice);
    if (filter.maxPrice != null) params = params.set('maxPrice', filter.maxPrice);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
    if (filter.sortDescending) params = params.set('sortDescending', filter.sortDescending);
    return this.http.get<ApiResponse<PagedResult<ProductDto>>>(this.api, { params });
  }

  getById(id: string): Observable<ApiResponse<ProductDto>> {
    return this.http.get<ApiResponse<ProductDto>>(`${this.api}/${id}`);
  }

  getCategories(): Observable<ApiResponse<CategoryDto[]>> {
    return this.http.get<ApiResponse<CategoryDto[]>>(this.categoryApi);
  }

  create(dto: CreateProductDto): Observable<ApiResponse<ProductDto>> {
    return this.http.post<ApiResponse<ProductDto>>(this.api, dto);
  }

  update(id: string, dto: Partial<CreateProductDto>): Observable<ApiResponse<ProductDto>> {
    return this.http.put<ApiResponse<ProductDto>>(`${this.api}/${id}`, dto);
  }

  delete(id: string): Observable<ApiResponse<object>> {
    return this.http.delete<ApiResponse<object>>(`${this.api}/${id}`);
  }
}

// ── Cart Service ─────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/cart`;

  // Cart count signal — shown in navbar badge
  readonly itemCount = signal<number>(0);

  getCart(): Observable<ApiResponse<CartDto>> {
    return this.http.get<ApiResponse<CartDto>>(this.api).pipe(
      tap(res => this.itemCount.set(res.data?.totalItems ?? 0))
    );
  }

  addItem(dto: AddToCartDto): Observable<ApiResponse<CartDto>> {
    return this.http.post<ApiResponse<CartDto>>(`${this.api}/items`, dto).pipe(
      tap(res => this.itemCount.set(res.data?.totalItems ?? 0))
    );
  }

  updateQuantity(itemId: string, dto: UpdateQuantityDto): Observable<ApiResponse<CartDto>> {
    return this.http.put<ApiResponse<CartDto>>(`${this.api}/items/${itemId}`, dto).pipe(
      tap(res => this.itemCount.set(res.data?.totalItems ?? 0))
    );
  }

  removeItem(itemId: string): Observable<ApiResponse<CartDto>> {
    return this.http.delete<ApiResponse<CartDto>>(`${this.api}/items/${itemId}`).pipe(
      tap(res => this.itemCount.set(res.data?.totalItems ?? 0))
    );
  }

  clearCart(): Observable<ApiResponse<object>> {
    return this.http.delete<ApiResponse<object>>(this.api).pipe(
      tap(() => this.itemCount.set(0))
    );
  }
}

// ── Order Service ─────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/orders`;

  placeOrder(dto: PlaceOrderDto): Observable<ApiResponse<OrderDto>> {
    return this.http.post<ApiResponse<OrderDto>>(this.api, dto);
  }

  getById(id: string): Observable<ApiResponse<OrderDto>> {
    return this.http.get<ApiResponse<OrderDto>>(`${this.api}/${id}`);
  }

  getHistory(pageNumber = 1, pageSize = 10): Observable<ApiResponse<PagedResult<OrderDto>>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<ApiResponse<PagedResult<OrderDto>>>(`${this.api}/history`, { params });
  }

  cancel(id: string): Observable<ApiResponse<OrderDto>> {
    return this.http.post<ApiResponse<OrderDto>>(`${this.api}/${id}/cancel`, {});
  }
}

// ── Payment Service ───────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/payments`;

  getByOrderId(orderId: string): Observable<ApiResponse<PaymentDto>> {
    return this.http.get<ApiResponse<PaymentDto>>(`${this.api}/order/${orderId}`);
  }
}

// ── Inventory Service ─────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/inventory`;

  getAll(): Observable<ApiResponse<InventoryDto[]>> {
    return this.http.get<ApiResponse<InventoryDto[]>>(this.api);
  }

  getLowStock(): Observable<ApiResponse<InventoryDto[]>> {
    return this.http.get<ApiResponse<InventoryDto[]>>(`${this.api}/low-stock`);
  }

  getByProduct(productId: string): Observable<ApiResponse<InventoryDto>> {
    return this.http.get<ApiResponse<InventoryDto>>(`${this.api}/product/${productId}`);
  }

  setStock(dto: SetStockDto): Observable<ApiResponse<InventoryDto>> {
    return this.http.post<ApiResponse<InventoryDto>>(`${this.api}/set`, dto);
  }

  adjustStock(productId: string, dto: AdjustStockDto): Observable<ApiResponse<InventoryDto>> {
    return this.http.patch<ApiResponse<InventoryDto>>(`${this.api}/product/${productId}/adjust`, dto);
  }
}
