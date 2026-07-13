using Abysalto.StefanParch.Api.DTOs;
using Abysalto.StefanParch.Api.Interfaces;
using Abysalto.StefanParch.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Abysalto.StefanParch.Api.Services;

public sealed class CartService(
    ICartRepository cartRepository,
    ILogger<CartService> logger) : ICartService
{
    public async Task<CartCreationResult> CreateCartAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);

        if (cart is not null)
        {
            return new CartCreationResult(MapToDto(cart), Created: false);
        }

        var createdCart = await cartRepository.CreateCartAsync(userId, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Cart Created {CartId} for user {UserId}",
            createdCart.Id,
            userId);

        return new CartCreationResult(MapToDto(createdCart), Created: true);
    }

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
        var total = updatedCart is null ? CalculateTotal(cart.Items) : CalculateTotal(updatedCart.Items);

        logger.LogInformation(
            "Item Added {ProductId} to cart {CartId} for user {UserId}. Quantity {Quantity}, UnitPrice {UnitPrice}, CartTotal {CartTotal}",
            request.ProductId,
            cart.Id,
            userId,
            request.Quantity,
            request.UnitPrice,
            total);

        return MapToDto(updatedCart ?? cart);
    }

    public async Task<bool> RemoveItemAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);
        var cartItem = cart?.Items.SingleOrDefault(item => item.ProductId == productId);

        if (cart is null || cartItem is null)
        {
            return false;
        }

        var removed = await cartRepository.RemoveItemAsync(cartItem.Id, cancellationToken);

        if (removed)
        {
            await cartRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Item Removed {ProductId} from cart {CartId} for user {UserId}",
                productId,
                cart.Id,
                userId);
        }

        return removed;
    }

    public async Task<CartDto?> UpdateQuantityAsync(
        Guid userId,
        Guid productId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateQuantity(request.Quantity);

        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);
        var cartItem = cart?.Items.SingleOrDefault(item => item.ProductId == productId);

        if (cart is null || cartItem is null)
        {
            return null;
        }

        var updated = await cartRepository.UpdateQuantityAsync(
            cartItem.Id,
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

    public async Task<CartTotalDto?> GetTotalAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);

        if (cart is null)
        {
            return null;
        }

        return new CartTotalDto(userId, CalculateTotal(cart.Items));
    }

    public async Task<CheckoutRequestResult?> RequestCheckoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userId, cancellationToken);

        if (cart is null)
        {
            return null;
        }

        var total = CalculateTotal(cart.Items);
        var requestedAt = DateTimeOffset.UtcNow;

        logger.LogInformation(
            "Checkout Requested for cart {CartId} by user {UserId}. ItemCount {ItemCount}, CartTotal {CartTotal}",
            cart.Id,
            userId,
            cart.Items.Count,
            total);

        return new CheckoutRequestResult(
            userId,
            cart.Id,
            total,
            requestedAt);
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

        logger.LogInformation(
            "Cart Created {CartId} for user {UserId}",
            createdCart.Id,
            userId);

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
            CalculateTotal(items));
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

    private static decimal CalculateTotal(IEnumerable<CartItem> items)
    {
        return items.Sum(item => item.Quantity * item.UnitPrice);
    }

    private static decimal CalculateTotal(IEnumerable<CartItemDto> items)
    {
        return items.Sum(item => item.LineTotal);
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
        if (unitPrice <= 0)
        {
            throw new ValidationException("Unit price must be greater than zero.");
        }
    }
}
