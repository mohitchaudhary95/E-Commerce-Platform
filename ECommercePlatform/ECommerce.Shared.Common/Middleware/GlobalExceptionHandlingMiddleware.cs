using ECommerce.Shared.Common.Exceptions;
using ECommerce.Shared.Common.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.Shared.Common.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Map each exception type to the right HTTP status + message
            var (statusCode, message, errors) = exception switch
            {
                NotFoundException ex => (HttpStatusCode.NotFound, ex.Message, new List<string>()),
                ValidationException ex => (HttpStatusCode.BadRequest, ex.Message, ex.Errors),
                BusinessRuleException ex => (HttpStatusCode.BadRequest, ex.Message, new List<string>()),
                ForbiddenException ex => (HttpStatusCode.Forbidden, ex.Message, new List<string>()),
                InsufficientStockException ex => (HttpStatusCode.BadRequest, ex.Message, new List<string>()),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access.", new List<string>()),

                // Catch-all — never expose internal details to the client
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", new List<string>())
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(message, (int)statusCode, errors);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
