namespace ECommerce.Cart.Domain.Entities;

/// <summary>
/// A Cart belongs to one user. One user = one active cart at a time.
///
/// Design decision: Cart has its OWN copy of price at time of adding.
/// Why? Because if a product's price changes after being added to cart,
/// the cart should still show the price the user saw when they added it.
/// This is standard e-commerce behavior (Amazon does this too).
/// </summary>
public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    // Computed — sum of all item totals. Not stored in DB.
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public int TotalItems => Items.Sum(i => i.Quantity);
}
