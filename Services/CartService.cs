using Abysalto.StefanParch.Api.DTOs;
using Abysalto.StefanParch.Api.Interfaces;
using Abysalto.StefanParch.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Abysalto.StefanParch.Api.Services;

public sealed class CartService(ICartRepository cartRepository) : ICartService
{
    public async Task<CartDto> GetOrCreateCartAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateTrackedCartAsync(userId, cancellationToken);

        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateQuantity(request.Quantity);
        ValidateUnitPrice(request.UnitPrice);

        var cart = await GetOrCreateTrackedCartAsync(userId, cancellationToken);
        var existingItem = await cartRepository.GetCartItemAsync(
            cart.Id,
            request.ProductId,
            cancellationToken);

        if (existingItem is null)
        {
            await cartRepository.AddItemAsync(
                cart.Id,
                request.ProductId,
                request.Quantity,
                request.UnitPrice,
                cancellationToken);
        }
        else
        {
            await cartRepository.UpdateQuantityAsync(
                existingItem.Id,
                checked(existingItem.Quantity + request.Quantity),
                cancellationToken);
        }

        await cartRepository.SaveChangesAsync(cancellationToken);

        var updatedCart = await cartRepository.GetCartAsync(cart.Id, cancellationToken);

        return MapToDto(updatedCart ?? cart);
    }

    public async Task<bool> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);

        if (cart is null || cart.Items.All(item => item.Id != cartItemId))
        {
            return false;
        }

        var removed = await cartRepository.RemoveItemAsync(cartItemId, cancellationToken);

        if (removed)
        {
            await cartRepository.SaveChangesAsync(cancellationToken);
        }

        return removed;
    }

    public async Task<CartDto?> UpdateQuantityAsync(
        Guid userId,
        Guid cartItemId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateQuantity(request.Quantity);

        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);

        if (cart is null || cart.Items.All(item => item.Id != cartItemId))
        {
            return null;
        }

        var updated = await cartRepository.UpdateQuantityAsync(
            cartItemId,
            request.Quantity,
            cancellationToken);

        if (!updated)
        {
            return null;
        }

        await cartRepository.SaveChangesAsync(cancellationToken);

        var updatedCart = await cartRepository.GetCartAsync(cart.Id, cancellationToken);

        return updatedCart is null ? null : MapToDto(updatedCart);
    }

    private async Task<Cart> GetOrCreateTrackedCartAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        var createdCart = await cartRepository.CreateCartAsync(userId, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return createdCart;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items
            .OrderBy(item => item.Id)
            .Select(MapToDto)
            .ToArray();

        return new CartDto(
            cart.Id,
            cart.UserId,
            cart.CreatedAt,
            items,
            items.Sum(item => item.LineTotal));
    }

    private static CartItemDto MapToDto(CartItem item)
    {
        var lineTotal = item.Quantity * item.UnitPrice;

        return new CartItemDto(
            item.Id,
            item.ProductId,
            item.Quantity,
            item.UnitPrice,
            lineTotal);
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than zero.");
        }
    }

    private static void ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ValidationException("Unit price cannot be negative.");
        }
    }
}
