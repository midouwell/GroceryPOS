namespace Inventory.Api.Domain;

public sealed class StockItem
{
    public string ProductId { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class StockMovement
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int QuantityDelta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public static class StockMovementTypes
{
    public const string InitialStock = "InitialStock";
    public const string Restock = "Restock";
    public const string Sale = "Sale";
    public const string Adjustment = "Adjustment";
    public const string Return = "Return";
    public const string Correction = "Correction";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        InitialStock, Restock, Sale, Adjustment, Return, Correction
    };
}
