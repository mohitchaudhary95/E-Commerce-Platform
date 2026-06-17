using ECommerce.Shared.RabbitMQ.Abstractions;
using ECommerce.Inventory.Application.Interfaces;
using DomainInventory = ECommerce.Inventory.Domain.Entities.Inventory;
using ECommerce.Inventory.Domain.Entities;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommerce.Inventory.Infrastructure.RabbitMQ.Publishers;

public class InventoryUpdatedPublisher : IInventoryEventPublisher
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<InventoryUpdatedPublisher> _logger;

    public InventoryUpdatedPublisher(IEventPublisher publisher, ILogger<InventoryUpdatedPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishInventoryUpdatedAsync(
        Inventory inventory, string reason, CancellationToken cancellationToken = default)
    {
        var @event = new DomainInventoryUpdatedEvent
        {
            ProductId = inventory.ProductId,
            ProductName = inventory.ProductName,
            NewStock = inventory.StockQuantity,
            UpdateReason = reason,
            UpdatedAt = inventory.UpdatedAt
        };

        await _publisher.PublishAsync(@event, QueueNames.InventoryUpdated, cancellationToken);

        _logger.LogInformation(
            "Published InventoryUpdatedEvent for Product {ProductId}: Stock={Stock}, Reason={Reason}",
            inventory.ProductId, inventory.StockQuantity, reason);
    }
}




