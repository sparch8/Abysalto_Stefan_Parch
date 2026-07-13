using Abysalto.StefanParch.Api.Data;
using Abysalto.StefanParch.Api.Interfaces;
using Abysalto.StefanParch.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Abysalto.StefanParch.Api.Repositories;

public sealed class CartRepository(ApplicationDbContext dbContext) : ICartRepository
{
    public Task<Cart?> GetCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return dbContext.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(cart => cart.Id == cartId, cancellationToken);
    }

    public Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Carts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
    }

    public Task<CartItem?> GetCartItemAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CartItems
            .SingleOrDefaultAsync(
                item => item.CartId == cartId && item.ProductId == productId,
                cancellationToken);
    }

    public async Task<Cart> CreateCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await dbContext.Carts.AddAsync(cart, cancellationToken);

        return cart;
    }

    public async Task<CartItem> AddItemAsync(
        Guid cartId,
        Guid productId,
        int quantity,
        decimal unitPrice,
        CancellationToken cancellationToken = default)
    {
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        await dbContext.CartItems.AddAsync(cartItem, cancellationToken);

        return cartItem;
    }

    public async Task<bool> RemoveItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cartItem = await dbContext.CartItems
            .SingleOrDefaultAsync(item => item.Id == cartItemId, cancellationToken);

        if (cartItem is null)
        {
            return false;
        }

        dbContext.CartItems.Remove(cartItem);

        return true;
    }

    public async Task<bool> UpdateQuantityAsync(
        Guid cartItemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var cartItem = await dbContext.CartItems
            .SingleOrDefaultAsync(item => item.Id == cartItemId, cancellationToken);

        if (cartItem is null)
        {
            return false;
        }

        cartItem.Quantity = quantity;

        return true;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
