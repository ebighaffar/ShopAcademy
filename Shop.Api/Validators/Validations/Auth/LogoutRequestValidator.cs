using FluentValidation;
using Shop.Api.Contracts.Auth;

namespace Shop.Api.Validators.Validations.Auth;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty().MaximumLength(200);
    }
}
