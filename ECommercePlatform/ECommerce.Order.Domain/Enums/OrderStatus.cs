namespace ECommerce.Order.Domain.Enums;

/// <summary>
/// Order lifecycle states.
///
/// Flow:
///   Pending → Processing (payment received) → Completed (shipped/delivered)
///                       ↘ Cancelled (payment failed or user cancelled)
///
/// Once Completed or Cancelled, no further transitions allowed.
/// </summary>
public enum OrderStatus
{
    Pending,     // Order placed, awaiting payment
    Processing,  // Payment confirmed, being fulfilled
    Completed,   // Order delivered
    Cancelled    // Payment failed or manually cancelled
}
