using Customers.Api.Application;
using Customers.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Api;

namespace Customers.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedCustomersResponse>> Get(
        [FromQuery] string? search, [FromQuery] bool? active, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetAsync(search, active, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    public Task<ActionResult<CustomerResponse>> GetById(string id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator,Manager,Cashier")]
    public Task<ActionResult<CustomerResponse>> Create(CustomerRequest request, CancellationToken cancellationToken) =>
        ExecuteCreatedAsync(() => _service.CreateAsync(request, cancellationToken));

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Manager,Cashier")]
    public Task<ActionResult<CustomerResponse>> Update(string id, CustomerRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, cancellationToken));

    [HttpPatch("{id}/active")]
    [Authorize(Roles = "Administrator,Manager")]
    public Task<ActionResult<CustomerResponse>> SetActive(string id, [FromQuery] bool active, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetActiveAsync(id, active, cancellationToken));

    private async Task<ActionResult<CustomerResponse>> ExecuteAsync(Func<Task<CustomerResponse>> action)
    {
        try { return Ok(await action()); }
        catch (NotFoundException exception) { return NotFound(new ApiError(exception.Code, exception.Message)); }
        catch (ValidationException exception) { return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors)); }
        catch (ConflictException exception) { return Conflict(new ApiError(exception.Code, exception.Message)); }
    }

    private async Task<ActionResult<CustomerResponse>> ExecuteCreatedAsync(Func<Task<CustomerResponse>> action)
    {
        try { return StatusCode(StatusCodes.Status201Created, await action()); }
        catch (ValidationException exception) { return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors)); }
        catch (ConflictException exception) { return Conflict(new ApiError(exception.Code, exception.Message)); }
    }
}
