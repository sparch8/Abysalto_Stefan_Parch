namespace Abysalto.StefanParch.Api.DTOs;

public sealed record CartTotalDto(
    Guid UserId,
    decimal Total);
