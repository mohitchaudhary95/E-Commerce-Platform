using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Application.Features.Commands;
using ECommerce.Cart.Application.Features.Queries;
using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Domain.Entities;
using ECommerce.Shared.Common.Exceptions;
using MediatR;

using CartEntity = ECommerce.Cart.Domain.Entities.Cart;

namespace ECommerce.Cart.Application.Features.Handlers;

// ─── Remove Item ──────────────────────────────────────────────────────────────

public class RemoveCartItemCommandHandler
    : IRequestHandler<RemoveCartItemCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;

    public RemoveCartItemCommandHandler(ICartRepository cartRepository)
        => _cartRepository = cartRepository;

    public async Task<CartDto> Handle(
        RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        // EF detects removal — generates DELETE on SaveChanges
        cart.Items.Remove(item);

        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return CartMapper.MapToDto(cart);
    }
}

// ─── Update Quantity ──────────────────────────────────────────────────────────

public class UpdateQuantityCommandHandler
    : IRequestHandler<UpdateQuantityCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;

    public UpdateQuantityCommandHandler(ICartRepository cartRepository)
        => _cartRepository = cartRepository;

    public async Task<CartDto> Handle(
        UpdateQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        if (request.Dto.Quantity <= 0)
            cart.Items.Remove(item);   // 0 qty = remove
        else
            item.Quantity = request.Dto.Quantity;

        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return CartMapper.MapToDto(cart);
    }
}

// ─── Clear Cart ───────────────────────────────────────────────────────────────

public class ClearCartCommandHandler
    : IRequestHandler<ClearCartCommand, bool>
{
    private readonly ICartRepository _cartRepository;

    public ClearCartCommandHandler(ICartRepository cartRepository)
        => _cartRepository = cartRepository;

    public async Task<bool> Handle(
        ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        if (cart == null) return true; // already empty

        cart.Items.Clear();
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return true;
    }
}

// ─── Get User Cart ────────────────────────────────────────────────────────────

public class GetUserCartQueryHandler
    : IRequestHandler<GetUserCartQuery, CartDto>
{
    private readonly ICartRepository _cartRepository;

    public GetUserCartQueryHandler(ICartRepository cartRepository)
        => _cartRepository = cartRepository;

    public async Task<CartDto> Handle(
        GetUserCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        if (cart == null)
        {
            // Return empty cart shape — not a 404
            return new CartDto
            {
                UserId      = request.UserId,
                Items       = new List<CartItemDto>(),
                TotalAmount = 0,
                TotalItems  = 0
            };
        }

        return CartMapper.MapToDto(cart);
    }
}

// ─── Shared mapper (file-scoped = private to this file) ──────────────────────

file static class CartMapper
{
    public static CartDto MapToDto(CartEntity cart) => new()
    {
        Id          = cart.Id,
        UserId      = cart.UserId,
        TotalAmount = cart.TotalAmount,
        TotalItems  = cart.TotalItems,
        UpdatedAt   = cart.UpdatedAt,
        Items = cart.Items.Select(i => new CartItemDto
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