using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Application.Features.Commands;
using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Domain.Entities;
using ECommerce.Shared.Common.Exceptions;
using MediatR;
 
using CartEntity = ECommerce.Cart.Domain.Entities.Cart;
 
namespace ECommerce.Cart.Application.Features.Handlers;
 
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
        // Step 1: Validate product
        var product = await _productServiceClient
            .GetProductByIdAsync(request.Dto.ProductId, cancellationToken);
 
        if (product == null)
            throw new NotFoundException("Product", request.Dto.ProductId);
 
        if (!product.IsActive)
            throw new BusinessRuleException(
                $"Product '{product.Name}' is no longer available.");
 
        // Step 2: Get or create cart
        var cart = await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);
 
        if (cart == null)
        {
            // Brand new cart — use AddAsync which does a clean INSERT
            var newCart = new CartEntity
            {
                UserId    = request.UserId,
                UpdatedAt = DateTime.UtcNow,
                Items     = new List<CartItem>
                {
                    new CartItem
                    {
                        ProductId       = product.Id,
                        ProductName     = product.Name,
                        ProductImageUrl = product.ImageUrl,
                        UnitPrice       = product.Price,
                        Quantity        = request.Dto.Quantity
                    }
                }
            };
            await _cartRepository.AddAsync(newCart, cancellationToken);
 
            // Reload so Items have their DB-generated IDs
            cart = (await _cartRepository.GetByUserIdAsync(
                request.UserId, cancellationToken))!;
            return MapToDto(cart);
        }
 
        // Step 3: Existing cart — check if product already in cart
        var existingItem = cart.Items
            .FirstOrDefault(i => i.ProductId == request.Dto.ProductId);
 
        if (existingItem != null)
        {
            // Increase quantity using direct SQL UPDATE — no change tracker
            var newQty = existingItem.Quantity + request.Dto.Quantity;
            await _cartRepository.UpdateCartItemQuantityAsync(
                existingItem.Id, newQty, cancellationToken);
        }
        else
        {
            // Add new item using direct INSERT — no change tracker
            var newItem = new CartItem
            {
                Id              = Guid.NewGuid(),
                ProductId       = product.Id,
                ProductName     = product.Name,
                ProductImageUrl = product.ImageUrl,
                UnitPrice       = product.Price,
                Quantity        = request.Dto.Quantity,
                CartId          = cart.Id
            };
            await _cartRepository.AddCartItemAsync(newItem, cancellationToken);
        }
 
        // Update cart timestamp
        await _cartRepository.UpdateAsync(cart, cancellationToken);
 
        // Reload cart to return fresh data with correct totals
        var updatedCart = (await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken))!;
 
        return MapToDto(updatedCart);
    }
 
    private static CartDto MapToDto(CartEntity cart) => new()
    {
        Id          = cart.Id,
        UserId      = cart.UserId,
        TotalAmount = cart.TotalAmount,
        TotalItems  = cart.TotalItems,
        UpdatedAt   = cart.UpdatedAt,
        Items       = cart.Items.Select(i => new CartItemDto
        {
            Id              = i.Id,
            ProductId       = i.ProductId,
            ProductName     = i.ProductName,
            ProductImageUrl = i.ProductImageUrl,
            UnitPrice       = i.UnitPrice,
            Quantity        = i.Quantity,
            TotalPrice      = i.TotalPrice
        }).ToList()
    };
}