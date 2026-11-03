using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Sales.Api.Dtos;
using SharedKernel.Api;

namespace Sales.Api.Application;

public sealed class ProductClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ProductResult> GetAsync(string id, CancellationToken cancellationToken)
    {
        ForwardAuthorization();
        var response = await _httpClient.GetAsync($"api/v1/products/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ValidationException("invalid_product", $"Product '{id}' was not found or is inactive.");
        return await response.Content.ReadFromJsonAsync<ProductResult>(cancellationToken: cancellationToken)
            ?? throw new ValidationException("invalid_product", $"Product '{id}' returned invalid data.");
    }

    private void ForwardAuthorization()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault() is { } authorization)
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorization);
    }
}

public sealed record ProductResult(string Id, string Sku, string Name, decimal Price, decimal TaxRate, bool IsActive);

public sealed class InventoryClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public InventoryClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) { _httpClient = httpClient; _httpContextAccessor = httpContextAccessor; }

    public async Task<StockResult> GetAsync(string id, CancellationToken cancellationToken)
    {
        ForwardAuthorization();
        var response = await _httpClient.GetAsync($"api/v1/inventory/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ValidationException("stock_not_found", $"Stock for product '{id}' was not found.");
        return await response.Content.ReadFromJsonAsync<StockResult>(cancellationToken: cancellationToken)
            ?? throw new ValidationException("stock_not_found", $"Stock for product '{id}' returned invalid data.");
    }

    public async Task AdjustAsync(string id, string idempotencyKey, int quantity, CancellationToken cancellationToken)
    {
        ForwardAuthorization();
        var request = new { QuantityDelta = quantity, Type = "Sale", Reason = $"checkout:{idempotencyKey}" };
        var response = await _httpClient.PostAsJsonAsync($"api/v1/inventory/{id}/adjustments", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ValidationException("inventory_update_failed", $"Inventory update failed for product '{id}'. {body}");
        }
    }

    private void ForwardAuthorization()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault() is { } authorization)
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorization);
    }
}

public sealed record StockResult(string ProductId, int QuantityOnHand, int ReorderLevel, DateTimeOffset UpdatedAt);

public sealed class PaymentClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public PaymentClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) { _httpClient = httpClient; _httpContextAccessor = httpContextAccessor; }

    public async Task<PaymentResult> ProcessAsync(string idempotencyKey, object request, CancellationToken cancellationToken)
    {
        ForwardAuthorization();
        _httpClient.DefaultRequestHeaders.Remove("Idempotency-Key");
        _httpClient.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);
        var response = await _httpClient.PostAsJsonAsync("api/v1/payments", request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ValidationException("payment_failed", $"Payment failed. {body}");
        return await response.Content.ReadFromJsonAsync<PaymentResult>(cancellationToken: cancellationToken)
            ?? throw new ValidationException("payment_failed", "Payment service returned invalid data.");
    }

    private void ForwardAuthorization()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault() is { } authorization)
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorization);
    }
}
