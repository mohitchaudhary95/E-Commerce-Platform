using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ECommerce.Cart.Infrastructure.HttpClients
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true  // Handles camelCase from API ? PascalCase DTO
        };

        public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            try
            {
                // GET http://product-service/api/products/{id}
                var response = await _httpClient.GetAsync($"api/products/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "ProductService returned {StatusCode} for product {ProductId}",
                        response.StatusCode, productId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                // Deserialize the ApiResponse<ProductResponseDto> wrapper
                var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<ProductResponseDto>>(content, JsonOptions);

                if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                {
                    _logger.LogWarning("ProductService returned unsuccessful response for product {ProductId}", productId);
                    return null;
                }

                return apiResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                // ProductService is down or unreachable
                _logger.LogError(ex, "Failed to reach ProductService for product {ProductId}", productId);
                return null;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Request timed out (not user cancellation)
                _logger.LogError(ex, "Request to ProductService timed out for product {ProductId}", productId);
                return null;
            }
        }
    }
}
