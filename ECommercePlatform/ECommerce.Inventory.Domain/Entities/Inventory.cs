namespace ECommerce.Inventory.Domain.Entities;

/// <summary>
/// Tracks stock levels for products. Lives in InventoryDb — separate from ProductDb.
///
/// Why separate from Product?
/// Stock changes extremely frequently (every order, every restock).
/// Product details change rarely. Separating them means the product listing
/// query never competes with high-frequency inventory writes.
///
/// ProductId is a cross-service reference — no FK constraint.
/// </summary>
public class Inventory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to ProductService — no FK in this DB
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; } // Future: reserve before confirming
    public int LowStockThreshold { get; set; } = 10; // Alert when stock drops below this

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Computed — actual available stock
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public bool IsLowStock => AvailableQuantity <= LowStockThreshold;
}
