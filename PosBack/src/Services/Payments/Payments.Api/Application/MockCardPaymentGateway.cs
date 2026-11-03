using Payments.Api.Domain;

namespace Payments.Api.Application;

public sealed class MockCardPaymentGateway : IPaymentGateway
{
    public Task<Payment> ProcessAsync(Payment payment, CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatuses.Mock;
        payment.ProviderReference = "MOCK_CARD_TERMINAL_NOT_AUTHORIZED";
        return Task.FromResult(payment);
    }
}
