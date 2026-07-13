using Abysalto.StefanParch.Api.DTOs;
using FluentValidation;

namespace Abysalto.StefanParch.Api.DTOs.Validators;

public sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(request => request.ProductId)
            .NotEmpty();

        RuleFor(request => request.Quantity)
            .GreaterThan(0);

        RuleFor(request => request.UnitPrice)
            .GreaterThan(0);
    }
}
