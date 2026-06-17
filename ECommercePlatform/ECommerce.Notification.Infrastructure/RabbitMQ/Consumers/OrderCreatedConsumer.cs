using ECommerce.Notification.Application.DTOs;
using ECommerce.Notification.Application.Features.Commands;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Notification.Infrastructure.RabbitMQ.Consumers;

/// <summary>
/// Receives OrderCreatedEvent → sends order confirmation email immediately.
/// This runs concurrently with PaymentService and InventoryService consumers
/// that are also listening on the same event.
///
/// The user gets "Your order has been placed!" email right away,
/// before payment is even processed. This is standard e-commerce UX.
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
            "NotificationService received OrderCreatedEvent for {Email}, Order {OrderId}",
            @event.UserEmail, @event.OrderId);

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var dto = new OrderConfirmationEmailDto
        {
            UserEmail = @event.UserEmail,
            OrderId = @event.OrderId,
            TotalAmount = @event.TotalAmount,
            OrderDate = @event.CreatedAt,
            Items = @event.Items.Select(i => new OrderEmailItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await mediator.Send(new SendOrderConfirmationEmailCommand(dto));
    }
}
