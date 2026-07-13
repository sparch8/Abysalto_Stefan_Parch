using Abysalto.StefanParch.Api.DTOs;
using FluentValidation;

namespace Abysalto.StefanParch.Api.DTOs.Validators;

public sealed class CreateCartRequestValidator : AbstractValidator<CreateCartRequest>
{
    public CreateCartRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();
    }
}
