namespace ECommerce.Product.Domain.Entities;

/// <summary>
/// Core Product entity stored in ProductDb.
///
/// Design decisions:
/// - StockQuantity lives in InventoryService's DB, NOT here.
///   ProductService only knows about product details (name, price, description).
///   This is the microservices principle: each service owns its own data.
/// - Price uses decimal — NEVER use float/double for money (floating point errors).
/// - ImageUrl is a simple string path — in production this would be a CDN URL.
/// </summary>
public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign key to Category
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
