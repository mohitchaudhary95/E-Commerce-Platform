using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Application.Features.Commands;
using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Domain.Entities;
using ECommerce.Shared.Common.Exceptions;
using MediatR;

// Alias needed because the class name 'Cart' conflicts with the namespace 'Cart'
using CartEntity = ECommerce.Cart.Domain.Entities.Cart;

namespace ECommerce.Cart.Application.Features.Handlers;

/// <summary>
/// Handles adding a product to the cart.
///
/// This is where synchronous inter-service communication happens:
///   1. Call ProductService HTTP API to validate product exists + get current price
///   2. Find or create the user's cart
///   3. If product already in cart → increase quantity
///      If new product → add new CartItem with price snapshot
///   4. Save and return updated cart
///
/// Why call ProductService instead of trusting the client's price?
/// Security — never trust price from the frontend. Always fetch from the source of truth.
/// </summary>
public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductServiceClient _productServiceClient;

    public AddToCartCommandHandler(
        ICartRepository cartRepository,
        IProductServiceClient productServiceClient)
    {
        _cartRepository = cartRepository;
        _productServiceClient = productServiceClient;
    }

    public async Task<CartDto> Handle(
        AddToCartCommand request, CancellationToken cancellationToken)
    {
        // ── Step 1: Validate product via ProductService ───────────────────────
        var product = await _productServiceClient
            .GetProductByIdAsync(request.Dto.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Dto.ProductId);

        if (!product.IsActive)
            throw new BusinessRuleException(
                $"Product '{product.Name}' is no longer available.");

        // ── Step 2: Get existing cart ─────────────────────────────────────────
        // Variable named 'cart' (lowercase) — no collision with CartEntity alias
        var cart = await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        if (cart == null)
        {
            // ── New cart: create with the first item already inside ────────────
            // EF Core sets CartId on the CartItem automatically via
            // the navigation property — no need to set it manually here.
            cart = new CartEntity
            {
                UserId = request.UserId,
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductImageUrl = product.ImageUrl,
                        UnitPrice = product.Price,
                        Quantity = request.Dto.Quantity
                    }
                }
            };

            // One SaveChanges — saves the cart AND its item together
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            // ── Existing cart: EF change tracker already knows about it ────────
            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductId == request.Dto.ProductId);

            if (existingItem != null)
            {
                // Product already in cart → just increase quantity
                // EF automatically marks this as Modified
                existingItem.Quantity += request.Dto.Quantity;
            }
            else
            {
                // New product → add a new CartItem row
                // EF automatically marks this as Added
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = request.Dto.Quantity,
                    CartId = cart.Id
                });
            }

            // One SaveChanges — EF generates the correct SQL for each change
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return MapToDto(cart);
    }

    private static CartDto MapToDto(CartEntity cart) => new()
    {
        Id = cart.Id,
        UserId = cart.UserId,
        TotalAmount = cart.TotalAmount,
        TotalItems = cart.TotalItems,
        UpdatedAt = cart.UpdatedAt,
        Items = cart.Items.Select(i => new CartItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductImageUrl = i.ProductImageUrl,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            TotalPrice = i.TotalPrice
        }).ToList()
    };
}
