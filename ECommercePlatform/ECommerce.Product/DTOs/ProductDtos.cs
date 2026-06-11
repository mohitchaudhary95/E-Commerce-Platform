namespace ECommerce.Product.Application.DTOs;

// ─── Product DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Returned to clients when fetching product data.
/// Flattens the Category navigation into just the name — no need to send full Category object.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Sent by the client (Admin) when creating a new product.
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}

/// <summary>
/// Sent by the client (Admin) when updating an existing product.
/// All fields optional — only non-null fields are updated (partial update pattern).
/// </summary>
public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query parameters for filtering and paginating product listings.
/// All fields are optional — if not provided, no filter is applied.
/// </summary>
public class ProductFilterDto
{
    public string? SearchTerm { get; set; }       // Searches Name and Description
    public Guid? CategoryId { get; set; }          // Filter by category
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }            // "price", "name", "createdAt"
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ─── Category DTOs ─────────────────────────────────────────────────────────────

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }          // How many products in this category
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
