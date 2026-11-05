using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reports.Api.Application;
using Reports.Api.Dtos;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Administrator,Manager")]
public sealed class ReportsController : ControllerBase
{
    private readonly ReportingService _service;
    public ReportsController(ReportingService service) => _service = service;

    [HttpGet("daily")]
    public Task<DailySalesSummary> Daily([FromQuery] DateOnly? date, CancellationToken cancellationToken) =>
        _service.GetDailyAsync(date, cancellationToken);

    [HttpGet("sales")]
    public Task<SalesReport> Sales([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken cancellationToken) =>
        _service.GetAsync(from, to, cancellationToken);

    [HttpGet("revenue")]
    public async Task<ActionResult<object>> Revenue([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken cancellationToken)
    {
        var report = await _service.GetAsync(from, to, cancellationToken);
        return Ok(new { revenueCents = report.RevenueCents });
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<object>> Transactions([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken cancellationToken)
    {
        var report = await _service.GetAsync(from, to, cancellationToken);
        return Ok(new { transactionCount = report.TransactionCount });
    }

    [HttpGet("average-transaction")]
    public async Task<ActionResult<object>> Average([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken cancellationToken)
    {
        var report = await _service.GetAsync(from, to, cancellationToken);
        return Ok(new { averageTransactionCents = report.AverageTransactionCents });
    }

    [HttpGet("by-payment-method")]
    public Task<IReadOnlyList<PaymentMethodSales>> ByPaymentMethod([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken cancellationToken) =>
        _service.GetByPaymentMethodAsync(from, to, cancellationToken);

    [HttpGet("best-selling-products")]
    public Task<IReadOnlyList<BestSellingProduct>> BestSelling([FromQuery] int top = 10, CancellationToken cancellationToken = default) =>
        _service.GetBestSellingAsync(top, cancellationToken);

    [HttpGet("low-stock")]
    public Task<IReadOnlyList<LowStockResponse>> LowStock(CancellationToken cancellationToken) =>
        _service.GetLowStockAsync(cancellationToken);
}
