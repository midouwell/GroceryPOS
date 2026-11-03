namespace Products.Api.Dtos;

public sealed record ProductRequest(
    string Sku,
    string Barcode,
    string Name,
    string Description,
    string Category,
    string Unit,
    decimal Price,
    decimal TaxRate,
    bool IsActive = true);

public sealed record ProductResponse(
    string Id,
    string Sku,
    string Barcode,
    string Name,
    string Description,
    string Category,
    string Unit,
    decimal Price,
    decimal TaxRate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
