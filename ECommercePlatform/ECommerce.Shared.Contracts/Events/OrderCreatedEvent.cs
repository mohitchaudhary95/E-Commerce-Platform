using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.Contracts.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderCreatedItem> Items { get; set; } = new();
    }

    public class OrderCreatedItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
