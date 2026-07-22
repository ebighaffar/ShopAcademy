using Shop.Domain.Constants;

namespace Shop.Api.Validators.Messages;

public static class CategoryValidationMessages
{
    public const string NameRequiredMessage = "نام اجباری می باشد.";
    public static readonly string NameMaxLengthMessage = $"نام نمی تواند بیشتر از {CategoryConstants.NameMaxLength} کاراکتر باشد.";
    public static readonly string DescriptionMaxLengthMessage = $"توضیحات نمی تواند بیشتر از {CategoryConstants.DescriptionMaxLength} کاراکتر باشد.";
}