using System.ComponentModel.DataAnnotations;

namespace Abysalto.StefanParch.Api.DTOs;

public sealed record UpdateCartItemQuantityRequest(
    [Range(1, int.MaxValue)] int Quantity);
