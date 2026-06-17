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
/// Receives PaymentCompletedEvent → sends payment success OR failure email.
/// Routes to the appropriate command based on IsSuccess flag.
///
/// This is the last step in the async chain for the user-facing flow.
/// After this email, the user knows the full status of their order.
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
            "NotificationService received PaymentCompletedEvent: Order {OrderId}, Success={IsSuccess}",
            @event.OrderId, @event.IsSuccess);

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var dto = new PaymentResultEmailDto
        {
            UserEmail = @event.UserEmail,
            OrderId = @event.OrderId,
            Amount = @event.Amount,
            IsSuccess = @event.IsSuccess,
            FailureReason = @event.FailureReason,
            ProcessedAt = @event.ProcessedAt
        };

        // Route to correct email based on payment result
        if (@event.IsSuccess)
            await mediator.Send(new SendPaymentSuccessEmailCommand(dto));
        else
            await mediator.Send(new SendPaymentFailureEmailCommand(dto));
    }
}
