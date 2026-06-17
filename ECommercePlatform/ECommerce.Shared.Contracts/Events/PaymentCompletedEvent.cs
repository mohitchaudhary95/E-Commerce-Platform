using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.Contracts.Events
{
    public class PaymentCompletedEvent
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsSuccess { get; set; }
        public string? FailureReason { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
