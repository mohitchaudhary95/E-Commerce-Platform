
using System.Text;
using ECommerce.Cart.Application.Features.Commands;
using ECommerce.Cart.Application.Features.Validators;
using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Infrastructure.HttpClients;
using ECommerce.Cart.Infrastructure.Persistence;
using ECommerce.Cart.Infrastructure.Repositories;
using ECommerce.Shared.Common.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

namespace ECommerce.Cart.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── Serilog ──────────────────────────────────────────────────────────────────
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            builder.Host.UseSerilog();

            // ─── Database ─────────────────────────────────────────────────────────────────
            builder.Services.AddDbContext<CartDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CartDb")));

            // ─── MediatR + Validation Pipeline ───────────────────────────────────────────
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AddToCartCommand).Assembly));

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddValidatorsFromAssemblyContaining<AddToCartValidator>();

            // ─── Repositories ─────────────────────────────────────────────────────────────
            builder.Services.AddScoped<ICartRepository, CartRepository>();

            // ─── HTTP Client for ProductService ───────────────────────────────────────────
            // IHttpClientFactory pattern — managed, pooled HTTP connections.
            // BaseAddress comes from config so it's easy to change for Docker/production.
            builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductService"]!);
                client.Timeout = TimeSpan.FromSeconds(10); // Don't wait forever if ProductService is slow
            });

            // ─── JWT Authentication ───────────────────────────────────────────────────────
            var jwtKey = builder.Configuration["JwtSettings:SecretKey"]!;
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
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

            builder.Services.AddAuthorization();

            // ─── Swagger ──────────────────────────────────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart Service API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.RoutePrefix = string.Empty);
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CartDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
