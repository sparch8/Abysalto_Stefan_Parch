using System.ComponentModel.DataAnnotations;

namespace Abysalto.StefanParch.Api.DTOs;

public sealed record AddCartItemRequest(
    [Required] Guid ProductId,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(typeof(decimal), "0.00", "79228162514264337593543950335")] decimal UnitPrice);
