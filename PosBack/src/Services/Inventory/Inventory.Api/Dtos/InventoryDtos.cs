namespace Inventory.Api.Dtos;

public sealed record StockResponse(string ProductId, int QuantityOnHand, int ReorderLevel, DateTimeOffset UpdatedAt);
public sealed record AdjustStockRequest(int QuantityDelta, string Type, string? Reason, int? ReorderLevel);
public sealed record StockMovementResponse(string Id, string ProductId, string Type, int QuantityDelta, string Reason, DateTimeOffset CreatedAt);
