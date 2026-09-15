using FluentValidation;
using Shop.Api.Contracts.Auth;

namespace Shop.Api.Validators.Validations.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty().MaximumLength(200);
    }
}
