namespace Abysalto.StefanParch.Api.DTOs;

public sealed record AddCartItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);
