namespace Abysalto.StefanParch.Api.DTOs;

public sealed record CartItemRouteRequest(
    Guid UserId,
    Guid ProductId);
