namespace Shop.Api.Validators.Messages;

public static class UserValidationMessages
{
    public const string FirstNameRequiredMessage = "نام اجباری می باشد.";
    public const string LastNameRequiredMessage = "نام خانوادگی اجباری می باشد.";
    public const string PhoneNumberRequiredMessage = "شماره تلفن اجباری می باشد.";
    public const string PhoneNumberInvalidMessage = "شماره تلفن نامعتبر می باشد.";
    public const string UserNameRequiredMessage = "نام کاربری اجباری می باشد.";
    public const string PasswordRequiredMessage = "کلمه عبور اجباری می باشد.";
    public const string PasswordMinLengthMessage = "کلمه عبور نمی تواند کمتر از 6 کاراکتر باشد.";
}
