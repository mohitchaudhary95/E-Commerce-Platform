namespace ECommerce.Payment.Domain.Enums;

/// <summary>
/// Payment lifecycle states.
///
/// Flow:
///   Pending → Success (payment gateway accepted)
///           → Failed  (card declined, timeout, etc.)
///
/// Terminal states — once Success or Failed, no transitions allowed.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Success,
    Failed
}
