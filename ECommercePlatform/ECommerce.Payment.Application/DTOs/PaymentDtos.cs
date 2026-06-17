using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusLabel => Status.ToString();
    public string? FailureReason { get; set; }
    public string? CardLastFour { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Internal DTO — used by OrderCreatedConsumer to trigger payment processing.
/// Not exposed via HTTP API (payment is triggered by event, not REST call).
/// </summary>
public class ProcessPaymentDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
