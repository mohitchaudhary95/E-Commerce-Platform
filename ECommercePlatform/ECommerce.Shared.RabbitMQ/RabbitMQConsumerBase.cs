using ECommerce.Shared.RabbitMQ.Settings;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ECommerce.Shared.RabbitMQ
{
    public abstract class RabbitMQConsumerBase<TEvent> : BackgroundService
     where TEvent : class
    {
        private IConnection? _connection;
        private IModel? _channel;
        private readonly RabbitMQSettings _settings;
        private readonly ILogger _logger;

        private const int MaxRetries = 3;
        protected abstract string QueueName { get; }

        protected RabbitMQConsumerBase(IOptions<RabbitMQSettings> settings, ILogger logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Connect with retry — RabbitMQ might not be ready on startup in Docker
            await ConnectWithRetryAsync(stoppingToken);

            if (_channel == null) return;

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // prefetchCount: 1 means the consumer processes one message at a time
            // This prevents one slow consumer from being overwhelmed
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false, // Manual acknowledgment — we ack only after successful processing
                consumer: consumer);

            _logger.LogInformation("{ConsumerName} started listening on queue '{Queue}'",
                GetType().Name, QueueName);

            // Keep alive until cancellation requested (app shutdown)
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = Encoding.UTF8.GetString(args.Body.ToArray());

            try
            {
                var @event = JsonSerializer.Deserialize<TEvent>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (@event == null)
                {
                    _logger.LogWarning("Received null event on queue '{Queue}', skipping.", QueueName);
                    _channel!.BasicAck(args.DeliveryTag, multiple: false);
                    return;
                }

                await ProcessMessageAsync(@event);

                // Acknowledge — tells RabbitMQ to remove the message from the queue
                _channel!.BasicAck(args.DeliveryTag, multiple: false);

                _logger.LogInformation("Successfully processed {EventType} from queue '{Queue}'",
                    typeof(TEvent).Name, QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue '{Queue}'. Message: {Body}", QueueName, body);

                // requeue: false — don't requeue indefinitely (prevents infinite loops)
                // In production: configure Dead Letter Exchange to capture failed messages
                _channel!.BasicNack(args.DeliveryTag, multiple: false, requeue: false);
            }
        }

        /// <summary>
        /// Override this in each concrete consumer to implement your business logic.
        /// </summary>
        protected abstract Task ProcessMessageAsync(TEvent @event);

        private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
        {
            var retryCount = 0;
            while (retryCount < MaxRetries && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _settings.Host,
                        Port = _settings.Port,
                        UserName = _settings.Username,
                        Password = _settings.Password,
                        VirtualHost = _settings.VirtualHost,
                        DispatchConsumersAsync = true // Required for AsyncEventingBasicConsumer
                    };

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt} failed. Retrying in 5s...", retryCount);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }

            _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} attempts.", MaxRetries);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
