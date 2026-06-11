namespace ECommerce.Product.Domain.Entities;

/// <summary>
/// A product belongs to one category (e.g. Electronics, Clothing).
/// Simple one-to-many: one Category → many Products.
/// </summary>
public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — EF Core uses this to join Products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
