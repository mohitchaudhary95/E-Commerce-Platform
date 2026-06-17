using ECommerce.Order.Application.Features.Commands;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Order.Infrastructure.RabbitMQ.Consumers;

/// <summary>
/// Listens on the "payment-completed" queue and updates order status accordingly.
///
/// Why IServiceScopeFactory instead of injecting IMediator directly?
/// This class is a BackgroundService (singleton lifetime).
/// IMediator and IOrderRepository are Scoped (created per request).
/// You CANNOT inject a Scoped service into a Singleton — it creates a "captive dependency"
/// bug where the same DbContext is reused across requests (data corruption risk).
///
/// Solution: inject IServiceScopeFactory (singleton-safe) and create a new scope
/// per message. Each message gets its own fresh DbContext. ✓
/// </summary>
public class PaymentCompletedConsumer : RabbitMQConsumerBase<PaymentCompletedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    protected override string QueueName => QueueNames.PaymentCompleted;

    public PaymentCompletedConsumer(
        IOptions<RabbitMQSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentCompletedConsumer> logger)
        : base(settings, logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(PaymentCompletedEvent @event)
    {
        _logger.LogInformation(
            "Received PaymentCompletedEvent: OrderId={OrderId}, Success={Success}",
            @event.OrderId, @event.IsSuccess);

        // Create a fresh DI scope for this message — gets its own DbContext
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Dispatch to UpdateOrderStatusCommandHandler via MediatR
        var result = await mediator.Send(new UpdateOrderStatusCommand(@event.OrderId, @event.IsSuccess));

        if (result)
        {
            _logger.LogInformation(
                "Order {OrderId} status updated. PaymentSuccess={Success}",
                @event.OrderId, @event.IsSuccess);
        }
        else
        {
            _logger.LogWarning(
                "Order {OrderId} not found or already processed when handling PaymentCompletedEvent.",
                @event.OrderId);
        }
    }
}
