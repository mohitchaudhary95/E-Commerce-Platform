using ECommerce.Payment.Application.DTOs;
using ECommerce.Payment.Application.Features.Commands;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Payment.Infrastructure.RabbitMQ.Consumers;

/// <summary>
/// Entry point of the payment flow.
///
/// Full async chain from this service's perspective:
///   OrderService publishes OrderCreatedEvent
///     → THIS consumer receives it
///     → builds ProcessPaymentCommand
///     → MediatR routes to ProcessPaymentCommandHandler
///     → handler simulates payment, saves result
///     → publishes PaymentCompletedEvent
///       → OrderService consumer updates order status
///       → NotificationService consumer sends email
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
            "PaymentService received OrderCreatedEvent: OrderId={OrderId}, Amount={Amount:C}",
            @event.OrderId, @event.TotalAmount);

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new ProcessPaymentCommand(new ProcessPaymentDto
        {
            OrderId = @event.OrderId,
            UserId = @event.UserId,
            UserEmail = @event.UserEmail,
            Amount = @event.TotalAmount
        });

        await mediator.Send(command);
    }
}
