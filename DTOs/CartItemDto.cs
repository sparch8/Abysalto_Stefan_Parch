namespace Abysalto.StefanParch.Api.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
