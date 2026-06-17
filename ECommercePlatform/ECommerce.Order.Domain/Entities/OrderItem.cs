namespace ECommerce.Order.Domain.Entities;

/// <summary>
/// A line item within an order.
/// Snapshot of product data at the time of ordering — same reasoning as CartItem.
/// ProductId is a cross-service reference (no DB foreign key).
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal TotalPrice => UnitPrice * Quantity;
}
