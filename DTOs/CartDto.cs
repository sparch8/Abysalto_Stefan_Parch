namespace Abysalto.StefanParch.Api.DTOs;

public sealed record CartDto(
    Guid Id,
    Guid UserId,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<CartItemDto> Items,
    decimal Total);
