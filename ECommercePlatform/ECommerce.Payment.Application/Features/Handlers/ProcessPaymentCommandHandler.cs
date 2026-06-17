using ECommerce.Payment.Application.DTOs;
using ECommerce.Payment.Application.Features.Commands;
using ECommerce.Payment.Application.Interfaces;
using DomainPayment = ECommerce.Payment.Domain.Entities.Payment;
using ECommerce.Payment.Domain.Entities;
using ECommerce.Payment.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Payment.Application.Features.Handlers;

/// <summary>
/// Core payment processing handler.
///
/// In a real system, step 3 would call Stripe/Razorpay SDK.
/// Here we simulate it with a random success/failure (90% success rate)
/// so the full async chain works end-to-end without a real gateway.
///
/// Steps:
///   1. Check duplicate — don't process same order twice (idempotency)
///   2. Create Payment record in Pending state
///   3. Simulate gateway call (random success/failure)
///   4. Update payment status
///   5. Publish PaymentCompletedEvent → triggers OrderService + NotificationService
/// </summary>
public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentEventPublisher _eventPublisher;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentEventPublisher eventPublisher,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // ── Step 1: Idempotency check ─────────────────────────────────────────
        // If the same OrderCreatedEvent is delivered twice (RabbitMQ can do this),
        // we must not charge the customer twice.
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(dto.OrderId, cancellationToken);
        if (existingPayment != null)
        {
            _logger.LogWarning("Payment for Order {OrderId} already exists. Skipping duplicate processing.", dto.OrderId);
            return MapToDto(existingPayment);
        }

        // ── Step 2: Create payment record ─────────────────────────────────────
        var payment = new DomainPayment
        {
            OrderId = dto.OrderId,
            UserId = dto.UserId,
            UserEmail = dto.UserEmail,
            Amount = dto.Amount,
            Status = PaymentStatus.Pending
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        // ── Step 3: Simulate payment gateway ──────────────────────────────────
        // Real implementation: var result = await _stripeClient.ChargeAsync(dto.Amount, cardToken);
        var (success, transactionId, failureReason) = SimulateGateway(dto.Amount);

        // ── Step 4: Update payment status ─────────────────────────────────────
        payment.Status = success ? PaymentStatus.Success : PaymentStatus.Failed;
        payment.GatewayTransactionId = transactionId;
        payment.FailureReason = failureReason;
        payment.CardLastFour = "4242"; // Simulated card
        payment.ProcessedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} for Order {OrderId}: {Status}",
            payment.Id, payment.OrderId, payment.Status);

        // ── Step 5: Publish result — triggers OrderService and NotificationService
        await _eventPublisher.PublishPaymentCompletedAsync(payment, cancellationToken);

        return MapToDto(payment);
    }

    /// <summary>
    /// Simulates a payment gateway response.
    /// 90% success rate — realistic enough for demo purposes.
    /// Returns: (isSuccess, transactionId, failureReason)
    /// </summary>
    private static (bool Success, string? TransactionId, string? FailureReason) SimulateGateway(decimal amount)
    {
        // Simulate network delay (real gateways take 200-800ms)
        Thread.Sleep(Random.Shared.Next(100, 300));

        var successRoll = Random.Shared.Next(1, 11); // 1-10
        if (successRoll <= 9) // 90% success
        {
            return (true, $"TXN_{Guid.NewGuid():N}", null);
        }

        // 10% failure — simulate card declined
        var reasons = new[] { "Insufficient funds", "Card declined", "Expired card" };
        return (false, null, reasons[Random.Shared.Next(reasons.Length)]);
    }

    private static PaymentDto MapToDto(DomainPayment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        UserId = p.UserId,
        Amount = p.Amount,
        Status = p.Status,
        FailureReason = p.FailureReason,
        CardLastFour = p.CardLastFour,
        CreatedAt = p.CreatedAt,
        ProcessedAt = p.ProcessedAt
    };
}

