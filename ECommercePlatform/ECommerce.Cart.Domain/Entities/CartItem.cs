namespace ECommerce.Cart.Domain.Entities;

/// <summary>
/// One line item inside a cart.
///
/// Key fields:
/// - ProductId: reference to ProductService (across service boundary — no FK in DB)
/// - ProductName/UnitPrice: SNAPSHOT of product data at the time of adding.
///   We store these locally so CartService doesn't need to call ProductService
///   every time we display the cart. Only called once on Add.
/// </summary>
public class CartItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Cross-service reference — no DB foreign key (different databases!)
    public Guid ProductId { get; set; }

    // Snapshot of product data at time of adding
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    // Foreign key to Cart
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    // Computed — quantity × price. Not stored.
    public decimal TotalPrice => Quantity * UnitPrice;
}
