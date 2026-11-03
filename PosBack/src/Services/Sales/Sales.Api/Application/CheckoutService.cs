using System.Security.Claims;
using Sales.Api.Domain;
using Sales.Api.Dtos;
using SharedKernel.Api;
using SharedKernel.Persistence;

namespace Sales.Api.Application;

public static class CheckoutCalculator
{
    public static (int SubtotalCents, int TaxCents, int GrandTotalCents) Calculate(
        IReadOnlyList<(int UnitPriceCents, decimal TaxRate, int Quantity)> items)
    {
        var subtotal = items.Sum(item => checked(item.UnitPriceCents * item.Quantity));
        var tax = items.Sum(item => checked((int)Math.Round(
            checked(item.UnitPriceCents * item.Quantity) * item.TaxRate,
            MidpointRounding.AwayFromZero)));
        return (subtotal, tax, checked(subtotal + tax));
    }
}

public sealed class CheckoutService
{
    private static readonly SemaphoreSlim CheckoutLock = new(1, 1);
    private readonly IJsonRepository<Sale> _repository;
    private readonly ProductClient _products;
    private readonly InventoryClient _inventory;
    private readonly PaymentClient _payments;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CheckoutService(IJsonRepository<Sale> repository, ProductClient products, InventoryClient inventory,
        PaymentClient payments, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _products = products;
        _inventory = inventory;
        _payments = payments;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CheckoutResponse> CheckoutAsync(string idempotencyKey, CheckoutRequest request, CancellationToken cancellationToken)
    {
        await CheckoutLock.WaitAsync(cancellationToken);
        try
        {
            var duplicate = await _repository.GetAllAsync(cancellationToken);
            var existing = duplicate.FirstOrDefault(sale => sale.IdempotencyKey == idempotencyKey);
            if (existing is not null && existing.Status == SaleStatuses.Completed)
                return BuildReceipt(existing, request.TenderedCents);

            if (request.Items.Count == 0)
                throw new ValidationException("empty_cart", "The checkout cart cannot be empty.");
            var saleItems = new List<SaleItem>();
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new ValidationException("invalid_quantity", $"Quantity for product '{item.ProductId}' must be positive.");
                var product = await _products.GetAsync(item.ProductId, cancellationToken);
                if (!product.IsActive)
                    throw new ValidationException("inactive_product", $"Product '{product.Id}' is inactive.");
                var stock = await _inventory.GetAsync(product.Id, cancellationToken);
                if (stock.QuantityOnHand < item.Quantity)
                    throw new ValidationException("insufficient_stock", $"Insufficient stock for product '{product.Name}'.");
                var unitPrice = checked((int)Math.Round(product.Price, MidpointRounding.AwayFromZero));
                var lineSubtotal = checked(unitPrice * item.Quantity);
                var tax = checked((int)Math.Round(lineSubtotal * product.TaxRate, MidpointRounding.AwayFromZero));
                saleItems.Add(new SaleItem
                {
                    ProductId = product.Id, ProductName = product.Name, Sku = product.Sku,
                    UnitPriceCents = unitPrice, TaxRate = product.TaxRate, Quantity = item.Quantity,
                    LineTotalCents = lineSubtotal, TaxCents = tax
                });
            }

            var subtotal = saleItems.Sum(item => item.LineTotalCents);
            var taxTotal = saleItems.Sum(item => item.TaxCents);
            var grandTotal = checked(subtotal + taxTotal);
            var cashierId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            var sale = existing ?? new Sale
            {
                Id = Guid.NewGuid().ToString("N"), SaleNumber = $"S-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant(),
                IdempotencyKey = idempotencyKey, CashierId = cashierId, SaleDate = DateTimeOffset.UtcNow,
            };
            sale.Items = saleItems;
            sale.SubtotalCents = subtotal;
            sale.TaxTotalCents = taxTotal;
            sale.GrandTotalCents = grandTotal;
            if (existing is null)
                await _repository.AddAsync(sale, candidate => candidate.Id, cancellationToken);

            try
            {
                var payment = await _payments.ProcessAsync(idempotencyKey, new
                {
                    OperationReference = sale.Id, Method = request.PaymentMethod, AmountCents = grandTotal,
                    TenderedCents = request.TenderedCents
                }, cancellationToken);
                sale.PaymentId = payment.Id;
                sale.PaymentStatus = payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                                     payment.Status.Equals("Mock", StringComparison.OrdinalIgnoreCase)
                    ? SalePaymentStatuses.Paid : SalePaymentStatuses.Failed;
                sale.Status = SaleStatuses.Processing;
                await _repository.UpdateAsync(sale.Id, sale, candidate => candidate.Id, cancellationToken);
            }
            catch
            {
                sale.Status = SaleStatuses.Failed;
                sale.PaymentStatus = SalePaymentStatuses.Failed;
                await _repository.UpdateAsync(sale.Id, sale, candidate => candidate.Id, cancellationToken);
                throw;
            }

            try
            {
                foreach (var item in saleItems)
                    await _inventory.AdjustAsync(item.ProductId, idempotencyKey, item.Quantity, cancellationToken);
                sale.Status = SaleStatuses.Completed;
                sale.CompletedAt = DateTimeOffset.UtcNow;
                await _repository.UpdateAsync(sale.Id, sale, candidate => candidate.Id, cancellationToken);
            }
            catch
            {
                sale.Status = SaleStatuses.Failed;
                await _repository.UpdateAsync(sale.Id, sale, candidate => candidate.Id, cancellationToken);
                throw;
            }
            return BuildReceipt(sale, request.TenderedCents);
        }
        finally { CheckoutLock.Release(); }
    }

    public async Task<IReadOnlyList<SaleResponse>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var sales = await _repository.GetAllAsync(cancellationToken);
        return sales.OrderByDescending(sale => sale.SaleDate).Select(ToResponse).ToList();
    }

    public async Task<SaleResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("sale_not_found", "Sale was not found.");
        return ToResponse(sale);
    }

    private static CheckoutResponse BuildReceipt(Sale sale, int tenderedCents)
    {
        var response = ToResponse(sale);
        var receipt = new ReceiptResponse(sale.SaleNumber, sale.SaleDate, sale.CashierId, response.Items,
            sale.SubtotalCents, sale.TaxTotalCents, sale.GrandTotalCents, tenderedCents,
            Math.Max(0, tenderedCents - sale.GrandTotalCents));
        return new CheckoutResponse(response, receipt);
    }

    private static SaleResponse ToResponse(Sale sale) =>
        new(sale.Id, sale.SaleNumber, sale.CashierId, sale.SaleDate,
            sale.Items.Select(item => new SaleItemResponse(item.ProductId, item.ProductName, item.Sku, item.UnitPriceCents,
                item.TaxRate, item.Quantity, item.LineTotalCents, item.TaxCents)).ToList(),
            sale.SubtotalCents, sale.TaxTotalCents, sale.GrandTotalCents, sale.PaymentStatus, sale.Status,
            sale.PaymentId, sale.CompletedAt);
}
