namespace Payments.Api.Domain;

public sealed class Payment
{
    public string Id { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string OperationReference { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int AmountCents { get; set; }
    public int TenderedCents { get; set; }
    public int ChangeCents { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public static class PaymentStatuses
{
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Mock = "Mock";
}
