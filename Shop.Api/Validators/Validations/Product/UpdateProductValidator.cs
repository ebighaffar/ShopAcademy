using FluentValidation;
using Shop.Api.Contracts.Product;
using Shop.Api.Validators.Messages;
using Shop.Domain.Constants;

namespace Shop.Api.Validators.Validations.Product;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage(ProductValidationMessages.NameRequiredMessage)
            .MaximumLength(ProductConstants.NameMaxLength)
            .WithMessage(ProductValidationMessages.NameMaxLengthMessage);

        RuleFor(request => request.Description)
            .MaximumLength(ProductConstants.DescriptionMaxLength)
            .WithMessage(ProductValidationMessages.DescriptionMaxLengthMessage)
            .When(request => request.Description is not null);

        RuleFor(request => request.CategoryId)
            .GreaterThan(0)
            .WithMessage(ProductValidationMessages.CategoryRequiredMessage);
    }
}
