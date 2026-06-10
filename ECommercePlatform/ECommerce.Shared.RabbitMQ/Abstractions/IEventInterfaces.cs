using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.RabbitMQ.Abstractions
{
    public interface IEventInterfaces
    {
        public interface IEventPublisher
        {
            Task PublishAsync<T>(T @event, string queueName, CancellationToken cancellationToken = default)
            where T : class;
        }
        public interface IEventConsumer
        {
            Task StartConsumingAsync(string queueName, CancellationToken cancellationToken);
        }

    }
}
