using Abysalto.StefanParch.Api.Models;

namespace Abysalto.StefanParch.Api.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartItem?> GetCartItemAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Cart> CreateCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartItem> AddItemAsync(
        Guid cartId,
        Guid productId,
        int quantity,
        decimal unitPrice,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemAsync(Guid cartItemId, CancellationToken cancellationToken = default);

    Task<bool> UpdateQuantityAsync(
        Guid cartItemId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
