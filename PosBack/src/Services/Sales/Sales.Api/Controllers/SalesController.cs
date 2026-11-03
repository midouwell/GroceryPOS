using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sales.Api.Application;
using Sales.Api.Dtos;
using SharedKernel.Api;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Authorize]
public sealed class SalesController : ControllerBase
{
    private readonly CheckoutService _service;
    public SalesController(CheckoutService service) => _service = service;

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CheckoutRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ApiError("idempotency_key_required", "Idempotency-Key is required."));
        try { return Ok(await _service.CheckoutAsync(idempotencyKey, request, cancellationToken)); }
        catch (ValidationException exception) { return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors)); }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleResponse>>> History(CancellationToken cancellationToken) =>
        Ok(await _service.GetHistoryAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleResponse>> Get(string id, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.GetAsync(id, cancellationToken)); }
        catch (NotFoundException exception) { return NotFound(new ApiError(exception.Code, exception.Message)); }
    }
}
