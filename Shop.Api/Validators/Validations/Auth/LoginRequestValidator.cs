using FluentValidation;
using Shop.Api.Contracts.Auth;

namespace Shop.Api.Validators.Validations.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Login).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Password).NotEmpty();
    }
}
