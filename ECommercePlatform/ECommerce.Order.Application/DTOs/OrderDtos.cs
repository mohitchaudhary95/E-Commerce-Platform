using ECommerce.Order.Domain.Enums;

namespace ECommerce.Order.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusLabel => Status.ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// What the client sends when placing an order.
/// Items come from the user's cart — the frontend passes them through.
/// In a stricter system, OrderService would fetch the cart directly,
/// but passing items avoids a synchronous cart→order dependency.
/// </summary>
public class PlaceOrderDto
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<PlaceOrderItemDto> Items { get; set; } = new();
}

public class PlaceOrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
