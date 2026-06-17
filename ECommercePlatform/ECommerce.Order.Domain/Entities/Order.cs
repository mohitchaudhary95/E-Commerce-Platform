using ECommerce.Order.Domain.Enums;

namespace ECommerce.Order.Domain.Entities;

/// <summary>
/// An Order is a confirmed intent to purchase.
/// Created when the user clicks "Place Order" from their cart.
///
/// Key design decisions:
/// - TotalAmount is stored on the Order (not computed from items) because
///   prices could change after the order is placed. We lock in the total at order time.
/// - UserEmail is stored directly — OrderService doesn't call IdentityService to
///   look it up. The email was passed in when the order was created and is needed
///   by NotificationService (via the OrderCreatedEvent).
/// - ShippingAddress is a simple string for now. Production would use a value object.
/// </summary>
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
