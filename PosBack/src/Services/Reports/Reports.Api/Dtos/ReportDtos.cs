namespace Reports.Api.Dtos;

public sealed record DailySalesSummary(DateOnly Date, int TransactionCount, int RevenueCents, int AverageTransactionCents);
public sealed record SalesReport(DateTimeOffset From, DateTimeOffset To, int TransactionCount, int RevenueCents,
    int AverageTransactionCents, IReadOnlyList<SaleSummary> Sales);
public sealed record SaleSummary(string Id, string SaleNumber, DateTimeOffset SaleDate, int GrandTotalCents,
    string PaymentMethod, string Status);
public sealed record PaymentMethodSales(string PaymentMethod, int TransactionCount, int RevenueCents);
public sealed record BestSellingProduct(string ProductId, string ProductName, string Sku, int QuantitySold, int RevenueCents);
public sealed record LowStockResponse(string ProductId, int QuantityOnHand, int ReorderLevel);
