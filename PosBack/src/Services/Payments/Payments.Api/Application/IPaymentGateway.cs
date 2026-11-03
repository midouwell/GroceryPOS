using Payments.Api.Domain;

namespace Payments.Api.Application;

public interface IPaymentGateway
{
    Task<Payment> ProcessAsync(Payment payment, CancellationToken cancellationToken);
}
