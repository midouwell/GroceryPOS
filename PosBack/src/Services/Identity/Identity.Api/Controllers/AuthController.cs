using Identity.Api.Application;
using Identity.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Api;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IdentityService _service;

    public AuthController(IdentityService service) => _service = service;

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiError>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.LoginAsync(request, cancellationToken));
        }
        catch (ValidationException exception)
        {
            return Unauthorized(new ApiError(exception.Code, exception.Message, exception.Errors));
        }
    }
}
