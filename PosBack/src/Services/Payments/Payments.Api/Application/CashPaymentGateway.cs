using Payments.Api.Domain;

namespace Payments.Api.Application;

public sealed class CashPaymentGateway : IPaymentGateway
{
    public Task<Payment> ProcessAsync(Payment payment, CancellationToken cancellationToken)
    {
        payment.Status = payment.TenderedCents >= payment.AmountCents
            ? PaymentStatuses.Completed
            : PaymentStatuses.Failed;
        payment.ChangeCents = payment.Status == PaymentStatuses.Completed
            ? payment.TenderedCents - payment.AmountCents
            : 0;
        payment.ProviderReference = $"CASH-{Guid.NewGuid():N}";
        return Task.FromResult(payment);
    }
}
