namespace ECommerce.Notification.Application.DTOs;

/// <summary>
/// Internal DTO passed from consumers to email handlers.
/// Contains everything needed to compose and send an email.
/// </summary>
public class EmailDto
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}

public class OrderConfirmationEmailDto
{
    public string UserEmail { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderEmailItem> Items { get; set; } = new();
    public DateTime OrderDate { get; set; }
}

public class OrderEmailItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PaymentResultEmailDto
{
    public string UserEmail { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}
