using Microsoft.OpenApi.Models;
using ECommerce.Identity.Application.Features.Validators;
using System.Text;
using ECommerce.Identity.Application.Features.Commands;
using ECommerce.Identity.Infrastructure.Persistence;
using ECommerce.Identity.Infrastructure.Repositories;
using ECommerce.Identity.Infrastructure.Services;
using ECommerce.Shared.Common.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using static ECommerce.Identity.Application.Interfaces.IIdentityInterfaces;
using static ECommerce.Identity.Application.Features.Validators.IdentityValidators;
namespace ECommerce.Identity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── Serilog ─────────────────────────────────────────────────────────────────
            // Replaces the default ASP.NET logger. Writes structured JSON logs.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            // ─── Database ─────────────────────────────────────────────────────────────────
            builder.Services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

            // ─── MediatR ─────────────────────────────────────────────────────────────────
            // Scans this assembly for all IRequestHandler<,> implementations and registers them
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

            // ─── Validation Pipeline ──────────────────────────────────────────────────────
            // Every command goes through ValidationBehavior before reaching its handler
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

            // ─── Repositories ─────────────────────────────────────────────────────────────
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // ─── Services ─────────────────────────────────────────────────────────────────
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

            // ─── JWT Authentication ───────────────────────────────────────────────────────
            var jwtKey = builder.Configuration["JwtSettings:SecretKey"]!;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,             // Reject expired tokens
                    ValidateIssuerSigningKey = true,     // Reject tokens with wrong signature
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero            // No grace period — token expired = rejected
                };
            });

            builder.Services.AddAuthorization();

            // ─── Swagger ──────────────────────────────────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Identity Service API",
                    Version = "v1",
                    Description = "Handles user registration, login, JWT authentication, and role management."
                });

                // Add the "Authorize" button in Swagger UI so you can test protected endpoints
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
            });

            builder.Services.AddControllers();

            // ─── Build ────────────────────────────────────────────────────────────────────
            var app = builder.Build();

            // ─── Middleware Pipeline ──────────────────────────────────────────────────────
            // ORDER MATTERS — middleware runs top to bottom on request, bottom to top on response

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>(); // Must be first — catches all errors

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service v1");
                    c.RoutePrefix = string.Empty; // Swagger at root URL in dev
                });
            }

            app.UseSerilogRequestLogging(); // Logs every HTTP request with timing

            app.UseHttpsRedirection();
            app.UseAuthentication(); // Must come before UseAuthorization
            app.UseAuthorization();
            app.MapControllers();

            // ─── Auto-migrate on startup ──────────────────────────────────────────────────
            // Automatically applies pending EF migrations when the app starts.
            // Fine for dev/demo — in production you'd run migrations separately.
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}


