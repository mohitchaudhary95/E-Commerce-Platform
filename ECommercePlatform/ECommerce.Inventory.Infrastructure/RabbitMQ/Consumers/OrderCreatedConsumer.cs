using ECommerce.Inventory.Application.Features.Commands;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Inventory.Infrastructure.RabbitMQ.Consumers;

/// <summary>
/// Listens on the SAME "order-created" queue as PaymentService's consumer.
///
/// Important: Both PaymentService AND InventoryService consume from "order-created".
/// RabbitMQ delivers the message to EACH consumer independently — both get a copy.
/// This is the fan-out pattern via competing consumers on different services.
///
/// When OrderService publishes one OrderCreatedEvent:
///   → PaymentService consumer receives it → processes payment
///   → InventoryService consumer receives it → deducts stock
///   → NotificationService consumer receives it → sends email
/// All three happen concurrently, independently.
/// </summary>
public class OrderCreatedConsumer : RabbitMQConsumerBase<OrderCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    protected override string QueueName => QueueNames.OrderCreated;

    public OrderCreatedConsumer(
        IOptions<RabbitMQSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderCreatedConsumer> logger)
        : base(settings, logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation(
            "InventoryService received OrderCreatedEvent: OrderId={OrderId}, Items={Count}",
            @event.OrderId, @event.Items.Count);

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var orderItems = @event.Items
            .Select(i => new OrderLineItem(i.ProductId, i.ProductName, i.Quantity))
            .ToList();

        await mediator.Send(new DeductStockForOrderCommand(@event.OrderId, orderItems));
    }
}
