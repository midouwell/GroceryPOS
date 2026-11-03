using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Api.Application;
using Payments.Api.Dtos;
using SharedKernel.Api;

namespace Payments.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly PaymentService _service;

    public PaymentsController(PaymentService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Process(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        PaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ApiError("idempotency_key_required", "Idempotency-Key is required."));
        try { return Ok(await _service.ProcessAsync(request, idempotencyKey, cancellationToken)); }
        catch (ValidationException exception) { return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors)); }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentResponse>> Get(string id, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.GetAsync(id, cancellationToken)); }
        catch (NotFoundException exception) { return NotFound(new ApiError(exception.Code, exception.Message)); }
    }
}
