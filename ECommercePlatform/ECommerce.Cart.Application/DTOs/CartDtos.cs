namespace ECommerce.Cart.Application.DTOs;

// ─── Cart Response DTOs ───────────────────────────────────────────────────────

public class CartDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

// ─── Cart Request DTOs ────────────────────────────────────────────────────────

public class AddToCartDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateQuantityDto
{
    public int Quantity { get; set; }
}

// ─── ProductService HTTP Response DTO ─────────────────────────────────────────

/// <summary>
/// This mirrors ProductDto from ProductService.
/// CartService calls ProductService over HTTP and deserializes the response into this.
///
/// Why duplicate the DTO instead of sharing it?
/// Because sharing DTOs between services creates tight coupling.
/// If ProductService changes its DTO, CartService shouldn't break.
/// Each service defines what IT needs from the response.
/// </summary>
public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Wrapper matching the ApiResponse<T> shape returned by ProductService.
/// We need this to correctly deserialize the HTTP response.
/// </summary>
public class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
