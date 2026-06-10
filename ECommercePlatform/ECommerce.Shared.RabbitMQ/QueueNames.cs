using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.RabbitMQ
{
    public static class QueueNames
    {
        public const string OrderCreated = "order-created";
        public const string PaymentCompleted = "payment-completed";
        public const string InventoryUpdated = "inventory-updated";
    }
}
