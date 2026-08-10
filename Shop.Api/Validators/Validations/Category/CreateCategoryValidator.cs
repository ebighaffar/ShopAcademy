using FluentValidation;
using Shop.Api.Contracts.Category;
using Shop.Api.Validators.Messages;
using Shop.Domain.Constants;

namespace Shop.Api.Validators.Validations.Category;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage(CategoryValidationMessages.NameRequiredMessage)
            .MaximumLength(CategoryConstants.NameMaxLength)
            .WithMessage(CategoryValidationMessages.NameMaxLengthMessage);

        RuleFor(request => request.Description)
            .MaximumLength(CategoryConstants.DescriptionMaxLength)
            .WithMessage(CategoryValidationMessages.DescriptionMaxLengthMessage)
            .When(request => request.Description is not null);
    }
}
