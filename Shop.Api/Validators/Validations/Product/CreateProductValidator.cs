using FluentValidation;
using Shop.Api.Contracts.Products;
using Shop.Api.Validators.Messages;
using Shop.Domain.Constants;

namespace Shop.Api.Validators.Validations.Product;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
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

        RuleFor(request => request.PriceAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ProductValidationMessages.PriceAmountMessage);

        RuleFor(request => request.PriceCurrency)
            .IsInEnum()
            .WithMessage(ProductValidationMessages.PriceCurrencyMessage);

        RuleFor(request => request.Count)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ProductValidationMessages.CountMessage);

        RuleFor(request => request.CategoryId)
            .GreaterThan(0)
            .WithMessage(ProductValidationMessages.CategoryRequiredMessage);
    }
}
