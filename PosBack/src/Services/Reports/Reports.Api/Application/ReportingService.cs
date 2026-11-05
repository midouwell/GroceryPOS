using System.Net.Http.Headers;
using System.Net.Http.Json;
using Reports.Api.Dtos;

namespace Reports.Api.Application;

public sealed class ReportingService
{
    private readonly HttpClient _salesClient;
    private readonly HttpClient _inventoryClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReportingService(HttpClient salesClient, HttpClient inventoryClient, IHttpContextAccessor httpContextAccessor)
    {
        _salesClient = salesClient;
        _inventoryClient = inventoryClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SalesReport> GetAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        ForwardAuthorization(_salesClient);
        var sales = await GetCompletedSalesAsync(cancellationToken);
        sales = sales.Where(sale => sale.SaleDate >= from && sale.SaleDate <= to).ToList();
        var revenue = sales.Sum(sale => sale.GrandTotalCents);
        return new SalesReport(from, to, sales.Count, revenue,
            sales.Count == 0 ? 0 : revenue / sales.Count, sales);
    }

    public async Task<DailySalesSummary> GetDailyAsync(DateOnly? date, CancellationToken cancellationToken)
    {
        var target = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = new DateTimeOffset(target.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var report = await GetAsync(start, start.AddDays(1).AddTicks(-1), cancellationToken);
        return new DailySalesSummary(target, report.TransactionCount, report.RevenueCents, report.AverageTransactionCents);
    }

    public async Task<IReadOnlyList<PaymentMethodSales>> GetByPaymentMethodAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        var sales = await GetCompletedSalesAsync(cancellationToken);
        sales = sales.Where(sale =>
            (!from.HasValue || sale.SaleDate >= from.Value) &&
            (!to.HasValue || sale.SaleDate <= to.Value)).ToList();
        return sales.GroupBy(sale => string.IsNullOrWhiteSpace(sale.PaymentMethod) ? "unknown" : sale.PaymentMethod)
            .Select(group => new PaymentMethodSales(group.Key, group.Count(), group.Sum(sale => sale.GrandTotalCents)))
            .OrderByDescending(item => item.RevenueCents).ToList();
    }

    public async Task<IReadOnlyList<BestSellingProduct>> GetBestSellingAsync(int top, CancellationToken cancellationToken)
    {
        ForwardAuthorization(_salesClient);
        var sales = await GetCompletedSalesAsync(cancellationToken);
        var grouped = new Dictionary<string, BestProductAccumulator>();
        foreach (var sale in sales)
        {
            var details = await _salesClient.GetFromJsonAsync<SaleDetails>($"api/v1/sales/{sale.Id}", cancellationToken: cancellationToken);
            foreach (var item in details?.Items ?? [])
            {
                if (!grouped.TryGetValue(item.ProductId, out var accumulator))
                    grouped[item.ProductId] = accumulator = new BestProductAccumulator(item.ProductId, item.ProductName, item.Sku);
                accumulator.Quantity += item.Quantity;
                accumulator.Revenue += item.LineTotalCents + item.TaxCents;
            }
        }
        return grouped.Values.Select(value => value.ToResponse())
            .OrderByDescending(item => item.QuantitySold).Take(Math.Clamp(top, 1, 100)).ToList();
    }

    public async Task<IReadOnlyList<LowStockResponse>> GetLowStockAsync(CancellationToken cancellationToken)
    {
        ForwardAuthorization(_inventoryClient);
        return await _inventoryClient.GetFromJsonAsync<IReadOnlyList<LowStockResponse>>("api/v1/inventory/low-stock", cancellationToken)
            ?? [];
    }

    private async Task<List<SaleSummary>> GetCompletedSalesAsync(CancellationToken cancellationToken)
    {
        ForwardAuthorization(_salesClient);
        var sales = await _salesClient.GetFromJsonAsync<List<SaleSummary>>("api/v1/sales", cancellationToken) ?? [];
        return sales.Where(sale => sale.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void ForwardAuthorization(HttpClient client)
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault() is { } authorization)
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorization);
    }

    private sealed class BestProductAccumulator
    {
        private readonly string _id;
        private readonly string _name;
        private readonly string _sku;
        public int Quantity;
        public int Revenue;
        public BestProductAccumulator(string id, string name, string sku) { _id = id; _name = name; _sku = sku; }
        public BestSellingProduct ToResponse() => new(_id, _name, _sku, Quantity, Revenue);
    }

    private sealed record SaleDetails(IReadOnlyList<SaleItemDetails> Items);
    private sealed record SaleItemDetails(string ProductId, string ProductName, string Sku, int Quantity, int LineTotalCents, int TaxCents);
}
