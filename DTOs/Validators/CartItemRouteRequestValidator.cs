using Abysalto.StefanParch.Api.DTOs;
using FluentValidation;

namespace Abysalto.StefanParch.Api.DTOs.Validators;

public sealed class CartItemRouteRequestValidator : AbstractValidator<CartItemRouteRequest>
{
    public CartItemRouteRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.ProductId)
            .NotEmpty();
    }
}
