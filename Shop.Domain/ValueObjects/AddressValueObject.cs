using Shop.Domain.Exceptions;

namespace Shop.Domain.ValueObjects;

public sealed record AddressValueObject
{
    public string RawAddress { get; }
    public string PostalCode { get; }

    public AddressValueObject(string rawAddress, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(rawAddress) || string.IsNullOrWhiteSpace(postalCode))
            throw new InvalidAddressException();

        RawAddress = rawAddress;
        PostalCode = postalCode;
    }
}