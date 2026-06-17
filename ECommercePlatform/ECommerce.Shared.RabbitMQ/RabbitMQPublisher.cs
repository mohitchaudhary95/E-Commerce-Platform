using ECommerce.Shared.RabbitMQ.Abstractions;
using ECommerce.Shared.RabbitMQ.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ECommerce.Shared.RabbitMQ
{
    public class RabbitMQPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = settings.Value.Host,
                Port = settings.Value.Port,
                UserName = settings.Value.Username,
                Password = settings.Value.Password,
                VirtualHost = settings.Value.VirtualHost,

                // Auto-reconnect if connection drops
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("RabbitMQ connection established: {Host}:{Port}", settings.Value.Host, settings.Value.Port);
        }

        public Task PublishAsync<T>(T @event, string queueName, CancellationToken cancellationToken = default)
            where T : class
        {
            // Declare the queue (idempotent — safe to call every time)
            // durable: queue survives broker restart
            // exclusive: not tied to one connection
            // autoDelete: queue stays after consumers disconnect
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(json);

            // Mark messages as persistent so they survive a RabbitMQ restart
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",         // Default exchange
                routingKey: queueName,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published {EventType} to queue '{Queue}'", typeof(T).Name, queueName);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
