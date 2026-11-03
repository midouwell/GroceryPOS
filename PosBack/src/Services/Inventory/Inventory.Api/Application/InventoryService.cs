using Inventory.Api.Domain;
using Inventory.Api.Dtos;
using SharedKernel.Api;
using SharedKernel.Persistence;

namespace Inventory.Api.Application;

public sealed class InventoryService
{
    private readonly IJsonRepository<StockItem> _stockRepository;
    private readonly IJsonRepository<StockMovement> _movementRepository;

    public InventoryService(IJsonRepository<StockItem> stockRepository, IJsonRepository<StockMovement> movementRepository)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
    }

    public async Task<StockResponse> GetAsync(string productId, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetByIdAsync(productId, item => item.ProductId, cancellationToken)
            ?? throw new NotFoundException("stock_not_found", "Stock for this product was not found.");
        return ToStockResponse(stock);
    }

    public async Task<IReadOnlyList<StockMovementResponse>> GetMovementsAsync(string productId, CancellationToken cancellationToken)
    {
        var movements = await _movementRepository.GetAllAsync(cancellationToken);
        return movements.Where(movement => movement.ProductId == productId)
            .OrderByDescending(movement => movement.CreatedAt)
            .Select(ToMovementResponse).ToList();
    }

    public async Task<StockResponse> AdjustAsync(string productId, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var operationReason = request.Reason?.Trim();
        if (!string.IsNullOrWhiteSpace(operationReason) && operationReason.StartsWith("checkout:", StringComparison.OrdinalIgnoreCase))
        {
            var movements = await _movementRepository.GetAllAsync(cancellationToken);
            var alreadyApplied = movements.Any(movement =>
                movement.ProductId == productId &&
                string.Equals(movement.Reason, operationReason, StringComparison.OrdinalIgnoreCase));
            if (alreadyApplied)
            {
                var existingStock = await _stockRepository.GetByIdAsync(productId, item => item.ProductId, cancellationToken)
                    ?? throw new NotFoundException("stock_not_found", "Stock for this product was not found.");
                return ToStockResponse(existingStock);
            }
        }
        var type = StockMovementTypes.All.SingleOrDefault(candidate => candidate.Equals(request.Type, StringComparison.OrdinalIgnoreCase))
            ?? throw new ValidationException("invalid_movement_type", "The stock movement type is invalid.");
        if (request.QuantityDelta == 0)
            throw new ValidationException("invalid_quantity", "Quantity delta cannot be zero.");
        if (request.ReorderLevel is < 0)
            throw new ValidationException("invalid_reorder_level", "Reorder level cannot be negative.");

        var expectedPositive = type is StockMovementTypes.InitialStock or StockMovementTypes.Restock or StockMovementTypes.Return;
        var saleQuantityIsPositive = type == StockMovementTypes.Sale && request.QuantityDelta > 0;
        if (expectedPositive && request.QuantityDelta < 0 || type == StockMovementTypes.Sale && !saleQuantityIsPositive)
            throw new ValidationException("invalid_quantity", $"Type {type} requires a compatible quantity delta.");

        var existing = await _stockRepository.GetByIdAsync(productId, item => item.ProductId, cancellationToken);
        var stock = existing is null
            ? new StockItem { ProductId = productId, QuantityOnHand = 0, ReorderLevel = request.ReorderLevel ?? 0, UpdatedAt = DateTimeOffset.UtcNow }
            : existing;

        var movementDelta = type == StockMovementTypes.Sale ? -request.QuantityDelta : request.QuantityDelta;
        var newQuantity = stock.QuantityOnHand + movementDelta;
        if (newQuantity < 0)
            throw new ValidationException("insufficient_stock", "The stock quantity cannot be negative.");
        if (request.ReorderLevel.HasValue)
            stock.ReorderLevel = request.ReorderLevel.Value;

        stock.QuantityOnHand = newQuantity;
        stock.UpdatedAt = DateTimeOffset.UtcNow;
        if (existing is null)
            await _stockRepository.AddAsync(stock, item => item.ProductId, cancellationToken);
        else
            await _stockRepository.UpdateAsync(productId, stock, item => item.ProductId, cancellationToken);

        await _movementRepository.AddAsync(new StockMovement
        {
            Id = Guid.NewGuid().ToString("N"),
            ProductId = productId,
            Type = type,
            QuantityDelta = movementDelta,
            Reason = request.Reason?.Trim() ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        }, movement => movement.Id, cancellationToken);
        return ToStockResponse(stock);
    }

    public async Task<IReadOnlyList<StockResponse>> GetByStatusAsync(bool outOfStock, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetAllAsync(cancellationToken);
        var filtered = outOfStock
            ? stock.Where(item => item.QuantityOnHand == 0)
            : stock.Where(item => item.QuantityOnHand <= item.ReorderLevel);
        return filtered.OrderBy(item => item.ProductId).Select(ToStockResponse).ToList();
    }

    private static StockResponse ToStockResponse(StockItem stock) =>
        new(stock.ProductId, stock.QuantityOnHand, stock.ReorderLevel, stock.UpdatedAt);

    private static StockMovementResponse ToMovementResponse(StockMovement movement) =>
        new(movement.Id, movement.ProductId, movement.Type, movement.QuantityDelta, movement.Reason, movement.CreatedAt);
}
