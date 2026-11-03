namespace Payments.Api.Dtos;

public sealed record PaymentRequest(string OperationReference, string Method, int AmountCents, int TenderedCents = 0);
public sealed record PaymentResponse(string Id, string IdempotencyKey, string OperationReference, string Method,
    int AmountCents, int TenderedCents, int ChangeCents, string Status, string ProviderReference, DateTimeOffset CreatedAt);
