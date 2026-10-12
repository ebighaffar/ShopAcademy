using Shop.Domain.Enums;
using Shop.Domain.Exceptions;
using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class Payment
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ProviderReference { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    private Payment()
    {
    }

    // Created through Order.CreatePayment so the amount is always the order total.
    internal Payment(long orderId, Money amount, PaymentMethod method)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSucceeded(string? providerReference = null)
    {
        EnsurePending();

        Status = PaymentStatus.Succeeded;
        ProviderReference = providerReference;
        PaidAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed()
    {
        EnsurePending();

        Status = PaymentStatus.Failed;
    }

    private void EnsurePending()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidPaymentException($"Payment is already '{Status}'");
    }
}
