using Inventory.Api.Application;
using Inventory.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Api;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public sealed class InventoryController : ControllerBase
{
    private readonly InventoryService _service;

    public InventoryController(InventoryService service) => _service = service;

    [HttpGet("{productId}")]
    public Task<ActionResult<StockResponse>> Get(string productId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetAsync(productId, cancellationToken));

    [HttpGet("{productId}/movements")]
    public Task<ActionResult<IReadOnlyList<StockMovementResponse>>> GetMovements(string productId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetMovementsAsync(productId, cancellationToken));

    [HttpPost("{productId}/adjustments")]
    [Authorize(Roles = "Administrator,Manager,Cashier")]
    public Task<ActionResult<StockResponse>> Adjust(string productId, AdjustStockRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.AdjustAsync(productId, request, cancellationToken));

    [HttpGet("low-stock")]
    public Task<ActionResult<IReadOnlyList<StockResponse>>> GetLowStock(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByStatusAsync(false, cancellationToken));

    [HttpGet("out-of-stock")]
    public Task<ActionResult<IReadOnlyList<StockResponse>>> GetOutOfStock(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByStatusAsync(true, cancellationToken));

    private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (NotFoundException exception) { return NotFound(new ApiError(exception.Code, exception.Message)); }
        catch (ValidationException exception) { return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors)); }
    }
}
