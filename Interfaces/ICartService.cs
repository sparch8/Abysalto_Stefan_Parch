using Abysalto.StefanParch.Api.DTOs;

namespace Abysalto.StefanParch.Api.Interfaces;

public interface ICartService
{
    Task<CartDto> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartDto> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        CancellationToken cancellationToken = default);

    Task<CartDto?> UpdateQuantityAsync(
        Guid userId,
        Guid cartItemId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken = default);
}
