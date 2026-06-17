using System.Text;
using ECommerce.Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ─── Load Ocelot config ───────────────────────────────────────────────────────
// AddJsonFile BEFORE AddOcelot — Ocelot reads from this file
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

// ─── JWT Authentication ───────────────────────────────────────────────────────
// The gateway validates JWT tokens ONCE here.
// Downstream services also validate (defense in depth),
// but the gateway is the first line of defence.
//
// "Bearer" key must match AuthenticationProviderKey in ocelot.json routes.
var jwtKey = builder.Configuration["JwtSettings:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// ─── CORS ─────────────────────────────────────────────────────────────────────
// Allow Angular frontend (localhost:4200) to call the gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",   // Angular dev server
                "http://localhost:5173"    // Vite (if used)
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─── Ocelot ───────────────────────────────────────────────────────────────────
builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x => x.WithDictionaryHandle()); // In-memory response caching

var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<RequestLoggingMiddleware>(); // Log every incoming request first
app.UseCors("AllowAngular");
app.UseAuthentication();

// Health check endpoint — not routed through Ocelot
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "API Gateway",
    Timestamp = DateTime.UtcNow,
    Routes = new[]
    {
        "Identity  → localhost:5001",
        "Product   → localhost:5002",
        "Cart      → localhost:5003",
        "Order     → localhost:5004",
        "Payment   → localhost:5005",
        "Inventory → localhost:5006"
    }
}));

// Ocelot must be LAST — it handles all other routes
await app.UseOcelot();

app.Run();
