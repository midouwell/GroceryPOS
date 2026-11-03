using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Application;
using Products.Api.Dtos;
using SharedKernel.Api;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductService _service;

    public ProductsController(ProductService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? active,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetAsync(search, category, active, page, pageSize, cancellationToken));
        }
        catch (ValidationException exception)
        {
            return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors));
        }
    }

    [HttpGet("{id}")]
    public Task<ActionResult<ProductResponse>> GetById(string id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken));

    [HttpGet("barcode/{barcode}")]
    public Task<ActionResult<ProductResponse>> GetByBarcode(string barcode, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByBarcodeAsync(barcode, cancellationToken));

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(CancellationToken cancellationToken) =>
        Ok(await _service.GetCategoriesAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator,Manager")]
    public Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, cancellationToken), StatusCodes.Status201Created);

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Manager")]
    public Task<ActionResult<ProductResponse>> Update(string id, ProductRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, cancellationToken));

    [HttpPatch("{id}/active")]
    [Authorize(Roles = "Administrator,Manager")]
    public Task<ActionResult<ProductResponse>> SetActive(string id, [FromQuery] bool active, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetActiveAsync(id, active, cancellationToken));

    private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action, int createdStatus = StatusCodes.Status200OK)
    {
        try
        {
            var value = await action();
            return createdStatus == StatusCodes.Status201Created ? StatusCode(createdStatus, value) : Ok(value);
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ApiError(exception.Code, exception.Message));
        }
        catch (ValidationException exception)
        {
            return BadRequest(new ApiError(exception.Code, exception.Message, exception.Errors));
        }
        catch (ConflictException exception)
        {
            return Conflict(new ApiError(exception.Code, exception.Message));
        }
    }
}
