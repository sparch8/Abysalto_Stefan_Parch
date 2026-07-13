using Abysalto.StefanParch.Api.DTOs;
using FluentValidation;

namespace Abysalto.StefanParch.Api.DTOs.Validators;

public sealed class UpdateCartItemQuantityRequestValidator : AbstractValidator<UpdateCartItemQuantityRequest>
{
    public UpdateCartItemQuantityRequestValidator()
    {
        RuleFor(request => request.Quantity)
            .GreaterThan(0);
    }
}
