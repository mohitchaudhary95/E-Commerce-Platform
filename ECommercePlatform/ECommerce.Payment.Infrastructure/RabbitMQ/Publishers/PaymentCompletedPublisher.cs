using ECommerce.Payment.Application.Interfaces;
using ECommerce.Payment.Domain.Entities;
using ECommerce.Payment.Domain.Enums;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.RabbitMQ;
using ECommerce.Shared.RabbitMQ.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommerce.Payment.Infrastructure.RabbitMQ.Publishers;

public class PaymentCompletedPublisher : IPaymentEventPublisher
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<PaymentCompletedPublisher> _logger;

    public PaymentCompletedPublisher(IEventPublisher publisher, ILogger<PaymentCompletedPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishPaymentCompletedAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var @event = new PaymentCompletedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            UserEmail = payment.UserEmail,
            Amount = payment.Amount,
            IsSuccess = payment.Status == PaymentStatus.Success,
            FailureReason = payment.FailureReason,
            ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow
        };

        await _publisher.PublishAsync(@event, QueueNames.PaymentCompleted, cancellationToken);

        _logger.LogInformation(
            "Published PaymentCompletedEvent for Order {OrderId}: Success={IsSuccess}",
            payment.OrderId, @event.IsSuccess);
    }
}
