// ── API wrapper (matches backend ApiResponse<T>) ──────────────────────────────
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
  statusCode: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ── Auth / Identity ───────────────────────────────────────────────────────────
export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface TokenResponseDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: 'Customer' | 'Admin';
  createdAt: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

// ── Product ───────────────────────────────────────────────────────────────────
export interface ProductDto {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  isActive: boolean;
  categoryId: string;
  categoryName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  productCount: number;
}

export interface ProductFilter {
  searchTerm?: string;
  categoryId?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  categoryId: string;
}

// ── Cart ──────────────────────────────────────────────────────────────────────
export interface CartDto {
  id: string;
  userId: string;
  items: CartItemDto[];
  totalAmount: number;
  totalItems: number;
  updatedAt: string;
}

export interface CartItemDto {
  id: string;
  productId: string;
  productName: string;
  productImageUrl: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface AddToCartDto {
  productId: string;
  quantity: number;
}

export interface UpdateQuantityDto {
  quantity: number;
}

// ── Order ─────────────────────────────────────────────────────────────────────
export type OrderStatus = 'Pending' | 'Processing' | 'Completed' | 'Cancelled';

export interface OrderDto {
  id: string;
  userId: string;
  userEmail: string;
  shippingAddress: string;
  totalAmount: number;
  status: OrderStatus;
  statusLabel: string;
  createdAt: string;
  updatedAt?: string;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface PlaceOrderDto {
  shippingAddress: string;
  userEmail: string;
  items: PlaceOrderItemDto[];
}

export interface PlaceOrderItemDto {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

// ── Payment ───────────────────────────────────────────────────────────────────
export type PaymentStatus = 'Pending' | 'Success' | 'Failed';

export interface PaymentDto {
  id: string;
  orderId: string;
  userId: string;
  amount: number;
  status: PaymentStatus;
  statusLabel: string;
  failureReason?: string;
  cardLastFour?: string;
  createdAt: string;
  processedAt?: string;
}

// ── Inventory ─────────────────────────────────────────────────────────────────
export interface InventoryDto {
  id: string;
  productId: string;
  productName: string;
  stockQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  lowStockThreshold: number;
  isLowStock: boolean;
  updatedAt: string;
}

export interface SetStockDto {
  productId: string;
  productName: string;
  quantity: number;
  lowStockThreshold: number;
}

export interface AdjustStockDto {
  quantity: number;
  reason: string;
}
