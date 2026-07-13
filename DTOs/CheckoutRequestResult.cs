namespace Abysalto.StefanParch.Api.DTOs;

public sealed record CheckoutRequestResult(
    Guid UserId,
    Guid CartId,
    decimal Total,
    DateTimeOffset RequestedAt);
