using Abysalto.StefanParch.Api.DTOs;
using FluentValidation;

namespace Abysalto.StefanParch.Api.DTOs.Validators;

public sealed class UserCartRouteRequestValidator : AbstractValidator<UserCartRouteRequest>
{
    public UserCartRouteRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();
    }
}
