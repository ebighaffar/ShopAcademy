using Shop.Domain.Constants;

namespace Shop.Api.Validators.Messages;

public static class ProductValidationMessages
{
    public const string NameRequiredMessage = "نام اجباری می باشد.";
    public static readonly string NameMaxLengthMessage = $"نام نمی تواند بیشتر از {ProductConstants.NameMaxLength} کاراکتر باشد.";
    public static readonly string DescriptionMaxLengthMessage = $"توضیحات نمی تواند بیشتر از {ProductConstants.DescriptionMaxLength} کاراکتر باشد.";
    public const string PriceAmountMessage = "قیمت نمی تواند منفی باشد.";
    public const string PriceCurrencyMessage = "واحد پول نامعتبر می باشد.";
    public const string CountMessage = "تعداد نمی تواند منفی باشد.";
    public const string CategoryRequiredMessage = "دسته بندی اجباری می باشد.";
}
