using Abysalto.StefanParch.Api.DTOs;

namespace Abysalto.StefanParch.Api.Interfaces;

public interface ICartService
{
    Task<CartCreationResult> CreateCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartDto> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartDto> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<CartDto?> UpdateQuantityAsync(
        Guid userId,
        Guid productId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken = default);

    Task<CartTotalDto?> GetTotalAsync(Guid userId, CancellationToken cancellationToken = default);
}
