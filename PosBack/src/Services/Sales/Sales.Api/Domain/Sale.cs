namespace Sales.Api.Domain;

public sealed class Sale
{
    public string Id { get; set; } = string.Empty;
    public string SaleNumber { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string CashierId { get; set; } = string.Empty;
    public DateTimeOffset SaleDate { get; set; }
    public List<SaleItem> Items { get; set; } = [];
    public int SubtotalCents { get; set; }
    public int TaxTotalCents { get; set; }
    public int GrandTotalCents { get; set; }
    public string PaymentStatus { get; set; } = SalePaymentStatuses.Pending;
    public string Status { get; set; } = SaleStatuses.Pending;
    public string PaymentId { get; set; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class SaleItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int UnitPriceCents { get; set; }
    public decimal TaxRate { get; set; }
    public int Quantity { get; set; }
    public int LineTotalCents { get; set; }
    public int TaxCents { get; set; }
}

public static class SaleStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";
}

public static class SalePaymentStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Failed = "Failed";
    public const string Refunded = "Refunded";
}
