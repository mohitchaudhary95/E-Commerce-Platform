using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.Contracts.Events
{
    public class InventoryUpdatedEvent
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public string UpdateReason { get; set; } = string.Empty; // "OrderFulfilled", "ManualAdjustment", etc.
        public DateTime UpdatedAt { get; set; }
    }
}
