using FluentValidation;
using Shop.Api.Contracts.User;
using Shop.Api.Validators.Messages;

namespace Shop.Api.Validators.Validations.User;

public class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator()
    {
        RuleFor(request => request.FirstName)
            .NotEmpty()
            .WithMessage(UserValidationMessages.FirstNameRequiredMessage)
            .MaximumLength(100);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .WithMessage(UserValidationMessages.LastNameRequiredMessage)
            .MaximumLength(100);

        RuleFor(request => request.PhoneNumber)
            .NotEmpty()
            .WithMessage(UserValidationMessages.PhoneNumberRequiredMessage)
            .Matches(@"^09\d{9}$")
            .WithMessage(UserValidationMessages.PhoneNumberInvalidMessage);

        RuleFor(request => request.UserName)
            .NotEmpty()
            .WithMessage(UserValidationMessages.UserNameRequiredMessage)
            .MaximumLength(100);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage(UserValidationMessages.PasswordRequiredMessage)
            .MinimumLength(6)
            .WithMessage(UserValidationMessages.PasswordMinLengthMessage);
    }
}
