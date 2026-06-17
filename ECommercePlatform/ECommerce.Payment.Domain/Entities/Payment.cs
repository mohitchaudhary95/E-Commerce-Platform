using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Domain.Entities;

/// <summary>
/// Records a payment attempt for an order.
///
/// Design: PaymentService stores its OWN payment record in PaymentDb.
/// It does NOT update OrderDb — that happens via PaymentCompletedEvent
/// consumed by OrderService. Each service owns its own data.
///
/// In production: CardLastFour, GatewayTransactionId would come from
/// a real payment gateway (Stripe, Razorpay). Here we simulate them.
/// </summary>
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }

    // Simulated gateway fields — in production these come from Stripe/Razorpay response
    public string? GatewayTransactionId { get; set; }
    public string? CardLastFour { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
