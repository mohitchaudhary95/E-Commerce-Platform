//using Microsoft.OpenApi.Models;
using ECommerce.Notification.Application.Features.Commands;
using ECommerce.Notification.Application.Interfaces;
using ECommerce.Notification.Infrastructure.RabbitMQ.Consumers;
using ECommerce.Notification.Infrastructure.Services;
using ECommerce.Shared.RabbitMQ.Settings;
using MediatR;
using Serilog;

/// <summary>
/// NotificationService has no API controllers â€” it's purely event-driven.
/// It starts, registers its two RabbitMQ consumers as BackgroundServices,
/// and just listens forever. That's it.
///
/// This is a perfectly valid microservice pattern:
/// some services expose HTTP APIs, others are pure consumers.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// â”€â”€â”€ MediatR â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SendOrderConfirmationEmailCommand).Assembly));

// â”€â”€â”€ Email Service â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Swap ConsoleEmailService â†’ SendGridEmailService for production
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();

// â”€â”€â”€ RabbitMQ â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Both consumers run as long-lived BackgroundServices
builder.Services.AddHostedService<OrderCreatedConsumer>();
builder.Services.AddHostedService<PaymentCompletedConsumer>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Minimal health check endpoint â€” useful for Docker/k8s liveness probes
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "NotificationService" }));

app.Run();

