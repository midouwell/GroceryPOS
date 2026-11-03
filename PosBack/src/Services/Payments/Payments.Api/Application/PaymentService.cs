using Payments.Api.Domain;
using Payments.Api.Dtos;
using SharedKernel.Api;
using SharedKernel.Persistence;

namespace Payments.Api.Application;

public sealed class PaymentService
{
    private readonly IJsonRepository<Payment> _repository;
    private readonly CashPaymentGateway _cashGateway;
    private readonly MockCardPaymentGateway _cardGateway;

    public PaymentService(IJsonRepository<Payment> repository, CashPaymentGateway cashGateway, MockCardPaymentGateway cardGateway)
    {
        _repository = repository;
        _cashGateway = cashGateway;
        _cardGateway = cardGateway;
    }

    public async Task<PaymentResponse?> FindAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        var existing = payments.FirstOrDefault(payment => payment.IdempotencyKey == idempotencyKey);
        return existing is null ? null : ToResponse(existing);
    }

    public async Task<PaymentResponse> ProcessAsync(PaymentRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        var existing = await FindAsync(idempotencyKey, cancellationToken);
        if (existing is not null) return existing;
        if (string.IsNullOrWhiteSpace(request.OperationReference))
            throw new ValidationException("invalid_operation_reference", "Operation reference is required.");
        if (request.AmountCents <= 0)
            throw new ValidationException("invalid_amount", "Payment amount must be positive.");

        var method = request.Method.Trim();
        if (!method.Equals("cash", StringComparison.OrdinalIgnoreCase) &&
            !method.Equals("card", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("invalid_payment_method", "Payment method must be cash or card.");

        var payment = new Payment
        {
            Id = Guid.NewGuid().ToString("N"),
            IdempotencyKey = idempotencyKey,
            OperationReference = request.OperationReference.Trim(),
            Method = method.ToLowerInvariant(),
            AmountCents = request.AmountCents,
            TenderedCents = request.TenderedCents,
            Status = PaymentStatuses.Failed,
            CreatedAt = DateTimeOffset.UtcNow
        };

        IPaymentGateway gateway = method.Equals("cash", StringComparison.OrdinalIgnoreCase)
            ? _cashGateway
            : _cardGateway;
        await gateway.ProcessAsync(payment, cancellationToken);
        await _repository.AddAsync(payment, candidate => candidate.Id, cancellationToken);

        if (payment.Status == PaymentStatuses.Failed)
            throw new ValidationException("insufficient_cash", "Amount tendered is less than the amount due.", new Dictionary<string, string[]> {
                ["tenderedCents"] = ["The tendered amount is insufficient."]
            });
        return ToResponse(payment);
    }

    public async Task<PaymentResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("payment_not_found", "Payment was not found.");
        return ToResponse(payment);
    }

    private static PaymentResponse ToResponse(Payment payment) =>
        new(payment.Id, payment.IdempotencyKey, payment.OperationReference, payment.Method, payment.AmountCents,
            payment.TenderedCents, payment.ChangeCents, payment.Status, payment.ProviderReference, payment.CreatedAt);
}
