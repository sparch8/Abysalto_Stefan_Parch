namespace Abysalto.StefanParch.Api.DTOs;

public sealed record CartCreationResult(
    CartDto Cart,
    bool Created);
