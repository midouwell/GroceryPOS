namespace Sales.Api.Dtos;

public sealed record CheckoutItemRequest(string ProductId, int Quantity);
public sealed record CheckoutRequest(IReadOnlyList<CheckoutItemRequest> Items, string PaymentMethod, int TenderedCents = 0);
public sealed record SaleItemResponse(string ProductId, string ProductName, string Sku, int UnitPriceCents,
    decimal TaxRate, int Quantity, int LineTotalCents, int TaxCents);
public sealed record SaleResponse(string Id, string SaleNumber, string CashierId, DateTimeOffset SaleDate,
    IReadOnlyList<SaleItemResponse> Items, int SubtotalCents, int TaxTotalCents, int GrandTotalCents,
    string PaymentStatus, string Status, string PaymentId, string PaymentMethod, DateTimeOffset? CompletedAt);
public sealed record CheckoutResponse(SaleResponse Sale, ReceiptResponse Receipt);
public sealed record ReceiptResponse(string ReceiptNumber, DateTimeOffset IssuedAt, string CashierId,
    IReadOnlyList<SaleItemResponse> Items, int SubtotalCents, int TaxTotalCents, int GrandTotalCents,
    int TenderedCents, int ChangeCents);
public sealed record PaymentResult(string Id, string Status, int ChangeCents);
