using ECommerce.Notification.Application.DTOs;
using ECommerce.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Infrastructure.Services;

/// <summary>
/// Development email service — logs beautifully formatted emails to console.
/// No SMTP server needed to test the full notification flow.
///
/// To swap for real email in production:
///   1. Install: dotnet add package SendGrid (or Mailkit for SMTP)
///   2. Create SendGridEmailService : IEmailService
///   3. In Program.cs change:
///      builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
///      to:
///      builder.Services.AddScoped<IEmailService, SendGridEmailService>();
///   4. Add API key to appsettings.json
///   Everything else stays the same — no handler changes needed.
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailDto email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "\n📧 EMAIL SENT\n" +
            "  To:      {To}\n" +
            "  Subject: {Subject}\n" +
            "  Body:    {Body}",
            email.To, email.Subject, email.Body);

        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationAsync(
        OrderConfirmationEmailDto dto, CancellationToken cancellationToken = default)
    {
        var itemLines = string.Join("\n", dto.Items.Select(i =>
            $"    • {i.ProductName} × {i.Quantity} @ ₹{i.UnitPrice:N2} = ₹{i.Quantity * i.UnitPrice:N2}"));

        _logger.LogInformation(
            "\n✅ ORDER CONFIRMATION EMAIL\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
            "  To:       {Email}\n" +
            "  Subject:  Order Confirmed — #{OrderId}\n" +
            "  Body:\n" +
            "    Thank you for your order!\n\n" +
            "    Order #{OrderId}\n" +
            "    Date: {Date}\n\n" +
            "    Items:\n{Items}\n\n" +
            "    Total: ₹{Total:N2}\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            dto.UserEmail, dto.OrderId, dto.OrderId,
            dto.OrderDate.ToString("dd MMM yyyy HH:mm"),
            itemLines, dto.TotalAmount);

        return Task.CompletedTask;
    }

    public Task SendPaymentSuccessAsync(
        PaymentResultEmailDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "\n💳 PAYMENT SUCCESS EMAIL\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
            "  To:       {Email}\n" +
            "  Subject:  Payment Confirmed — ₹{Amount:N2}\n" +
            "  Body:\n" +
            "    Your payment of ₹{Amount:N2} for Order #{OrderId} was successful!\n" +
            "    Processed at: {ProcessedAt}\n" +
            "    Your order is now being processed.\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            dto.UserEmail, dto.Amount, dto.Amount,
            dto.OrderId, dto.ProcessedAt.ToString("dd MMM yyyy HH:mm"));

        return Task.CompletedTask;
    }

    public Task SendPaymentFailureAsync(
        PaymentResultEmailDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "\n❌ PAYMENT FAILURE EMAIL\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
            "  To:       {Email}\n" +
            "  Subject:  Payment Failed for Order #{OrderId}\n" +
            "  Body:\n" +
            "    Unfortunately your payment for Order #{OrderId} failed.\n" +
            "    Reason: {Reason}\n" +
            "    Please try placing your order again.\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            dto.UserEmail, dto.OrderId, dto.OrderId,
            dto.FailureReason ?? "Unknown error");

        return Task.CompletedTask;
    }
}
