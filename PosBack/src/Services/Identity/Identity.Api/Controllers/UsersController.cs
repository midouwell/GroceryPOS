using Identity.Api.Application;
using Identity.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Api;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IdentityService _service;

    public UsersController(IdentityService service) => _service = service;

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (userId is null)
            return Unauthorized();

        var user = (await _service.GetAsync(cancellationToken)).FirstOrDefault(candidate => candidate.Id == userId);
        return user is null ? NotFound(new ApiError("user_not_found", "User was not found.")) : Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Me), null, user);
        }
        catch (DomainException exception)
        {
            var status = exception switch
            {
                ConflictException => StatusCodes.Status409Conflict,
                ValidationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            var errors = exception is ValidationException validation ? validation.Errors : null;
            return StatusCode(status, new ApiError(exception.Code, exception.Message, errors));
        }
    }
}
