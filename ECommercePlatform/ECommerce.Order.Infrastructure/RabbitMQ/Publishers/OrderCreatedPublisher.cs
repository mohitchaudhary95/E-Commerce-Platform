using ECommerce.Order.Application.Interfaces;
using DomainOrder = ECommerce.Order.Domain.Entities.Order;
using ECommerce.Order.Domain.Entities;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Infrastructure.RabbitMQ.Publishers;

/// <summary>
/// Implements IOrderEventPublisher using the shared RabbitMQ publisher.
///
/// Responsibility: build the event payload from the Order entity and send it.
/// The Application layer only knows about IOrderEventPublisher — it never
/// touches RabbitMQ directly. This keeps the Application layer clean.
///
/// The event carries enough data so consumers don't need to call back:
///   - PaymentService needs: OrderId, UserId, TotalAmount
///   - InventoryService needs: OrderId, Items (ProductId + Quantity)
///   - NotificationService needs: UserEmail, OrderId, Items, TotalAmount
/// All of that is in one OrderCreatedEvent — one publish, three consumers.
/// </summary>
public class OrderCreatedPublisher : IOrderEventPublisher
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<OrderCreatedPublisher> _logger;

    public OrderCreatedPublisher(IEventPublisher publisher, ILogger<OrderCreatedPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishOrderCreatedAsync(DomainOrder order, CancellationToken cancellationToken = default)
    {
        var @event = new DomainOrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            UserEmail = order.UserEmail,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new DomainOrderCreatedItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _publisher.PublishAsync(@event, QueueNames.OrderCreated, cancellationToken);

        _logger.LogInformation(
            "Published OrderCreatedEvent for Order {OrderId} with {ItemCount} items, Total: {Total:C}",
            order.Id, order.Items.Count, order.TotalAmount);
    }
}

